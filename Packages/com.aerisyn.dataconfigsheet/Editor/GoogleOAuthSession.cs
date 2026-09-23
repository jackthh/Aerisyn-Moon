using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Sheets.v4;
using Google.Apis.Util.Store;
using UnityEngine;

namespace Aerisyn.DataConfigSheet.Editor
{
    /// <summary>
    /// Editor OAuth (browser sign-in) and credential resolution for Google bake.
    /// Writes an authorized_user JSON that BakingSheet GoogleSheetConverter can FromJson.
    /// </summary>
    public static class GoogleOAuthSession
    {
        const string FileDataStoreFolderName = "Aerisyn.DataConfigSheet.GoogleOAuth";


        #region Public API

        /// <summary>
        /// Opens the system browser for Google sign-in, then saves an authorized_user token for bake.
        /// Sheet sharing: Viewer to the signed-in Google email (no public link required).
        /// </summary>
        public static async Task SignInAsync(BakeConfig config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));
            if (config.AuthMode != GoogleAuthMode.OAuthUser)
                throw new InvalidOperationException(
                    "BakeConfig Auth Mode must be OAuth User to sign in. Switch Auth Mode, or use a service-account JSON instead.");

            string clientSecretsFullPath = DataConfigPathUtility.ResolveProjectPath(config.OAuthClientSecretsPath);
            if (!File.Exists(clientSecretsFullPath))
                throw new FileNotFoundException(
                    "OAuth client_secrets JSON not found. Create an OAuth Desktop client in Google Cloud, " +
                    "download the JSON, save it at the BakeConfig client-secrets path (gitignored), then Sign In again.",
                    clientSecretsFullPath);

            string tokenFullPath = DataConfigPathUtility.ResolveProjectPath(config.OAuthUserTokenPath);
            string storeDir = GetFileDataStoreDirectory();

            ClientSecrets secrets;
            using (FileStream stream = new FileStream(clientSecretsFullPath, FileMode.Open, FileAccess.Read))
            {
                secrets = GoogleClientSecrets.FromStream(stream).Secrets;
            }

            // Browser consent; FileDataStore remembers the session for re-auth without a prompt when valid
            UserCredential userCredential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                secrets,
                new[]
                {
                    SheetsService.Scope.SpreadsheetsReadonly,
                    DriveService.Scope.DriveReadonly,
                },
                "user",
                CancellationToken.None,
                new FileDataStore(storeDir, true));

            if (userCredential?.Token == null || string.IsNullOrEmpty(userCredential.Token.RefreshToken))
                throw new InvalidOperationException(
                    "Google sign-in did not return a refresh token. Revoke prior app access at " +
                    "https://myaccount.google.com/permissions then Sign In again (offline access required).");

            WriteAuthorizedUserToken(tokenFullPath, secrets.ClientId, secrets.ClientSecret, userCredential.Token.RefreshToken);
            Debug.Log($"[DataConfigSheet] Signed in. Token saved to '{config.OAuthUserTokenPath}'.");
        }


        /// <summary>Deletes the local OAuth token and FileDataStore so the next bake requires Sign In.</summary>
        public static void SignOut(BakeConfig config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            string tokenFullPath = DataConfigPathUtility.ResolveProjectPath(config.OAuthUserTokenPath);
            if (File.Exists(tokenFullPath))
                File.Delete(tokenFullPath);

            string storeDir = GetFileDataStoreDirectory();
            if (Directory.Exists(storeDir))
                Directory.Delete(storeDir, true);

            Debug.Log("[DataConfigSheet] Signed out. Local Google OAuth token cleared.");
        }


        /// <summary>True when the BakeConfig OAuth user-token file exists.</summary>
        public static bool IsSignedIn(BakeConfig config)
        {
            if (config == null || config.AuthMode != GoogleAuthMode.OAuthUser)
                return false;

            string tokenFullPath = DataConfigPathUtility.ResolveProjectPath(config.OAuthUserTokenPath);
            return File.Exists(tokenFullPath);
        }


        /// <summary>
        /// Returns credential JSON for GoogleSheetConverter (authorized_user or service_account).
        /// </summary>
        public static string ResolveCredentialJson(BakeConfig config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            if (config.AuthMode == GoogleAuthMode.ServiceAccount)
            {
                string path = DataConfigPathUtility.ResolveProjectPath(config.ServiceAccountCredentialPath);
                if (!File.Exists(path))
                    throw new FileNotFoundException(
                        "Service-account credential JSON not found. Place the key at the BakeConfig path, " +
                        "share the sheet with the robot email as Viewer, or switch Auth Mode to OAuth User.",
                        path);

                return File.ReadAllText(path);
            }

            string tokenPath = DataConfigPathUtility.ResolveProjectPath(config.OAuthUserTokenPath);
            if (!File.Exists(tokenPath))
                throw new FileNotFoundException(
                    "Not signed in. Use Aerisyn → Data Config Sheet → Sign In With Google, " +
                    "then bake again. Share the sheet with your Google email as Viewer.",
                    tokenPath);

            return File.ReadAllText(tokenPath);
        }

        #endregion


        #region Token file

        /// <summary>
        /// BakingSheet uses GoogleCredential.FromJson; authorized_user shape is the user-OAuth equivalent
        /// of a service-account key file.
        /// </summary>
        static void WriteAuthorizedUserToken(string fullPath, string clientId, string clientSecret, string refreshToken)
        {
            string directory = Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            var builder = new StringBuilder(256);
            builder.Append("{\n");
            builder.Append("  \"type\": \"authorized_user\",\n");
            builder.Append("  \"client_id\": \"").Append(EscapeJson(clientId)).Append("\",\n");
            builder.Append("  \"client_secret\": \"").Append(EscapeJson(clientSecret)).Append("\",\n");
            builder.Append("  \"refresh_token\": \"").Append(EscapeJson(refreshToken)).Append("\"\n");
            builder.Append("}\n");

            File.WriteAllText(fullPath, builder.ToString());
        }


        static string EscapeJson(string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            return value
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("\n", "\\n")
                .Replace("\r", "\\r");
        }


        static string GetFileDataStoreDirectory()
        {
            // Project-local store under UserSettings (already gitignored at repo root)
            return Path.GetFullPath(Path.Combine(
                Application.dataPath,
                "..",
                "UserSettings",
                "AerisynDataConfig",
                FileDataStoreFolderName));
        }

        #endregion


    }
}
