#nullable disable
using System;

namespace Aerisyn.DataConfigSheet
{
    /// <summary>
    /// Opt-in rule for player builds: the active Environment Key must be Production. The package never
    /// wires this into the build pipeline itself; a game calls it from its own build preprocessor.
    /// </summary>
    public static class ProductionBuildGate
    {


        const string ProductionKey = "Production";


        /// <summary>
        /// Returns null when the active environment is Production (ordinal, case-sensitive like all
        /// Environment Keys); otherwise a message naming the active environment so a build step can fail with it.
        /// </summary>
        public static string Check(string activeEnvironmentKey)
        {
            if (string.Equals(activeEnvironmentKey, ProductionKey, StringComparison.Ordinal))
                return null;

            string shown = string.IsNullOrWhiteSpace(activeEnvironmentKey) ? "(none)" : activeEnvironmentKey;
            return $"Active Environment Key is '{shown}' but a player build requires '{ProductionKey}'. " +
                   "Switch the Environment Profile to Production, Pull All, then build.";
        }


    }
}
