using System;
using UnityEditor;
using UnityEngine;

namespace Aerisyn.DataConfigSheet.Editor
{
    /// <summary>
    /// Loads the one Data Config Settings asset from its fixed path. Pull, Pull All, Sign In and the
    /// build gate go through here so a missing asset fails with one clear message.
    /// </summary>
    public static class DataConfigSettingsLoader
    {


        /// <summary>
        /// Returns the settings at <see cref="DataConfigSettings.DefaultAssetPath"/>.
        /// Throws InvalidOperationException when the asset is missing or cannot be loaded.
        /// </summary>
        public static DataConfigSettings Load()
        {
            DataConfigSettings settings =
                AssetDatabase.LoadAssetAtPath<DataConfigSettings>(DataConfigSettings.DefaultAssetPath);
            if (settings == null)
            {
                throw new InvalidOperationException(
                    $"Data Config Settings not found at '{DataConfigSettings.DefaultAssetPath}'. " +
                    "Open Aerisyn > Data Config Sheet to create it, or restore the deleted asset.");
            }

            return settings;
        }


        /// <summary>
        /// Returns the settings at <see cref="DataConfigSettings.DefaultAssetPath"/>, creating the asset
        /// (and its folder) first when missing. A new asset is seeded with Dev, Test, Production and no
        /// Feature Keys by the <see cref="DataConfigSettings"/> field initializers. Used by the window only;
        /// Pull and Sign In use <see cref="Load"/> so a deleted asset still fails loudly.
        /// </summary>
        public static DataConfigSettings LoadOrCreate()
        {
            DataConfigSettings settings =
                AssetDatabase.LoadAssetAtPath<DataConfigSettings>(DataConfigSettings.DefaultAssetPath);
            if (settings != null)
                return settings;

            foreach ((string parent, string name) in
                     AssetFolderChain.MissingFolders(DataConfigSettings.DefaultAssetPath, AssetDatabase.IsValidFolder))
            {
                AssetDatabase.CreateFolder(parent, name);
            }

            settings = ScriptableObject.CreateInstance<DataConfigSettings>();
            AssetDatabase.CreateAsset(settings, DataConfigSettings.DefaultAssetPath);
            AssetDatabase.SaveAssets();
            return settings;
        }


    }
}
