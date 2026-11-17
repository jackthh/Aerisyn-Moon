using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Google.Apis.Drive.v3;
using Google.Apis.Sheets.v4;
using UnityEngine;

namespace Aerisyn.DataConfigSheet.Editor
{
    /// <summary>
    /// Result of a live Pull: the PullReport plus Source Sheet facts for the Config Manifest
    /// (null Source when the Pull failed).
    /// </summary>
    public readonly struct LivePullOutcome
    {


        public LivePullOutcome(PullReport report, ManifestFeatureEntry source)
        {
            Report = report;
            Source = source;
        }


        /// <summary>The Pull outcome: created/updated Baked Assets on success, or failures with sheet coordinates.</summary>
        public PullReport Report { get; }


        /// <summary>Feature Key, spreadsheet id, checked title, Drive revision; null when the Pull failed.</summary>
        public ManifestFeatureEntry Source { get; }


    }


    /// <summary>
    /// Runs one-way Pull: Google Sheets (or injected grids) → Vertical Nest → Baked Assets.
    /// Returns a PullReport (success summary or failures with sheet coordinates).
    /// </summary>
    public static class DataConfigPullRunner
    {


        #region Public API

        /// <summary>
        /// Live Google Pull: resolve spreadsheet id from the Environment Profile → OAuth/service-account →
        /// Title Check (aborts on mismatch before any cell fetch) → Sheets API cell grids → parse → Baked Assets.
        /// Commas inside cells do not break Pull (no CSV required). Delegates to <see cref="PullWithSourceAsync"/>
        /// and returns only its report.
        /// </summary>
        public static async Task<PullReport> PullAsync(PullConfig config)
        {
            LivePullOutcome outcome = await PullWithSourceAsync(config);
            return outcome.Report;
        }


        /// <summary>
        /// Same live Pull as <see cref="PullAsync"/>, also returning the Source Sheet facts the Config Manifest
        /// records (Feature Key, spreadsheet id, checked title, Drive revision). Drive revision is fetched only
        /// after a successful Pull and is best effort: a Drive failure leaves those fields empty. When the Pull
        /// Config sets <see cref="PullConfig.SkipTitleCheck"/> no title is read, so the recorded title is empty.
        /// </summary>
        public static async Task<LivePullOutcome> PullWithSourceAsync(PullConfig config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            ValidateTargets(config);
            ValidateGoogle(config);
            string spreadsheetId = ResolveSpreadsheetId(config);

            SheetsService sheets = await GoogleOAuthSession.CreateSheetsServiceAsync(config);
            EnvironmentProfile profile = config.EnvironmentProfile;

            // The guard discards the title it reads; keep it for the manifest row.
            string checkedTitle = "";
            Dictionary<Type, SheetGrid> grids = await SourceSheetTitleGuard.FetchAfterTitleCheckAsync(
                spreadsheetId,
                profile.ActiveEnvironmentKey,
                profile.ToTitlePrefixOverrides(),
                async id =>
                {
                    checkedTitle = await GoogleSheetsGridFetcher.FetchSpreadsheetTitleAsync(sheets, id);
                    return checkedTitle;
                },
                id => GoogleSheetsGridFetcher.FetchGridsAsync(config, sheets, id),
                config.SkipTitleCheck);
            PullReport report = PullFromGrids(config, grids);
            if (!report.Success)
                return new LivePullOutcome(report, null);

            // Created lazily inside the lookup so a Drive client failure is swallowed with the fetch.
            SheetRevisionMetadata revision = await SheetRevisionLookup.TryFetchAsync(
                async id =>
                {
                    DriveService drive = await GoogleOAuthSession.CreateDriveServiceAsync(config);
                    return await GoogleDriveRevisionFetcher.FetchAsync(drive, id);
                },
                spreadsheetId);

            ManifestFeatureEntry source = new ManifestFeatureEntry(
                config.FeatureKey,
                spreadsheetId.Trim(),
                checkedTitle,
                revision.ModifiedTime,
                revision.Version);
            return new LivePullOutcome(report, source);
        }


        /// <summary>
        /// Fixture/inject Pull: parse provided grids and create or overwrite Baked Assets
        /// under the Pull Config's shared output folder (GUID stable on re-Pull).
        /// Does not require an Environment Profile, Feature Key, or credentials (no Title Check; grids are already in hand).
        /// Failed Pulls write nothing and return a report with sheet coordinates when available.
        /// Only Config Types with Include In Pull are processed.
        /// </summary>
        public static PullReport PullFromGrids(
            PullConfig config,
            IReadOnlyDictionary<Type, SheetGrid> gridsByConfigType)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));
            if (gridsByConfigType == null)
                throw new ArgumentNullException(nameof(gridsByConfigType));

            ValidateTargets(config);

            PullTypeCandidate[] candidates = config.ToPullTypeCandidates();
            Type[] includedTypes = PullTargetRules.ResolveIncluded(candidates);

            List<PendingWrite> pending = new List<PendingWrite>();
            List<VerticalNestParseError> errors = new List<VerticalNestParseError>();
            // Local Only (and similar) notices from successful parses; shown on Succeeded PullReport
            List<string> warnings = new List<string>();

            // Parse every included tab into scratch instances first so a failure does not partially write
            for (int i = 0; i < includedTypes.Length; i++)
            {
                Type configType = includedTypes[i];
                SheetGrid grid;
                string tabTitle = ConfigTypeTabName.Resolve(configType);
                if (!gridsByConfigType.TryGetValue(configType, out grid) || grid == null)
                {
                    errors.Add(new VerticalNestParseError(
                        -1,
                        -1,
                        $"[{tabTitle}] No injected grid for Config Type '{configType.Name}'."));
                    continue;
                }

                ConfigTypeAsset scratch = ScriptableObject.CreateInstance(configType) as ConfigTypeAsset;
                if (scratch == null)
                {
                    errors.Add(new VerticalNestParseError(
                        -1,
                        -1,
                        $"[{tabTitle}] Could not create Config Type instance '{configType.FullName}'."));
                    continue;
                }

                VerticalNestParseResult parseResult = VerticalNestParser.ParseInto(scratch, grid);
                if (!parseResult.Success)
                {
                    UnityEngine.Object.DestroyImmediate(scratch);
                    for (int e = 0; e < parseResult.Errors.Count; e++)
                        errors.Add(PrefixTab(tabTitle, parseResult.Errors[e]));
                    continue;
                }

                PullReport.AppendTabPrefixedWarnings(tabTitle, parseResult.Warnings, warnings);

                pending.Add(new PendingWrite
                {
                    ConfigType = configType,
                    Scratch = scratch,
                });
            }

            if (errors.Count > 0)
            {
                for (int i = 0; i < pending.Count; i++)
                    UnityEngine.Object.DestroyImmediate(pending[i].Scratch);

                return PullReport.Failed(config.name, errors);
            }

            // After Pull on each scratch before any Baked Asset create/save
            List<Action> afterPullHooks = new List<Action>(pending.Count);
            for (int i = 0; i < pending.Count; i++)
            {
                ConfigTypeAsset scratch = pending[i].Scratch;
                string typeName = pending[i].ConfigType.Name;
                afterPullHooks.Add(() =>
                {
                    try
                    {
                        scratch.OnAfterPull();
                    }
                    catch (Exception ex)
                    {
                        // Preserve type name so designers know which Config Type threw
                        throw new InvalidOperationException(
                            $"On '{typeName}': {ex.Message}",
                            ex);
                    }
                });
            }

            VerticalNestParseError afterPullError;
            if (!AfterPull.TryInvokeAll(afterPullHooks, out afterPullError))
            {
                for (int i = 0; i < pending.Count; i++)
                    UnityEngine.Object.DestroyImmediate(pending[i].Scratch);

                return PullReport.Failed(config.name, new[] { afterPullError });
            }

            // All parses + After Pull succeeded: create missing assets or overwrite in place
            List<PullWriteAction> writes = new List<PullWriteAction>(pending.Count);
            for (int i = 0; i < pending.Count; i++)
            {
                PendingWrite write = pending[i];
                string assetPath = BakedAssetPath.ForConfigType(config.OutputFolder, write.ConfigType);
                ConfigTypeAsset existing = BakedAssetWriter.LoadExisting(write.ConfigType, config.OutputFolder);
                if (existing == null)
                {
                    BakedAssetWriter.CreateNew(write.Scratch, write.ConfigType, config.OutputFolder);
                    writes.Add(new PullWriteAction(write.ConfigType.Name, assetPath, PullWriteKind.Created));
                }
                else
                {
                    BakedAssetItemsCopy.CopyRootItems(write.Scratch, existing);
                    BakedAssetWriter.SaveExisting(existing);
                    UnityEngine.Object.DestroyImmediate(write.Scratch);
                    writes.Add(new PullWriteAction(write.ConfigType.Name, assetPath, PullWriteKind.Updated));
                }
            }

            return PullReport.Succeeded(config.name, writes, warnings);
        }

        #endregion


        #region Validation

        /// <summary>
        /// Shared output folder, candidates, Include In Pull, and ConfigTypeAsset checks
        /// (inject + live Google).
        /// </summary>
        internal static void ValidateTargets(PullConfig config)
        {
            PullTypeCandidate[] candidates = config.ToPullTypeCandidates();
            PullTargetRules.Validate(config.name, config.OutputFolder, candidates);

            // Concrete ConfigTypeAsset check stays here (Unity runtime types).
            for (int i = 0; i < candidates.Length; i++)
            {
                Type type = candidates[i].ConfigType;
                if (type.IsAbstract || !typeof(ConfigTypeAsset).IsAssignableFrom(type))
                {
                    throw new InvalidOperationException(
                        $"PullConfig '{config.name}' Config Types[{i}] '{type.FullName}' must be a concrete ConfigTypeAsset subclass.");
                }
            }
        }


        /// <summary>
        /// Source Sheet spreadsheet id for this Pull Config's Feature Key in the Environment Profile's
        /// active environment. Throws when the profile or Feature Key is unset, or the profile cannot resolve.
        /// </summary>
        internal static string ResolveSpreadsheetId(PullConfig config)
        {
            if (config.EnvironmentProfile == null)
                throw new InvalidOperationException($"PullConfig '{config.name}' has no Environment Profile.");

            if (string.IsNullOrWhiteSpace(config.FeatureKey))
                throw new InvalidOperationException($"PullConfig '{config.name}' has no Feature Key.");

            return config.EnvironmentProfile.ResolveSpreadsheetId(config.FeatureKey);
        }


        /// <summary>Credential paths required for live Google Pull.</summary>
        internal static void ValidateGoogle(PullConfig config)
        {
            if (config.AuthMode == GoogleAuthMode.OAuthUser)
            {
                // Empty is the new-config default; fail here instead of ArgumentException in path resolve.
                if (string.IsNullOrWhiteSpace(config.OAuthClientSecretsPath))
                    throw new InvalidOperationException(
                        $"PullConfig '{config.name}' has an empty OAuth client secrets path.");

                if (string.IsNullOrWhiteSpace(config.OAuthUserTokenPath))
                    throw new InvalidOperationException(
                        $"PullConfig '{config.name}' has an empty OAuth user token path.");
            }
            else if (string.IsNullOrWhiteSpace(config.ServiceAccountCredentialPath))
            {
                throw new InvalidOperationException(
                    $"PullConfig '{config.name}' has an empty service-account credential path.");
            }
        }

        #endregion


        #region Helpers

        struct PendingWrite
        {
            public Type ConfigType;
            public ConfigTypeAsset Scratch;
        }


        /// <summary>Prefixes parse errors with the Google tab title designers will open.</summary>
        static VerticalNestParseError PrefixTab(string tabTitle, VerticalNestParseError error)
        {
            return new VerticalNestParseError(
                error.Row,
                error.Column,
                $"[{tabTitle}] {error.Message}");
        }

        #endregion


    }
}
