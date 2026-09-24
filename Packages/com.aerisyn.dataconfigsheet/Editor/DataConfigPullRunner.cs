using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Aerisyn.DataConfigSheet.Editor
{
    /// <summary>
    /// Runs one-way Pull: cell grids → Vertical Nest → Baked Assets.
    /// Live Google fetch lands in a follow-up ticket; fixture/inject uses <see cref="PullFromGrids"/>.
    /// </summary>
    public static class DataConfigPullRunner
    {


        #region Public API

        /// <summary>
        /// Live Google Pull (ticket 03). Validates config, then fails clearly until Sheets fetch lands.
        /// </summary>
        public static Task PullAsync(PullConfig config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            ValidateTargets(config);
            ValidateGoogle(config);

            throw new InvalidOperationException(
                $"Live Google Pull for '{config.name}' is not implemented yet. " +
                "Use PullFromGrids with fixture/injected cell grids to parse and write Baked Assets. " +
                "OAuth Sign In remains available.");
        }


        /// <summary>
        /// Fixture/inject Pull: parse provided grids and create or overwrite Baked Assets
        /// under the Pull Config's shared output folder (GUID stable on re-Pull).
        /// Does not require Google spreadsheet id or credentials.
        /// </summary>
        public static VerticalNestParseResult PullFromGrids(
            PullConfig config,
            IReadOnlyDictionary<Type, SheetGrid> gridsByConfigType)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));
            if (gridsByConfigType == null)
                throw new ArgumentNullException(nameof(gridsByConfigType));

            ValidateTargets(config);

            List<PendingBake> pending = new List<PendingBake>();
            List<VerticalNestParseError> errors = new List<VerticalNestParseError>();

            // Parse every tab into scratch instances first so a failure does not partially write
            for (int i = 0; i < config.ConfigTypes.Length; i++)
            {
                Type configType = config.ConfigTypes[i];
                SheetGrid grid;
                if (!gridsByConfigType.TryGetValue(configType, out grid) || grid == null)
                {
                    errors.Add(new VerticalNestParseError(
                        -1,
                        -1,
                        $"No injected grid for Config Type '{configType.Name}'."));
                    continue;
                }

                ConfigTypeAsset scratch = ScriptableObject.CreateInstance(configType) as ConfigTypeAsset;
                if (scratch == null)
                {
                    errors.Add(new VerticalNestParseError(
                        -1,
                        -1,
                        $"Could not create Config Type instance '{configType.FullName}'."));
                    continue;
                }

                VerticalNestParseResult parseResult = VerticalNestParser.ParseInto(scratch, grid);
                if (!parseResult.Success)
                {
                    UnityEngine.Object.DestroyImmediate(scratch);
                    for (int e = 0; e < parseResult.Errors.Count; e++)
                        errors.Add(PrefixType(configType, parseResult.Errors[e]));
                    continue;
                }

                pending.Add(new PendingBake
                {
                    ConfigType = configType,
                    Scratch = scratch,
                });
            }

            if (errors.Count > 0)
            {
                for (int i = 0; i < pending.Count; i++)
                    UnityEngine.Object.DestroyImmediate(pending[i].Scratch);

                return VerticalNestParseResult.Fail(errors);
            }

            // All parses succeeded: create missing assets or overwrite data in place
            for (int i = 0; i < pending.Count; i++)
            {
                PendingBake bake = pending[i];
                ConfigTypeAsset existing = BakedAssetWriter.LoadExisting(bake.ConfigType, config.OutputFolder);
                if (existing == null)
                {
                    BakedAssetWriter.CreateNew(bake.Scratch, bake.ConfigType, config.OutputFolder);
                }
                else
                {
                    BakedAssetItemsCopy.CopyRootItems(bake.Scratch, existing);
                    BakedAssetWriter.SaveExisting(existing);
                    UnityEngine.Object.DestroyImmediate(bake.Scratch);
                }
            }

            return VerticalNestParseResult.Ok();
        }

        #endregion


        #region Validation

        /// <summary>Shared output folder and explicit Config Type list (inject + live Google).</summary>
        internal static void ValidateTargets(PullConfig config)
        {
            if (string.IsNullOrWhiteSpace(config.OutputFolder))
                throw new InvalidOperationException($"PullConfig '{config.name}' has an empty Output Folder.");

            if (config.ConfigTypes == null || config.ConfigTypes.Length == 0)
                throw new InvalidOperationException(
                    $"PullConfig '{config.name}' has no Config Types. Add explicit types to Pull (no assembly auto-scan).");

            for (int i = 0; i < config.ConfigTypes.Length; i++)
            {
                Type type = config.ConfigTypes[i];
                if (type == null)
                    throw new InvalidOperationException($"PullConfig '{config.name}' Config Types[{i}] is null.");

                if (type.IsAbstract || !typeof(ConfigTypeAsset).IsAssignableFrom(type))
                    throw new InvalidOperationException(
                        $"PullConfig '{config.name}' Config Types[{i}] '{type.FullName}' must be a concrete ConfigTypeAsset subclass.");
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


        /// <summary>Full live-Pull preconditions (targets + Google).</summary>
        internal static void Validate(PullConfig config)
        {
            ValidateTargets(config);
            ValidateGoogle(config);
        }

        #endregion


        #region Helpers

        struct PendingBake
        {
            public Type ConfigType;
            public ConfigTypeAsset Scratch;
        }


        static VerticalNestParseError PrefixType(Type configType, VerticalNestParseError error)
        {
            return new VerticalNestParseError(
                error.Row,
                error.Column,
                $"[{configType.Name}] {error.Message}");
        }

        #endregion


    }
}
