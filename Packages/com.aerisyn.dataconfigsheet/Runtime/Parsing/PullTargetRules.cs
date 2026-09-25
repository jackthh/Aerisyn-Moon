#nullable disable
using System;
using System.Collections.Generic;

namespace Aerisyn.DataConfigSheet
{
    /// <summary>
    /// Pull inclusion and target validation used by live Google and inject PullFromGrids.
    /// Resolve once; callers must not re-interpret Include In Pull independently.
    /// </summary>
    public static class PullTargetRules
    {


        #region Inclusion

        /// <summary>
        /// Returns Config Types with Include In Pull true, preserving candidate list order.
        /// </summary>
        public static Type[] ResolveIncluded(IReadOnlyList<PullTypeCandidate> candidates)
        {
            if (candidates == null)
                throw new ArgumentNullException(nameof(candidates));

            List<Type> included = new List<Type>();
            for (int i = 0; i < candidates.Count; i++)
            {
                PullTypeCandidate candidate = candidates[i];
                if (!candidate.IncludeInPull)
                    continue;

                included.Add(candidate.ConfigType);
            }

            return included.ToArray();
        }

        #endregion


        #region Validation

        /// <summary>
        /// Shared output folder and inclusion gates for inject + live Google.
        /// Empty candidate list, empty output folder, or zero Include In Pull all fail clearly.
        /// Does not check ConfigTypeAsset assignability (Unity runner seam does that).
        /// </summary>
        public static void Validate(
            string configName,
            string outputFolder,
            IReadOnlyList<PullTypeCandidate> candidates)
        {
            if (string.IsNullOrWhiteSpace(outputFolder))
                throw new InvalidOperationException($"PullConfig '{configName}' has an empty Output Folder.");

            if (candidates == null || candidates.Count == 0)
            {
                throw new InvalidOperationException(
                    $"PullConfig '{configName}' has no Config Types. Add explicit types to Pull (no assembly auto-scan).");
            }

            for (int i = 0; i < candidates.Count; i++)
            {
                if (candidates[i].ConfigType == null)
                {
                    throw new InvalidOperationException(
                        $"PullConfig '{configName}' Config Types[{i}] is null.");
                }
            }

            Type[] included = ResolveIncluded(candidates);
            if (included.Length == 0)
            {
                throw new InvalidOperationException(
                    $"PullConfig '{configName}' has no Config Types with Include In Pull enabled. " +
                    "Tick at least one candidate, or remove unused types from the list.");
            }
        }

        #endregion


    }
}
