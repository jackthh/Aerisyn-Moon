// Package entry markers: keep the assemblies non-empty and establish namespaces
// until the real Quests API is ported from the source project.

namespace Aerisyn.Quests
{
    /// <summary>
    /// Runtime assembly marker for <c>com.aerisyn.quests</c>.
    /// Replace / extend with the public quest API as features land.
    /// </summary>
    public static class QuestsPackage
    {
        #region Public API

        /// <summary>UPM package id (`package.json` name).</summary>
        public const string PackageId = "com.aerisyn.quests";

        /// <summary>SemVer string; keep in sync with package.json when releasing.</summary>
        public const string Version = "0.1.0";

        #endregion
    }
}
