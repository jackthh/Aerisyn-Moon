using System;
using System.Threading.Tasks;

namespace Aerisyn.DataConfigSheet.Editor
{
    /// <summary>
    /// Runs one-way Pull: Google tabs → in-memory grid → Vertical Nest → Baked Assets.
    /// Parse and asset write land in later tickets; this shell validates config and fails clearly.
    /// </summary>
    public static class DataConfigPullRunner
    {


        #region Public API

        /// <summary>
        /// Validates Pull Config, then fails with a clear message until Vertical Nest Pull is implemented.
        /// </summary>
        public static Task PullAsync(PullConfig config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            Validate(config);

            throw new InvalidOperationException(
                $"Pull for '{config.name}' is not implemented yet. " +
                "Pull Config shell and OAuth Sign In are ready; Vertical Nest parse and Baked Asset write ship in later tickets.");
        }

        #endregion


        #region Validation

        /// <summary>
        /// Preconditions for Pull: spreadsheet, output folder, Config Types, and credential paths.
        /// </summary>
        internal static void Validate(PullConfig config)
        {
            if (string.IsNullOrWhiteSpace(config.SpreadsheetId))
                throw new InvalidOperationException($"PullConfig '{config.name}' has no Spreadsheet Id.");

            if (string.IsNullOrWhiteSpace(config.OutputFolder))
                throw new InvalidOperationException($"PullConfig '{config.name}' has an empty Output Folder.");

            if (config.ConfigTypes == null || config.ConfigTypes.Length == 0)
                throw new InvalidOperationException(
                    $"PullConfig '{config.name}' has no Config Types. Add explicit types to Pull (no assembly auto-scan).");

            for (int i = 0; i < config.ConfigTypes.Length; i++)
            {
                Type type = config.ConfigTypes[i];
                if (type == null)
                    throw new InvalidOperationException($"PullConfig '{config.name}' Config Types[{i}] is null.");

                if (type.IsAbstract || !typeof(ConfigTypeAsset).IsAssignableFrom(type))
                    throw new InvalidOperationException(
                        $"PullConfig '{config.name}' Config Types[{i}] '{type.FullName}' must be a concrete ConfigTypeAsset subclass.");
            }

            if (config.AuthMode == GoogleAuthMode.OAuthUser)
            {
                if (string.IsNullOrWhiteSpace(config.OAuthUserTokenPath))
                    throw new InvalidOperationException(
                        $"PullConfig '{config.name}' has an empty OAuth user token path.");
            }
            else if (string.IsNullOrWhiteSpace(config.ServiceAccountCredentialPath))
            {
                throw new InvalidOperationException(
                    $"PullConfig '{config.name}' has an empty service-account credential path.");
            }
        }

        #endregion


    }
}
