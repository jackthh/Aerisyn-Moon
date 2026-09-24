using System;
using System.Collections.Generic;
using System.Reflection;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Aerisyn.DataConfigSheet
{
    /// <summary>
    /// Editor settings for one Pull job: Google spreadsheet, OAuth/service-account auth,
    /// one shared Baked Asset output folder, and an explicit Config Type list.
    /// One-way: Google → Vertical Nest parse → Baked Assets. Never push back to Google.
    /// </summary>
    [CreateAssetMenu(
        fileName = "PullConfig",
        menuName = "Aerisyn/Data Config Sheet/Pull Config",
        order = 0)]
    [InfoBox(
        "Pull → ScriptableObject (ADR 0010). List Config Types explicitly; tabs match type name by default. " +
        "One shared output folder for all Baked Assets. Never push generated data back to Google.",
        InfoMessageType.Info)]
    [InfoBox(
        "Default auth is OAuth (Sign In With Google). Share the sheet with your Google email as Viewer. " +
        "Service account stays available for CI / headless Pull.",
        InfoMessageType.Info)]
    public sealed class PullConfig : SerializedScriptableObject
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


        #region Pull targets

        [FoldoutGroup("Pull targets")]
        [Tooltip("Shared folder for all Baked Assets from this Pull Config (project-relative).")]
        [FolderPath(RequireExistingPath = false)]
        [SerializeField]
        string _outputFolder = "Assets/AerisynDataConfig/Baked";


        [FoldoutGroup("Pull targets")]
        [Tooltip(
            "Explicit Config Types to Pull. Pull only processes this list (no auto-scan of work). " +
            "Google tab title matches the type name by default.")]
        [ListDrawerSettings(ShowIndexLabels = true, DraggableItems = true)]
        [TypeFilter(nameof(FilterConfigTypes))]
        [SerializeField]
        Type[] _configTypes = Array.Empty<Type>();

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


        /// <summary>Explicit Config Types Pull will create or overwrite.</summary>
        public Type[] ConfigTypes => _configTypes;

        #endregion


        #region Type filter

        /// <summary>
        /// Odin picker candidates only (concrete ConfigTypeAsset subclasses).
        /// Does not choose what Pull runs; that is the explicit serialized list above.
        /// </summary>
        static IEnumerable<Type> FilterConfigTypes()
        {
            Type baseType = typeof(ConfigTypeAsset);
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (int a = 0; a < assemblies.Length; a++)
            {
                Type[] types;
                try
                {
                    types = assemblies[a].GetTypes();
                }
                catch (ReflectionTypeLoadException loadException)
                {
                    types = loadException.Types;
                }

                if (types == null)
                    continue;

                for (int t = 0; t < types.Length; t++)
                {
                    Type type = types[t];
                    if (type == null || type.IsAbstract || !baseType.IsAssignableFrom(type))
                        continue;

                    yield return type;
                }
            }
        }

        #endregion


    }
}
