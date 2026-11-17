#nullable disable
using System;
using System.Collections.Generic;

namespace Aerisyn.DataConfigSheet
{
    /// <summary>
    /// Pull All uses one Environment Profile (one active Environment Key, one Config Manifest). This rule
    /// picks the first Pull Config's profile as the reference and flags every Pull Config that points at a
    /// different one, so a second profile can never write a second manifest or mix environments in one run.
    /// </summary>
    public static class PullAllProfileRule
    {


        /// <summary>
        /// Returns one entry per Pull Config: null when it may run, otherwise a message naming the Pull Config
        /// and both profiles. The reference is the first non-blank profile id in run order. A blank id (no
        /// profile assigned) is never flagged here; that Pull already fails on its own.
        /// </summary>
        public static string[] FindMismatches(
            IReadOnlyList<string> pullConfigNames,
            IReadOnlyList<string> profileIds)
        {
            if (pullConfigNames == null)
                throw new ArgumentNullException(nameof(pullConfigNames));
            if (profileIds == null)
                throw new ArgumentNullException(nameof(profileIds));
            if (pullConfigNames.Count != profileIds.Count)
                throw new ArgumentException("Pull Config names and profile ids must have the same length.");

            string[] messages = new string[profileIds.Count];
            string referenceId = null;

            for (int i = 0; i < profileIds.Count; i++)
            {
                string profileId = profileIds[i];
                if (string.IsNullOrWhiteSpace(profileId))
                    continue;

                if (referenceId == null)
                {
                    referenceId = profileId;
                    continue;
                }

                if (!string.Equals(profileId, referenceId, StringComparison.Ordinal))
                {
                    messages[i] =
                        $"Not pulled: Pull Config '{pullConfigNames[i]}' uses Environment Profile '{profileId}' " +
                        $"but Pull All uses '{referenceId}' (the first Pull Config's profile). " +
                        "Point every Pull Config at one Environment Profile.";
                }
            }

            return messages;
        }


    }
}
