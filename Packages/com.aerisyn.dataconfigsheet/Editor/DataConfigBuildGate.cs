using System;
using System.Collections.Generic;
using UnityEditor.Build;

namespace Aerisyn.DataConfigSheet.Editor
{
    /// <summary>
    /// Opt-in build helper. The package registers nothing: a game calls <see cref="Require"/> from its own
    /// <c>IPreprocessBuildWithReport</c> (see README) so player builds fail when the Environment Profile is not
    /// Production or the Config Manifest is incomplete.
    /// </summary>
    public static class DataConfigBuildGate
    {


        /// <summary>
        /// Throws <see cref="BuildFailedException"/> (which aborts the build) when the active Environment Key
        /// is not Production, or when the Config Manifest check against the required Config Type list fails.
        /// </summary>
        public static void Require(EnvironmentProfile profile)
        {
            if (profile == null)
                throw new ArgumentNullException(nameof(profile));

            List<string> problems = new List<string>();

            string environmentProblem = ProductionBuildGate.Check(profile.ActiveEnvironmentKey);
            if (environmentProblem != null)
                problems.Add(environmentProblem);

            problems.AddRange(ConfigManifestStore.CheckProfile(profile));

            if (problems.Count > 0)
            {
                throw new BuildFailedException(
                    "Data Config Sheet build gate failed:\n  • " + string.Join("\n  • ", problems));
            }
        }


    }
}
