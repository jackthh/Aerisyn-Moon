using System;
using UnityEditor;

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


    }
}
