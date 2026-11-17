using System;
using System.Collections.Generic;
using UnityEditor.Build;

namespace Aerisyn.DataConfigSheet.Editor
{
    /// <summary>
    /// Opt-in build helpers. The package registers nothing: a game calls these from its own
    /// <c>IPreprocessBuildWithReport</c> (see README). <see cref="Require"/> is the Production gate;
    /// <see cref="RequireCompleteManifest"/> is a separate, additional check the game may also call.
    /// Pass <see cref="DataConfigSettingsLoader.Load"/> for the project's settings asset.
    /// </summary>
    public static class DataConfigBuildGate
    {


        /// <summary>
        /// Production gate: throws <see cref="BuildFailedException"/> (which aborts the build) when the
        /// settings' active Environment Key is not Production. Does not look at the Config Manifest; pair it
        /// with <see cref="RequireCompleteManifest"/> to also fail on missing required Config Types.
        /// </summary>
        public static void Require(DataConfigSettings settings)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            string environmentProblem = ProductionBuildGate.Check(settings.ActiveEnvironmentKey);
            if (environmentProblem != null)
                throw new BuildFailedException("Data Config Sheet build gate failed:\n  • " + environmentProblem);
        }


        /// <summary>
        /// Completeness gate: throws <see cref="BuildFailedException"/> when the Config Manifest is missing,
        /// was written for another environment, or lacks a required Config Type. Independent of the
        /// Production check, so a game can gate a non-Production build on completeness alone.
        /// </summary>
        public static void RequireCompleteManifest(DataConfigSettings settings)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            IReadOnlyList<string> problems = ConfigManifestStore.CheckSettings(settings);
            if (problems.Count > 0)
            {
                throw new BuildFailedException(
                    "Data Config Sheet manifest check failed:\n  • " + string.Join("\n  • ", problems));
            }
        }


    }
}
