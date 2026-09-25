using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Aerisyn.DataConfigSheet.Editor
{
    /// <summary>
    /// Runs one-way Pull: Google Sheets (or injected grids) → Vertical Nest → Baked Assets.
    /// Returns a PullReport (success summary or failures with sheet coordinates).
    /// </summary>
    public static class DataConfigPullRunner
    {


        #region Public API

        /// <summary>
        /// Live Google Pull: OAuth/service-account → Sheets API cell grids → parse → Baked Assets.
        /// Commas inside cells do not break Pull (no CSV required).
        /// </summary>
        public static async Task<PullReport> PullAsync(PullConfig config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            ValidateTargets(config);
            ValidateGoogle(config);

            Dictionary<Type, SheetGrid> grids = await GoogleSheetsGridFetcher.FetchGridsAsync(config);
            return PullFromGrids(config, grids);
        }


        /// <summary>
        /// Fixture/inject Pull: parse provided grids and create or overwrite Baked Assets
        /// under the Pull Config's shared output folder (GUID stable on re-Pull).
        /// Does not require Google spreadsheet id or credentials.
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

            // All parses succeeded: create missing assets or overwrite data in place
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

            return PullReport.Succeeded(config.name, writes);
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


        /// <summary>Spreadsheet id and credential paths required for live Google Pull.</summary>
        internal static void ValidateGoogle(PullConfig config)
        {
            if (string.IsNullOrWhiteSpace(config.SpreadsheetId))
                throw new InvalidOperationException($"PullConfig '{config.name}' has no Spreadsheet Id.");

            if (config.AuthMode == GoogleAuthMode.OAuthUser)
            {
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
