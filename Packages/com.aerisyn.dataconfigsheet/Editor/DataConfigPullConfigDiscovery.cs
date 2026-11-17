using System;
using System.Collections.Generic;
using UnityEditor;

namespace Aerisyn.DataConfigSheet.Editor
{
    /// <summary>
    /// Finds the Pull Config assets Pull All runs: those under the Data Config Settings folder path
    /// (recursive, stable order). Thin AssetDatabase wrapper around the pure PullConfigFolderScope rule.
    /// </summary>
    public static class DataConfigPullConfigDiscovery
    {


        /// <summary>
        /// Pull Config asset paths under <see cref="DataConfigSettings.PullAllFolderPath"/>.
        /// Throws InvalidOperationException when the folder is blank or holds no Pull Config.
        /// </summary>
        public static string[] FindPaths(DataConfigSettings settings)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            // Blank folder must fail before any project-wide search runs (no whole-project fallback).
            settings.SelectPullConfigPaths(Array.Empty<string>());

            string[] guids = AssetDatabase.FindAssets("t:PullConfig");
            List<string> allPaths = new List<string>(guids.Length);
            for (int i = 0; i < guids.Length; i++)
                allPaths.Add(AssetDatabase.GUIDToAssetPath(guids[i]));

            string[] selected = settings.SelectPullConfigPaths(allPaths);
            PullConfigFolderScope.RequireAny(settings.PullAllFolderPath, selected);
            return selected;
        }


        /// <summary>Loads the Pull Configs at <see cref="FindPaths"/> in the same order.</summary>
        public static List<PullConfig> Find(DataConfigSettings settings)
        {
            string[] paths = FindPaths(settings);
            List<PullConfig> configs = new List<PullConfig>(paths.Length);
            for (int i = 0; i < paths.Length; i++)
            {
                PullConfig config = AssetDatabase.LoadAssetAtPath<PullConfig>(paths[i]);
                if (config != null)
                    configs.Add(config);
            }

            return configs;
        }


    }
}
