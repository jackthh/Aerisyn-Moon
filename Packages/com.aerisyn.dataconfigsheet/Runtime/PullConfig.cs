using System;
using System.Collections.Generic;
#if UNITY_EDITOR
using System.Threading.Tasks;
#endif
using Sirenix.OdinInspector;
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


        #region Inspector Pull (editor-wired)

#if UNITY_EDITOR
        /// <summary>
        /// Editor module assigns the shared Pull-with-UI workflow here so the Inspector
        /// button and menu items cannot diverge (Runtime cannot reference the Editor asm).
        /// </summary>
        public static Func<PullConfig, Task> RunEditorPullAsync;


        // Same path as menu "Pull From Google (Selected Config)"; Sign In / Sign Out stay menu-only.
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


        [FoldoutGroup("Pull targets")]
        [Tooltip(
            "Owned Config Type candidates. Include In Pull (checkbox) gates fetch and bake; " +
            "unticked types stay listed. Google tab title matches the type name by default, " +
            "or [SheetTab(\"...\")] when titles differ.")]
        [ListDrawerSettings(ShowIndexLabels = true, DraggableItems = true)]
        [SerializeField]
        List<PullConfigTypeEntry> _configTypeEntries = new List<PullConfigTypeEntry>();

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
