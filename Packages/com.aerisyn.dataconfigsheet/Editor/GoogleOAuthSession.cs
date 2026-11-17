using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Util.Store;
using UnityEngine;

namespace Aerisyn.DataConfigSheet.Editor
{
    /// <summary>
    /// Editor OAuth (browser sign-in) and credential resolution for Google Sheets API Pull.
    /// </summary>
    public static class GoogleOAuthSession
    {


        const string FileDataStoreFolderName = "Aerisyn.DataConfigSheet.GoogleOAuth";


        #region Public API

        /// <summary>
        /// Opens the system browser for Google sign-in, then saves an authorized_user token.
        /// Sheet sharing: Viewer to the signed-in Google email (no public link required).
        /// </summary>
        public static async Task SignInAsync(DataConfigSettings settings)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));
            if (settings.AuthMode != GoogleAuthMode.OAuthUser)
                throw new InvalidOperationException(
                    "Data Config Settings Auth Mode must be OAuth User to sign in. Switch Auth Mode, or use a service-account JSON instead.");

            // Match Pull ValidateGoogle: empty default must not fall through to ResolveProjectPath.
            if (string.IsNullOrWhiteSpace(settings.OAuthClientSecretsPath))
                throw new InvalidOperationException(
                    "Data Config Settings has an empty OAuth client secrets path.");

            string clientSecretsFullPath = DataConfigPathUtility.ResolveProjectPath(settings.OAuthClientSecretsPath);
            if (!File.Exists(clientSecretsFullPath))
                throw new FileNotFoundException(
                    "OAuth client_secrets JSON not found. Create an OAuth Desktop client in Google Cloud, " +
                    "download the JSON, save it at the Data Config Settings client-secrets path (gitignored), then Sign In again.",
                    clientSecretsFullPath);

            string tokenFullPath = DataConfigPathUtility.ResolveProjectPath(settings.OAuthUserTokenPath);
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
            Debug.Log($"[DataConfigSheet] Signed in. Token saved to '{settings.OAuthUserTokenPath}'.");
        }


        /// <summary>Deletes the local OAuth token and FileDataStore so the next Pull requires Sign In.</summary>
        public static void SignOut(DataConfigSettings settings)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            string tokenFullPath = DataConfigPathUtility.ResolveProjectPath(settings.OAuthUserTokenPath);
            if (File.Exists(tokenFullPath))
                File.Delete(tokenFullPath);

            string storeDir = GetFileDataStoreDirectory();
            if (Directory.Exists(storeDir))
                Directory.Delete(storeDir, true);

            Debug.Log("[DataConfigSheet] Signed out. Local Google OAuth token cleared.");
        }


        /// <summary>True when the Data Config Settings OAuth user-token file exists.</summary>
        public static bool IsSignedIn(DataConfigSettings settings)
        {
            if (settings == null || settings.AuthMode != GoogleAuthMode.OAuthUser)
                return false;

            string tokenFullPath = DataConfigPathUtility.ResolveProjectPath(settings.OAuthUserTokenPath);
            return File.Exists(tokenFullPath);
        }


        /// <summary>
        /// Builds a Sheets API client from Data Config Settings OAuth or service-account credentials.
        /// </summary>
        public static async Task<SheetsService> CreateSheetsServiceAsync(DataConfigSettings settings)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            return new SheetsService(await CreateClientInitializerAsync(settings));
        }


        /// <summary>
        /// Builds a Drive API client from the same OAuth or service-account credentials. The existing
        /// <c>drive.readonly</c> scope is enough for <c>files.get</c> revision fields; no new scope.
        /// </summary>
        public static async Task<DriveService> CreateDriveServiceAsync(DataConfigSettings settings)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            return new DriveService(await CreateClientInitializerAsync(settings));
        }

        #endregion


        #region Credentials

        // Sheets and Drive clients share the same credential and application name.
        static async Task<BaseClientService.Initializer> CreateClientInitializerAsync(DataConfigSettings settings)
        {
            return new BaseClientService.Initializer
            {
                HttpClientInitializer = await ResolveGoogleCredentialAsync(settings),
                ApplicationName = "Aerisyn DataConfigSheet",
            };
        }


        static async Task<GoogleCredential> ResolveGoogleCredentialAsync(DataConfigSettings settings)
        {
            if (settings.AuthMode == GoogleAuthMode.ServiceAccount)
            {
                string path = DataConfigPathUtility.ResolveProjectPath(settings.ServiceAccountCredentialPath);
                if (!File.Exists(path))
                    throw new FileNotFoundException(
                        "Service-account credential JSON not found. Place the key at the Data Config Settings path, " +
                        "share the sheet with the robot email as Viewer, or switch Auth Mode to OAuth User.",
                        path);

                using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    GoogleCredential credential = GoogleCredential.FromStream(stream);
                    return credential.CreateScoped(
                        SheetsService.Scope.SpreadsheetsReadonly,
                        DriveService.Scope.DriveReadonly);
                }
            }

            string tokenPath = DataConfigPathUtility.ResolveProjectPath(settings.OAuthUserTokenPath);
            if (!File.Exists(tokenPath))
                throw new FileNotFoundException(
                    "Not signed in. Open the Data Config Sheet window (or menu) → Sign In With Google, " +
                    "then Pull again. Share the sheet with your Google email as Viewer.",
                    tokenPath);

            // authorized_user JSON written by Sign In
            GoogleCredential userCredential = GoogleCredential.FromFile(tokenPath);
            return await Task.FromResult(userCredential.CreateScoped(
                SheetsService.Scope.SpreadsheetsReadonly,
                DriveService.Scope.DriveReadonly));
        }


        /// <summary>
        /// Writes authorized_user JSON that GoogleCredential.FromFile / FromJson can load.
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
