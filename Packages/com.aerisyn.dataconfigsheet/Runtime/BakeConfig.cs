using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Aerisyn.DataConfigSheet
{
    /// <summary>
    /// Editor bake settings: Google spreadsheet, OAuth/service-account auth, and Luban paths.
    /// One-way: Google → CSV → Luban JSON/code. Never push generated data back to Google.
    /// </summary>
    [CreateAssetMenu(
        fileName = "BakeConfig",
        menuName = "Aerisyn/Data Config Sheet/Bake Config",
        order = 0)]
    [InfoBox(
        "0.3.0: Google Sheet → CSV → Luban. Runtime final is Luban Tables + JSON (not editable SO). " +
        "Put ##var / ##type headers in each Google tab. Never push generated data back to Google.",
        InfoMessageType.Warning)]
    [InfoBox(
        "Default auth is OAuth (Sign In With Google). Share the sheet with your Google email as Viewer. " +
        "Service account stays available for CI / headless bake.",
        InfoMessageType.Info)]
    public sealed class BakeConfig : ScriptableObject
    {
        #region Google source

        [FoldoutGroup("Google source")]
        [Tooltip("Google Spreadsheet id from the URL (.../d/{id}/...).")]
        [SerializeField]
        string _spreadsheetId = "";


        [FoldoutGroup("Google source")]
        [Tooltip("OAuthUser = browser sign-in (default). ServiceAccount = robot JSON for CI.")]
        [SerializeField]
        GoogleAuthMode _authMode = GoogleAuthMode.OAuthUser;


        [FoldoutGroup("Google source")]
        [ShowIf(nameof(_authMode), GoogleAuthMode.OAuthUser)]
        [Tooltip("Desktop OAuth client_secrets JSON from Google Cloud (gitignored).")]
        [FilePath(Extensions = "json", RequireExistingPath = false)]
        [SerializeField]
        string _oauthClientSecretsPath = "Assets/AerisynDataConfig/Credentials/oauth-client-secrets.json";


        [FoldoutGroup("Google source")]
        [ShowIf(nameof(_authMode), GoogleAuthMode.OAuthUser)]
        [Tooltip("Per-machine authorized_user token written after Sign In (gitignored).")]
        [FilePath(Extensions = "json", RequireExistingPath = false)]
        [SerializeField]
        string _oauthUserTokenPath = "UserSettings/AerisynDataConfig/oauth-user-token.json";


        [FoldoutGroup("Google source")]
        [ShowIf(nameof(_authMode), GoogleAuthMode.ServiceAccount)]
        [Tooltip("Service-account JSON. Share the sheet with that robot email as Viewer.")]
        [FilePath(Extensions = "json", RequireExistingPath = false)]
        [SerializeField]
        string _serviceAccountCredentialPath = "Assets/AerisynDataConfig/Credentials/service-account.json";

        #endregion


        #region Tab export

        [FoldoutGroup("Tab export")]
        [Tooltip("Google tab title → CSV file name under Luban dataDir (e.g. Items → items.csv).")]
        [ListDrawerSettings(ShowIndexLabels = true, DraggableItems = true)]
        [SerializeField]
        SheetExportEntry[] _sheetExports = Array.Empty<SheetExportEntry>();

        #endregion


        #region Luban

        [FoldoutGroup("Luban")]
        [Tooltip("Folder containing luban.conf (project-relative).")]
        [FolderPath(RequireExistingPath = false)]
        [SerializeField]
        string _lubanProjectPath = "Assets/AerisynDataConfig/Luban";


        [FoldoutGroup("Luban")]
        [Tooltip("Path to Luban.dll (dotnet run). Default: repo Tools/Luban/Luban/Luban.dll.")]
        [FilePath(Extensions = "dll", RequireExistingPath = false)]
        [SerializeField]
        string _lubanDllPath = "Tools/Luban/Luban/Luban.dll";


        [FoldoutGroup("Luban")]
        [Tooltip("Luban -t target name from luban.conf.")]
        [SerializeField]
        string _lubanTarget = "client";


        [FoldoutGroup("Luban")]
        [Tooltip("Generated C# output directory (project-relative).")]
        [FolderPath(RequireExistingPath = false)]
        [SerializeField]
        string _outputCodeDir = "Assets/AerisynDataConfig/Gen";


        [FoldoutGroup("Luban")]
        [Tooltip("Generated JSON data directory (project-relative).")]
        [FolderPath(RequireExistingPath = false)]
        [SerializeField]
        string _outputDataDir = "Assets/AerisynDataConfig/GeneratedData";

        #endregion


        #region Public API

        /// <summary>Single Google spreadsheet document id.</summary>
        public string SpreadsheetId => _spreadsheetId;


        /// <summary>OAuth user (default) or service-account auth.</summary>
        public GoogleAuthMode AuthMode => _authMode;


        /// <summary>Desktop OAuth client_secrets.json path.</summary>
        public string OAuthClientSecretsPath => _oauthClientSecretsPath;


        /// <summary>Per-machine authorized_user token path.</summary>
        public string OAuthUserTokenPath => _oauthUserTokenPath;


        /// <summary>Service-account credential JSON path.</summary>
        public string ServiceAccountCredentialPath => _serviceAccountCredentialPath;


        /// <summary>Tab title → CSV file mappings to write under Luban dataDir.</summary>
        public SheetExportEntry[] SheetExports => _sheetExports;


        /// <summary>Directory that contains luban.conf.</summary>
        public string LubanProjectPath => _lubanProjectPath;


        /// <summary>Filesystem path to Luban.dll.</summary>
        public string LubanDllPath => _lubanDllPath;


        /// <summary>Luban export target name (e.g. client).</summary>
        public string LubanTarget => _lubanTarget;


        /// <summary>Where Luban writes generated C#.</summary>
        public string OutputCodeDir => _outputCodeDir;


        /// <summary>Where Luban writes generated JSON.</summary>
        public string OutputDataDir => _outputDataDir;

        #endregion


        #region Nested types

        /// <summary>One Google tab exported to one CSV file for Luban input.</summary>
        [Serializable]
        public sealed class SheetExportEntry
        {
            [Tooltip("Exact Google Sheet tab title.")]
            public string TabTitle;


            [Tooltip("CSV file name under Luban dataDir (e.g. items.csv).")]
            public string CsvFileName;
        }

        #endregion
    }
}
