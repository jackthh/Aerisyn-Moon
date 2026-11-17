#nullable disable
using System;
using System.Collections.Generic;

namespace Aerisyn.DataConfigSheet
{
    /// <summary>
    /// Completeness check for the Config Manifest: compares it to the expected Environment Key and a
    /// required Config Type list. Used by Pull All after the manifest write and by CI/build helpers.
    /// </summary>
    public static class ManifestCompleteness
    {


        /// <summary>
        /// Returns human-readable problems; empty means complete. Reports a missing manifest, a blank
        /// or different environment (ordinal, case-sensitive like Environment Keys), and every required
        /// Config Type name with no Baked Asset row. An empty required list only checks the environment.
        /// </summary>
        public static IReadOnlyList<string> Check(
            ConfigManifestSnapshot manifest,
            string expectedEnvironmentKey,
            IReadOnlyList<string> requiredConfigTypeNames)
        {
            List<string> problems = new List<string>();

            if (manifest == null)
            {
                problems.Add("Config Manifest not found. Run Pull All to write it.");
                return problems;
            }

            if (string.IsNullOrWhiteSpace(expectedEnvironmentKey))
            {
                problems.Add("No active Environment Key to check the Config Manifest against.");
            }
            else if (!string.Equals(manifest.EnvironmentKey, expectedEnvironmentKey, StringComparison.Ordinal))
            {
                problems.Add(
                    $"Config Manifest environment is '{manifest.EnvironmentKey}' but '{expectedEnvironmentKey}' is expected. " +
                    "Pull All again in the expected environment.");
            }

            if (requiredConfigTypeNames == null)
                return problems;

            for (int i = 0; i < requiredConfigTypeNames.Count; i++)
            {
                string requiredName = requiredConfigTypeNames[i];
                if (!HasAsset(manifest, requiredName))
                    problems.Add($"Config Manifest is missing required Config Type '{requiredName}'.");
            }

            return problems;
        }


        static bool HasAsset(ConfigManifestSnapshot manifest, string configTypeName)
        {
            for (int i = 0; i < manifest.Assets.Count; i++)
            {
                if (string.Equals(manifest.Assets[i].ConfigTypeName, configTypeName, StringComparison.Ordinal))
                    return true;
            }

            return false;
        }


    }
}
