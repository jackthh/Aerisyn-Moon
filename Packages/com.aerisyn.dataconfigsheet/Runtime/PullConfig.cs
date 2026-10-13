using System;
using System.Collections.Generic;
#if UNITY_EDITOR
using System.Threading.Tasks;
#endif
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace Aerisyn.DataConfigSheet
{
    /// <summary>
    /// Editor settings for one Pull job: Google spreadsheet, OAuth/service-account auth,
    /// one shared Baked Asset output folder, and owned Config Type candidates with Include In Pull.
    /// One-way: Google → Vertical Nest parse → Baked Assets. Never push back to Google.
    /// </summary>
    [CreateAssetMenu(
        fileName = "PullConfig",
        menuName = "Aerisyn/Data Config Sheet/Pull Config",
        order = 0)]
    [InfoBox(
        "Pull → ScriptableObject (ADR 0010). List Config Types explicitly; tabs match type name by default " +
        "(optional [SheetTab] override). Optional [ColumnAlias] on fields for designer headers. " +
        "One shared output folder for all Baked Assets. Never push generated data back to Google.",
        InfoMessageType.Info)]
    [InfoBox(
        "Default auth is OAuth (Sign In With Google). Share the sheet with your Google email as Viewer. " +
        "Service account stays available for CI / headless Pull.",
        InfoMessageType.Info)]
    public sealed class PullConfig : SerializedScriptableObject
    {


        #region Inspector actions (editor-wired)

#if UNITY_EDITOR
        /// <summary>
        /// Editor module assigns the shared Pull-with-UI workflow here so the Inspector
        /// button and menu items cannot diverge (Runtime cannot reference the Editor asm).
        /// </summary>
        public static Func<PullConfig, Task> RunEditorPullAsync;


        /// <summary>Editor-wired OAuth Sign In (progress bar + dialogs), same as the menu.</summary>
        public static Func<PullConfig, Task> RunEditorSignInAsync;


        /// <summary>Editor-wired OAuth Sign Out (dialogs), same as the menu.</summary>
        public static Action<PullConfig> RunEditorSignOut;


        /// <summary>Editor-wired token-file check for the Inspector signed-in status line.</summary>
        public static Func<PullConfig, bool> EditorIsSignedIn;


        // Top of Inspector (with Pull): OAuth status + Sign In / Sign Out when Auth Mode is OAuth User.
        [Button("Pull", ButtonSizes.Large)]
        [PropertyOrder(-100)]
        void InspectorPull()
        {
            if (RunEditorPullAsync == null)
            {
                Debug.LogError(
                    "[DataConfigSheet] Editor Pull workflow is not registered. " +
                    "Is the Data Config Sheet Editor assembly loaded?");
                return;
            }

            _ = RunEditorPullAsync(this);
        }


        // Token-file status from EditorIsSignedIn; hidden for Service Account.
        [ShowInInspector]
        [ShowIf(nameof(ShowOAuthAuthActions))]
        [PropertyOrder(-99)]
        [DisplayAsString]
        [LabelText("Google auth")]
        string InspectorSignedInStatus
        {
            get
            {
                if (EditorIsSignedIn != null && EditorIsSignedIn(this))
                    return "Signed in";
                return "Not signed in";
            }
        }


        // Same SignInWithUiAsync path as the menu alias.
        [Button("Sign In With Google")]
        [ShowIf(nameof(ShowOAuthAuthActions))]
        [PropertyOrder(-98)]
        [HorizontalGroup("OAuthActions")]
        void InspectorSignIn()
        {
            if (RunEditorSignInAsync == null)
            {
                Debug.LogError(
                    "[DataConfigSheet] Editor Sign In workflow is not registered. " +
                    "Is the Data Config Sheet Editor assembly loaded?");
                return;
            }

            _ = RunEditorSignInAsync(this);
        }


        // Same SignOutWithUi path as the menu alias.
        [Button("Sign Out")]
        [ShowIf(nameof(ShowOAuthAuthActions))]
        [PropertyOrder(-97)]
        [HorizontalGroup("OAuthActions")]
        void InspectorSignOut()
        {
            if (RunEditorSignOut == null)
            {
                Debug.LogError(
                    "[DataConfigSheet] Editor Sign Out workflow is not registered. " +
                    "Is the Data Config Sheet Editor assembly loaded?");
                return;
            }

            RunEditorSignOut(this);
        }


        // Gates OAuth-only Inspector chrome (status + Sign In / Sign Out).
        bool ShowOAuthAuthActions => _authMode == GoogleAuthMode.OAuthUser;
#endif

        #endregion


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
        string _oauthClientSecretsPath = "";


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
        string _serviceAccountCredentialPath = "";

        #endregion


        #region Pull targets

        [FoldoutGroup("Pull targets")]
        [Tooltip("Shared folder for all Baked Assets from this Pull Config (project-relative).")]
        [FolderPath(RequireExistingPath = false)]
        [SerializeField]
        string _outputFolder = "";


        // Odin must own this list: Unity [Serializable] entries drop System.Type, so a Unity
        // SerializeField list only ever persisted Include In Pull and left _configType null.
        // CustomAddFunction avoids null slots (Odin default for reference lists → "None" rows).
        [FoldoutGroup("Pull targets")]
        [Tooltip(
            "Owned Config Type candidates. Include In Pull (checkbox) gates fetch and bake; " +
            "unticked types stay listed. Google tab title matches the type name by default, " +
            "or [SheetTab(\"...\")] when titles differ.")]
        [ListDrawerSettings(
            ShowIndexLabels = true,
            DraggableItems = true,
            ShowFoldout = false,
            CustomAddFunction = nameof(CreateConfigTypeEntry))]
        [OdinSerialize, NonSerialized]
        List<PullConfigTypeEntry> _configTypeEntries = new List<PullConfigTypeEntry>();

        #endregion


        #region List helpers

        // List + must construct an entry; otherwise Odin inserts null and draws None (Type).
        static PullConfigTypeEntry CreateConfigTypeEntry()
        {
            return new PullConfigTypeEntry();
        }

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


        /// <summary>Shared output folder for Baked Assets from this Pull.</summary>
        public string OutputFolder => _outputFolder;


        /// <summary>Owned Config Type candidates (included and unticked).</summary>
        public IReadOnlyList<PullConfigTypeEntry> ConfigTypeEntries => _configTypeEntries;


        /// <summary>
        /// Builds pure inclusion candidates for PullTargetRules (validation, fetch, inject).
        /// </summary>
        public PullTypeCandidate[] ToPullTypeCandidates()
        {
            List<PullConfigTypeEntry> entries = _configTypeEntries;
            if (entries == null || entries.Count == 0)
                return Array.Empty<PullTypeCandidate>();

            PullTypeCandidate[] candidates = new PullTypeCandidate[entries.Count];
            for (int i = 0; i < entries.Count; i++)
            {
                PullConfigTypeEntry entry = entries[i];
                if (entry == null)
                {
                    candidates[i] = new PullTypeCandidate(null, includeInPull: false);
                    continue;
                }

                candidates[i] = entry.ToCandidate();
            }

            return candidates;
        }

        #endregion


    }
}
