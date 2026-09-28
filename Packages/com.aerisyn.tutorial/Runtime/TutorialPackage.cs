namespace Aerisyn.Tutorial
{
    /// <summary>
    /// Package constants for <c>com.aerisyn.tutorial</c>.
    /// Core runtime: <see cref="TutorialRunner"/>, <see cref="TutorialDefinition"/>, and <see cref="ProgressSnapshot"/>.
    /// </summary>
    public static class TutorialPackage
    {
        #region Identity

        /// <summary>UPM package id (`package.json` name).</summary>
        public const string PackageId = "com.aerisyn.tutorial";

        /// <summary>SemVer string; keep in sync with package.json when releasing.</summary>
        public const string Version = "0.4.0";

        #endregion
    }
}
