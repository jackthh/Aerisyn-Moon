using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Aerisyn.DataConfigSheet.Editor
{
    /// <summary>
    /// Editor IO around the pure Config Manifest seams: hash written Baked Assets, write/overwrite the
    /// manifest asset, and run the completeness check against an Environment Profile's required list.
    /// </summary>
    public static class ConfigManifestStore
    {


        #region Asset rows

        /// <summary>
        /// Maps a successful PullReport into manifest asset rows. Hashes the saved .asset file bytes, so a
        /// changed Baked Asset changes its hash. Config Type name is the asset file name (BakedAssetPath).
        /// </summary>
        public static IReadOnlyList<ManifestAssetEntry> BuildAssetRows(PullReport report)
        {
            if (report == null)
                throw new ArgumentNullException(nameof(report));

            List<ManifestAssetEntry> rows = new List<ManifestAssetEntry>(report.Writes.Count);
            for (int i = 0; i < report.Writes.Count; i++)
            {
                PullWriteAction write = report.Writes[i];
                string fullPath = DataConfigPathUtility.ResolveProjectPath(write.AssetPath);
                string hash = ManifestContentHash.Sha256Hex(File.ReadAllBytes(fullPath));
                rows.Add(new ManifestAssetEntry(write.ConfigTypeName, write.AssetPath, hash));
            }

            return rows;
        }

        #endregion


        #region Write

        /// <summary>
        /// Creates or overwrites the manifest asset at the project-relative path (GUID stays stable on
        /// overwrite). Undo is not recorded: Pull All already rewrites Baked Assets outside Undo.
        /// </summary>
        public static ConfigManifest Write(string assetPath, ConfigManifestSnapshot snapshot)
        {
            if (string.IsNullOrWhiteSpace(assetPath))
                throw new ArgumentException("Manifest asset path is empty.", nameof(assetPath));
            if (snapshot == null)
                throw new ArgumentNullException(nameof(snapshot));

            string normalizedPath = DataConfigPathUtility.NormalizeAssetPath(assetPath.Trim());
            ConfigManifest manifest = AssetDatabase.LoadAssetAtPath<ConfigManifest>(normalizedPath);
            if (manifest == null)
            {
                manifest = ScriptableObject.CreateInstance<ConfigManifest>();
                manifest.Apply(snapshot);
                EnsureFolderExists(normalizedPath);
                AssetDatabase.CreateAsset(manifest, normalizedPath);
            }
            else
            {
                manifest.Apply(snapshot);
                EditorUtility.SetDirty(manifest);
            }

            AssetDatabase.SaveAssets();
            return manifest;
        }

        #endregion


        #region Completeness

        /// <summary>
        /// Checks the manifest asset against the settings' active Environment Key and required Config Type
        /// list. Returns problems (empty = complete). Settings with no manifest path and no required list
        /// have nothing to check and return empty; a required list without a path is a problem.
        /// </summary>
        public static IReadOnlyList<string> CheckSettings(DataConfigSettings settings)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            bool hasPath = !string.IsNullOrWhiteSpace(settings.ManifestAssetPath);
            if (!hasPath)
            {
                if (settings.RequiredConfigTypeNames.Count == 0)
                    return Array.Empty<string>();

                return new[]
                {
                    "Data Config Settings lists required Config Types but has no Config Manifest asset path.",
                };
            }

            ConfigManifest manifest = AssetDatabase.LoadAssetAtPath<ConfigManifest>(
                DataConfigPathUtility.NormalizeAssetPath(settings.ManifestAssetPath.Trim()));
            ConfigManifestSnapshot snapshot = manifest == null ? null : manifest.ToSnapshot();
            return ManifestCompleteness.Check(snapshot, settings.ActiveEnvironmentKey, settings.RequiredConfigTypeNames);
        }

        #endregion


        #region Folders

        static void EnsureFolderExists(string assetPath)
        {
            string directory = Path.GetDirectoryName(assetPath)?.Replace('\\', '/');
            if (string.IsNullOrEmpty(directory) || AssetDatabase.IsValidFolder(directory))
                return;

            string[] parts = directory.Split('/');
            if (parts.Length == 0 || parts[0] != "Assets")
                throw new InvalidOperationException($"Config Manifest must be under Assets/. Got '{directory}'.");

            string current = "Assets";
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
