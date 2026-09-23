using Sirenix.OdinInspector;
using UnityEngine;

namespace Aerisyn.DataConfigSheet
{
    /// <summary>
    /// Editor bake settings: Google spreadsheet id(s), auth, optional CSV cache,
    /// and a single editable baked ScriptableObject (game-owned). One-way: Google → SO only.
    /// </summary>
    [CreateAssetMenu(
        fileName = "BakeConfig",
        menuName = "Aerisyn/Data Config Sheet/Bake Config",
        order = 0)]
    [InfoBox(
        "One-way bake only: Google Sheet is the official source of truth. " +
        "The baked ScriptableObject is runtime final and editable for fast local tests; " +
        "never push SO edits back to Google Sheets. Re-bake overwrites local SO data.",
        InfoMessageType.Warning)]
    [InfoBox(
        "Default auth is OAuth (Sign In With Google). Share the sheet with your Google email as Viewer. " +
        "Service account stays available for CI / headless bake.",
        InfoMessageType.Info)]
    public sealed class BakeConfig : ScriptableObject
    {


        #region Google source

        [FoldoutGroup("Google source")]
        [Tooltip("Google Spreadsheet id(s) from the sheet URL (.../d/{id}/...). One id is enough for MVP.")]
        [ListDrawerSettings(ShowIndexLabels = true, DraggableItems = true)]
        [SerializeField]
        string[] _spreadsheetIds = System.Array.Empty<string>();


        [FoldoutGroup("Google source")]
        [Tooltip("OAuthUser = browser sign-in (default). ServiceAccount = robot JSON for CI.")]
        [SerializeField]
        GoogleAuthMode _authMode = GoogleAuthMode.OAuthUser;


        [FoldoutGroup("Google source")]
        [ShowIf(nameof(_authMode), GoogleAuthMode.OAuthUser)]
        [Tooltip("Desktop OAuth client_secrets JSON from Google Cloud (one org setup). Gitignored.")]
        [FilePath(Extensions = "json", RequireExistingPath = false)]
        [SerializeField]
        string _oauthClientSecretsPath = "Assets/AerisynDataConfig/Credentials/oauth-client-secrets.json";


        [FoldoutGroup("Google source")]
        [ShowIf(nameof(_authMode), GoogleAuthMode.OAuthUser)]
        [Tooltip("Per-machine authorized_user token written after Sign In. Prefer UserSettings (gitignored).")]
        [FilePath(Extensions = "json", RequireExistingPath = false)]
        [SerializeField]
        string _oauthUserTokenPath = "UserSettings/AerisynDataConfig/oauth-user-token.json";


        [FoldoutGroup("Google source")]
        [ShowIf(nameof(_authMode), GoogleAuthMode.ServiceAccount)]
        [Tooltip("Service-account JSON. Share the sheet with that robot email as Viewer. Gitignored.")]
        [FilePath(Extensions = "json", RequireExistingPath = false)]
        [SerializeField]
        string _serviceAccountCredentialPath = "Assets/AerisynDataConfig/Credentials/service-account.json";

        #endregion


        #region Outputs

        [FoldoutGroup("Outputs")]
        [Tooltip("Single editable ScriptableObject that receives baked rows (no BakingSheet row sub-assets).")]
        [Required]
        [SerializeField]
        BakedSheetContainerAsset _bakedOutput;


        [FoldoutGroup("Outputs")]
        [Tooltip("Optional project-relative folder for a CSV cache after Google bake. Leave empty to skip.")]
        [FolderPath(RequireExistingPath = false)]
        [SerializeField]
        string _csvCachePath = "";

        #endregion


        #region Schema

        [FoldoutGroup("Schema")]
        [Tooltip("Game-owned factory that constructs the SheetContainer for this bake.")]
        [Required]
        [SerializeField]
        SheetContainerFactory _sheetContainerFactory;

        #endregion


        #region Public API

        /// <summary>Spreadsheet document ids to import (order preserved).</summary>
        public string[] SpreadsheetIds => _spreadsheetIds;


        /// <summary>OAuth user (default) or service-account auth.</summary>
        public GoogleAuthMode AuthMode => _authMode;


        /// <summary>Desktop OAuth client_secrets.json path (org setup, gitignored).</summary>
        public string OAuthClientSecretsPath => _oauthClientSecretsPath;


        /// <summary>Per-machine authorized_user token path written after browser sign-in.</summary>
        public string OAuthUserTokenPath => _oauthUserTokenPath;


        /// <summary>Service-account credential JSON path (CI / headless).</summary>
        public string ServiceAccountCredentialPath => _serviceAccountCredentialPath;


        /// <summary>Editable single-file bake destination (game-owned).</summary>
        public BakedSheetContainerAsset BakedOutput => _bakedOutput;


        /// <summary>Optional CSV cache folder; empty means no CSV store step.</summary>
        public string CsvCachePath => _csvCachePath;


        /// <summary>Factory that creates the game SheetContainer for bake.</summary>
        public SheetContainerFactory SheetContainerFactory => _sheetContainerFactory;

        #endregion


    }
}
