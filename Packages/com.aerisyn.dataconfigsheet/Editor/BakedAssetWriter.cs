using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Aerisyn.DataConfigSheet.Editor
{
    /// <summary>
    /// Creates missing Baked Assets and loads existing ones for in-place overwrite (stable GUID).
    /// </summary>
    public static class BakedAssetWriter
    {


        #region Public API

        /// <summary>
        /// Loads `{TypeName}.asset` from the shared output folder, or null when missing.
        /// </summary>
        public static ConfigTypeAsset LoadExisting(Type configType, string outputFolder)
        {
            string assetPath = BakedAssetPath.ForConfigType(outputFolder, configType);
            return AssetDatabase.LoadAssetAtPath(assetPath, configType) as ConfigTypeAsset;
        }


        /// <summary>
        /// Creates a new Baked Asset at the stable path. Caller must pass a populated instance.
        /// </summary>
        public static ConfigTypeAsset CreateNew(ConfigTypeAsset instance, Type configType, string outputFolder)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));

            string assetPath = BakedAssetPath.ForConfigType(outputFolder, configType);
            EnsureFolderExists(assetPath);

            AssetDatabase.CreateAsset(instance, assetPath);
            AssetDatabase.SaveAssets();
            return instance;
        }


        /// <summary>
        /// Persists in-place data overwrite on an existing Baked Asset (GUID unchanged).
        /// </summary>
        public static void SaveExisting(ConfigTypeAsset asset)
        {
            if (asset == null)
                throw new ArgumentNullException(nameof(asset));

            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
        }

        #endregion


        #region Folders

        /// <summary>
        /// Ensures every folder segment under Assets/ exists before CreateAsset.
        /// </summary>
        static void EnsureFolderExists(string assetPath)
        {
            string normalized = DataConfigPathUtility.NormalizeAssetPath(assetPath);
            string directory = Path.GetDirectoryName(normalized);
            if (string.IsNullOrEmpty(directory))
                return;

            directory = directory.Replace('\\', '/');
            if (AssetDatabase.IsValidFolder(directory))
                return;

            string[] parts = directory.Split('/');
            if (parts.Length == 0 || parts[0] != "Assets")
            {
                throw new InvalidOperationException(
                    $"Baked Asset output must be under Assets/. Got '{directory}'.");
            }

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
