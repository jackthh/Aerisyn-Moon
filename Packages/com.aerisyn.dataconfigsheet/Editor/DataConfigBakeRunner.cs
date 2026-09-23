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
    /// Runs one-way bake: Google Sheet → optional CSV cache → ScriptableObjectSheetExporter.
    /// Never pushes ScriptableObject edits back to Google Sheets.
    /// </summary>
    public static class DataConfigBakeRunner
    {


        #region Public API

        /// <summary>
        /// Validates config, bakes from Google (all spreadsheet ids), optionally caches CSV, then exports SOs.
        /// </summary>
        public static async Task BakeAsync(BakeConfig config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            Validate(config);

            string credentialJson = File.ReadAllText(ResolveProjectPath(config.CredentialPath));
            var logger = UnityLogger.Default;
            SheetContainerBase container = config.SheetContainerFactory.Create(logger);
            if (container == null)
                throw new InvalidOperationException(
                    $"SheetContainerFactory '{config.SheetContainerFactory.name}' returned null.");

            // Import: one GoogleSheetConverter per spreadsheet document
            ISheetImporter[] importers = BuildGoogleImporters(config.SpreadsheetIds, credentialJson);
            await container.Bake(importers);

            // Optional offline cache for diffing / CI without hitting Google every time
            if (!string.IsNullOrWhiteSpace(config.CsvCachePath))
            {
                string csvPath = ResolveProjectPath(config.CsvCachePath);
                Directory.CreateDirectory(csvPath);
                await container.Store(new CsvSheetConverter(csvPath));
            }

            // Runtime final: ScriptableObjects under the configured output folder
            string soPath = NormalizeAssetPath(config.ScriptableObjectOutputPath);
            EnsureAssetFolderExists(soPath);
            var exporter = new ScriptableObjectSheetExporter(soPath);
            await container.Store(exporter);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log(
                $"[DataConfigSheet] Bake complete → '{soPath}'" +
                (exporter.Result != null ? $" (container SO: {exporter.Result.name})" : string.Empty),
                exporter.Result);
        }

        #endregion


        #region Validation

        static void Validate(BakeConfig config)
        {
            if (config.SheetContainerFactory == null)
                throw new InvalidOperationException(
                    $"BakeConfig '{config.name}' has no SheetContainerFactory. Assign a game-owned factory asset.");

            if (config.SpreadsheetIds == null || config.SpreadsheetIds.Length == 0)
                throw new InvalidOperationException(
                    $"BakeConfig '{config.name}' has no spreadsheet ids.");

            for (int i = 0; i < config.SpreadsheetIds.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(config.SpreadsheetIds[i]))
                    throw new InvalidOperationException(
                        $"BakeConfig '{config.name}' spreadsheet id at index {i} is empty.");
            }

            if (string.IsNullOrWhiteSpace(config.CredentialPath))
                throw new InvalidOperationException(
                    $"BakeConfig '{config.name}' has an empty credential path.");

            string credentialFullPath = ResolveProjectPath(config.CredentialPath);
            if (!File.Exists(credentialFullPath))
                throw new FileNotFoundException(
                    "Service-account credential JSON not found. Create a Google Cloud service account, " +
                    "share the sheet as Viewer, place the JSON at the configured path (gitignored), then bake again.",
                    credentialFullPath);

            if (string.IsNullOrWhiteSpace(config.ScriptableObjectOutputPath))
                throw new InvalidOperationException(
                    $"BakeConfig '{config.name}' has an empty ScriptableObject output path.");
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


        #region Paths

        /// <summary>Maps a project-relative path to an absolute filesystem path.</summary>
        static string ResolveProjectPath(string projectRelativePath)
        {
            string normalized = projectRelativePath.Replace('\\', '/').Trim();
            if (normalized.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase) ||
                normalized.Equals("Assets", StringComparison.OrdinalIgnoreCase))
            {
                return Path.GetFullPath(Path.Combine(Application.dataPath, "..", normalized));
            }

            // Allow absolute paths for local-only credential files outside Assets
            if (Path.IsPathRooted(normalized))
                return normalized;

            return Path.GetFullPath(Path.Combine(Application.dataPath, "..", normalized));
        }


        static string NormalizeAssetPath(string path)
        {
            return path.Replace('\\', '/').TrimEnd('/');
        }


        /// <summary>
        /// Ensures each segment under Assets/ exists so ScriptableObjectSheetExporter can write.
        /// </summary>
        static void EnsureAssetFolderExists(string assetPath)
        {
            string normalized = NormalizeAssetPath(assetPath);
            if (!normalized.StartsWith("Assets", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException(
                    $"ScriptableObject output path must be under Assets/. Got: '{assetPath}'");

            if (AssetDatabase.IsValidFolder(normalized))
                return;

            string[] parts = normalized.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }

        #endregion


    }
}
