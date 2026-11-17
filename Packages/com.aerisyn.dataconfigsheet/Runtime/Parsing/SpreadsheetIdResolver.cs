#nullable disable
using System;
using System.Collections.Generic;

namespace Aerisyn.DataConfigSheet
{
    /// <summary>
    /// Resolves a Source Sheet spreadsheet id from the Environment Profile bindings, the active
    /// Environment Key, and a Feature Key. Pull calls this instead of reading a hard-coded id.
    /// </summary>
    public static class SpreadsheetIdResolver
    {


        #region Resolve

        /// <summary>
        /// Returns the spreadsheet id bound to (active environment, feature key).
        /// Throws InvalidOperationException when the active Environment Key or Feature Key is not in its
        /// list, when no binding exists for the pair, or when the bound id is blank. Bindings for keys
        /// no longer in a list are never matched because the key checks run first.
        /// </summary>
        public static string Resolve(
            IReadOnlyList<string> environmentKeys,
            IReadOnlyList<string> featureKeys,
            IReadOnlyList<EnvironmentFeatureBinding> bindings,
            string activeEnvironmentKey,
            string featureKey)
        {
            KeyListRules.RequireMember("Environment Key", environmentKeys, activeEnvironmentKey);
            KeyListRules.RequireMember("Feature Key", featureKeys, featureKey);

            if (bindings != null)
            {
                for (int i = 0; i < bindings.Count; i++)
                {
                    EnvironmentFeatureBinding binding = bindings[i];
                    if (binding.EnvironmentKey != activeEnvironmentKey || binding.FeatureKey != featureKey)
                        continue;

                    if (string.IsNullOrWhiteSpace(binding.SpreadsheetId))
                        break;

                    return binding.SpreadsheetId;
                }
            }

            throw new InvalidOperationException(
                $"Environment Profile has no spreadsheet id for Feature Key '{featureKey}' " +
                $"in Environment Key '{activeEnvironmentKey}'.");
        }

        #endregion


    }
}
