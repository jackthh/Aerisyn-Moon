using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Cathei.BakingSheet;
using Cathei.BakingSheet.Unity;
using UnityEditor;
using UnityEngine;

namespace Aerisyn.DataConfigSheet.Editor
{
    /// <summary>
    /// Runs one-way bake: Google Sheet → optional CSV cache → one editable BakedSheetContainerAsset.
    /// Never pushes ScriptableObject edits back to Google Sheets.
    /// </summary>
    public static class DataConfigBakeRunner
    {


        #region Public API

        /// <summary>
        /// Validates config, bakes from Google, optionally caches CSV, then applies rows into the baked SO.
        /// </summary>
        public static async Task BakeAsync(BakeConfig config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            Validate(config);

            string credentialJson = GoogleOAuthSession.ResolveCredentialJson(config);
            var logger = UnityLogger.Default;
            SheetContainerBase container = config.SheetContainerFactory.Create(logger);
            if (container == null)
                throw new InvalidOperationException(
                    $"SheetContainerFactory '{config.SheetContainerFactory.name}' returned null.");

            ISheetImporter[] importers = BuildGoogleImporters(config.SpreadsheetIds, credentialJson);
            await container.Bake(importers);

            if (!string.IsNullOrWhiteSpace(config.CsvCachePath))
            {
                string csvPath = DataConfigPathUtility.ResolveProjectPath(config.CsvCachePath);
                Directory.CreateDirectory(csvPath);
                await container.Store(new CsvSheetConverter(csvPath));
            }

            // One parent SO file with Inspector-editable lists (not BakingSheet row sub-assets)
            BakedSheetContainerAsset bakedOutput = config.BakedOutput;
            Undo.RecordObject(bakedOutput, "Bake Data Config Sheet");
            bakedOutput.ApplyFromContainer(container);
            EditorUtility.SetDirty(bakedOutput);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log(
                $"[DataConfigSheet] Bake complete → '{AssetDatabase.GetAssetPath(bakedOutput)}'",
                bakedOutput);
        }

        #endregion


        #region Validation

        static void Validate(BakeConfig config)
        {
            if (config.SheetContainerFactory == null)
                throw new InvalidOperationException(
                    $"BakeConfig '{config.name}' has no SheetContainerFactory. Assign a game-owned factory asset.");

            if (config.BakedOutput == null)
                throw new InvalidOperationException(
                    $"BakeConfig '{config.name}' has no Baked Output. Create a game-owned BakedSheetContainerAsset " +
                    "(one editable SO file) and assign it.");

            if (config.SpreadsheetIds == null || config.SpreadsheetIds.Length == 0)
                throw new InvalidOperationException(
                    $"BakeConfig '{config.name}' has no spreadsheet ids.");

            for (int i = 0; i < config.SpreadsheetIds.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(config.SpreadsheetIds[i]))
                    throw new InvalidOperationException(
                        $"BakeConfig '{config.name}' spreadsheet id at index {i} is empty.");
            }

            if (config.AuthMode == GoogleAuthMode.OAuthUser)
            {
                if (string.IsNullOrWhiteSpace(config.OAuthUserTokenPath))
                    throw new InvalidOperationException(
                        $"BakeConfig '{config.name}' has an empty OAuth user token path.");
            }
            else if (string.IsNullOrWhiteSpace(config.ServiceAccountCredentialPath))
            {
                throw new InvalidOperationException(
                    $"BakeConfig '{config.name}' has an empty service-account credential path.");
            }
        }

        #endregion


        #region Google importers

        static ISheetImporter[] BuildGoogleImporters(string[] spreadsheetIds, string credentialJson)
        {
            var importers = new List<ISheetImporter>(spreadsheetIds.Length);
            for (int i = 0; i < spreadsheetIds.Length; i++)
            {
                importers.Add(new GoogleSheetConverter(spreadsheetIds[i], credentialJson, TimeZoneInfo.Utc));
            }

            return importers.ToArray();
        }

        #endregion


    }
}
