namespace Aerisyn.DataConfigSheet
{
    /// <summary>
    /// How the editor authenticates to Google Sheets for bake.
    /// OAuthUser is the default Idle-like path (browser sign-in, email-shared sheets).
    /// </summary>
    public enum GoogleAuthMode
    {
        /// <summary>Browser OAuth as the signed-in Google user (email-shared private sheets).</summary>
        OAuthUser = 0,

        /// <summary>Service-account JSON key (CI / headless; share sheet with the robot email).</summary>
        ServiceAccount = 1,
    }
}
