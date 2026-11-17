#nullable disable
using System;
using System.Collections.Generic;

namespace Aerisyn.DataConfigSheet
{
    /// <summary>
    /// Pure Title Check rules: which prefix the Source Sheet file title must carry for an
    /// Environment Key, and the case-sensitive prefix match itself. The spreadsheet id stays the
    /// source of truth; the title is only a guard against a wrong id pasted into the wrong place.
    /// </summary>
    public static class TitleCheckRules
    {


        #region Prefix match

        /// <summary>
        /// True when the title starts with the prefix by ordinal, case-sensitive comparison
        /// (brackets included, no trimming). A null title or a blank prefix never matches, so a
        /// missing prefix can never accept every sheet.
        /// </summary>
        public static bool Matches(string title, string prefix)
        {
            if (title == null || string.IsNullOrWhiteSpace(prefix))
                return false;

            return title.StartsWith(prefix, StringComparison.Ordinal);
        }

        #endregion


        #region Expected prefix

        /// <summary>
        /// Prefix the Source Sheet title must carry for the active Environment Key: the profile's
        /// non-blank override when present, else the package default (Dev→[Dev], Test→[Tester],
        /// Production→[Production]). Throws InvalidOperationException for a key with neither (e.g.
        /// a game-added stage), so a custom environment is never silently unguarded. A blank override
        /// counts as unset for the same reason. Keys match ordinally, case-sensitive.
        /// </summary>
        public static string ResolveExpectedPrefix(
            string activeEnvironmentKey,
            IReadOnlyList<EnvironmentTitlePrefix> overrides)
        {
            if (overrides != null)
            {
                for (int i = 0; i < overrides.Count; i++)
                {
                    EnvironmentTitlePrefix entry = overrides[i];
                    if (!string.Equals(entry.EnvironmentKey, activeEnvironmentKey, StringComparison.Ordinal))
                        continue;

                    if (!string.IsNullOrWhiteSpace(entry.Prefix))
                        return entry.Prefix;
                }
            }

            string defaultPrefix = GetDefaultPrefix(activeEnvironmentKey);
            if (defaultPrefix != null)
                return defaultPrefix;

            throw new InvalidOperationException(
                $"Environment Key '{activeEnvironmentKey}' has no Source Sheet title prefix. " +
                "Add a title prefix override for it on the Environment Profile.");
        }


        // Package defaults for the seeded Environment Keys; null for anything else.
        static string GetDefaultPrefix(string environmentKey)
        {
            switch (environmentKey)
            {
                case "Dev":
                    return "[Dev]";
                case "Test":
                    return "[Tester]";
                case "Production":
                    return "[Production]";
                default:
                    return null;
            }
        }

        #endregion


    }
}
