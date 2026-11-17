#nullable disable
using System;
using System.Collections.Generic;

namespace Aerisyn.DataConfigSheet
{
    /// <summary>
    /// Pure rules for the Data Config Settings Environment Key and Feature Key lists: package seed defaults and
    /// pick membership. Key list assets call these so Inspectors and tests share one definition.
    /// </summary>
    public static class KeyListRules
    {


        #region Seed defaults

        /// <summary>
        /// Environment Keys the package seeds when Data Config Settings is created: Dev, Test, Production.
        /// Returns a fresh array so callers cannot mutate the shared defaults.
        /// </summary>
        public static string[] CreateSeedEnvironmentKeys()
        {
            return new[] { "Dev", "Test", "Production" };
        }


        /// <summary>
        /// Feature Keys the package seeds: none. Games own their Feature Keys (e.g. ai_ship, cannon).
        /// </summary>
        public static string[] CreateSeedFeatureKeys()
        {
            return Array.Empty<string>();
        }

        #endregion


        #region Pick membership

        /// <summary>
        /// True when the key appears in the list by ordinal, case-sensitive match. Null or blank keys
        /// never count, even if the list holds a blank entry, so an unset Inspector pick is never valid.
        /// </summary>
        public static bool IsMember(IReadOnlyList<string> keys, string key)
        {
            if (string.IsNullOrWhiteSpace(key) || keys == null)
                return false;

            for (int i = 0; i < keys.Count; i++)
            {
                if (string.Equals(keys[i], key, StringComparison.Ordinal))
                    return true;
            }

            return false;
        }


        /// <summary>
        /// Throws unless the key is a member of the list. The message names the list kind
        /// (e.g. "Feature Key"), the rejected key, and the allowed keys.
        /// </summary>
        public static void RequireMember(string keyKind, IReadOnlyList<string> keys, string key)
        {
            string problem = DescribeProblem(keyKind, keys, key);
            if (problem != null)
                throw new InvalidOperationException(problem);
        }


        /// <summary>
        /// Null when the key is a member of the list; otherwise the message naming the list kind, the
        /// rejected key and the allowed keys. Used by <see cref="RequireMember"/> and by UI warnings.
        /// </summary>
        public static string DescribeProblem(string keyKind, IReadOnlyList<string> keys, string key)
        {
            if (IsMember(keys, key))
                return null;

            string allowed = keys == null || keys.Count == 0
                ? "(none defined)"
                : string.Join(", ", keys);
            return $"{keyKind} '{key}' is not in the {keyKind} list. Allowed: {allowed}.";
        }

        #endregion


    }
}
