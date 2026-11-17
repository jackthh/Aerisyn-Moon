using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Aerisyn.DataConfigSheet
{
    /// <summary>
    /// The single project settings asset for Data Config: Environment Keys, Feature Keys, the active
    /// Environment Key, Environment Key x Feature Key spreadsheet id bindings, title-prefix overrides,
    /// Config Manifest path and required Config Types, Google auth, and the Pull All folder.
    /// Lives at <see cref="DefaultAssetPath"/>; a new asset is seeded with Dev, Test, Production and no Feature Keys.
    /// </summary>
    [InfoBox(
        "One settings asset for the project. Keys are defined here once; bindings and the active environment " +
        "only pick from these lists.",
        InfoMessageType.Info)]
    public sealed class DataConfigSettings : ScriptableObject, IDataConfigKeySource
    {


        #region Constants

        /// <summary>Fixed project-relative path of the one settings asset.</summary>
        public const string DefaultAssetPath = "Assets/AerisynDataConfig/DataConfigSettings.asset";

        #endregion


        #region Keys

        // Field initializers (not Reset) so the seed applies to every CreateInstance path.
        // Unity overwrites them with saved values when an existing asset deserializes.
        [FoldoutGroup("Keys")]
        [Tooltip("Environment Keys games can pick. Seeded with Dev, Test, Production.")]
        [ListDrawerSettings(ShowFoldout = false, DraggableItems = true)]
        [SerializeField]
        List<string> _environmentKeys = new List<string>(KeyListRules.CreateSeedEnvironmentKeys());


        [FoldoutGroup("Keys")]
        [Tooltip("Feature Keys for this project. Empty until the game adds its own.")]
        [ListDrawerSettings(ShowFoldout = false, DraggableItems = true)]
        [SerializeField]
        List<string> _featureKeys = new List<string>(KeyListRules.CreateSeedFeatureKeys());

        #endregion


        #region Active environment

        [Tooltip("The project's active Environment Key. Pick from the Environment Keys list.")]
        [ValueDropdown("@EnvironmentKeys")]
        [SerializeField]
        string _activeEnvironmentKey = "";

        #endregion


        #region Bindings

        [Tooltip("Environment Key x Feature Key -> Source Sheet spreadsheet id.")]
        [ListDrawerSettings(ShowFoldout = false, DraggableItems = true)]
        [SerializeField]
        List<EnvironmentFeatureBindingEntry> _bindings = new List<EnvironmentFeatureBindingEntry>();


        // Empty by default: Dev->[Dev], Test->[Tester], Production->[Production] come from the package.
        [Tooltip(
            "Optional Source Sheet title prefix per Environment Key. Package defaults: " +
            "Dev -> [Dev], Test -> [Tester], Production -> [Production]. Required for game-added keys.")]
        [ListDrawerSettings(ShowFoldout = false, DraggableItems = true)]
        [SerializeField]
        List<EnvironmentTitlePrefixEntry> _titlePrefixOverrides = new List<EnvironmentTitlePrefixEntry>();

        #endregion


        #region Config Manifest

        [FoldoutGroup("Config Manifest")]
        [Tooltip(
            "Project-relative path of the Config Manifest asset (e.g. Assets/Config/ConfigManifest.asset). " +
            "Pull All creates or overwrites it after every Pull Config passes. Leave blank to skip the manifest.")]
        [FilePath(Extensions = "asset", RequireExistingPath = false)]
        [SerializeField]
        string _manifestAssetPath = "";


        [FoldoutGroup("Config Manifest")]
        [Tooltip(
            "Config Types that must appear in the Config Manifest. Pull All and the build gate fail when one is missing.")]
        [ValueDropdown(nameof(ConfigTypeNameChoices), DropdownTitle = "Required Config Types")]
        [ListDrawerSettings(ShowFoldout = false, DraggableItems = true)]
        [SerializeField]
        List<string> _requiredConfigTypeNames = new List<string>();

        #endregion


        #region Google auth

        [FoldoutGroup("Google auth")]
        [Tooltip("OAuthUser = browser sign-in (default). ServiceAccount = robot JSON for CI.")]
        [SerializeField]
        GoogleAuthMode _authMode = GoogleAuthMode.OAuthUser;


        [FoldoutGroup("Google auth")]
        [ShowIf(nameof(_authMode), GoogleAuthMode.OAuthUser)]
        [Tooltip("Desktop OAuth client_secrets JSON from Google Cloud (gitignored).")]
        [FilePath(Extensions = "json", RequireExistingPath = false)]
        [SerializeField]
        string _oauthClientSecretsPath = "";


        // Machine-local under UserSettings (gitignored), never in Assets.
        [FoldoutGroup("Google auth")]
        [ShowIf(nameof(_authMode), GoogleAuthMode.OAuthUser)]
        [Tooltip("Per-machine authorized_user token written after Sign In (gitignored).")]
        [FilePath(Extensions = "json", RequireExistingPath = false)]
        [SerializeField]
        string _oauthUserTokenPath = "UserSettings/AerisynDataConfig/oauth-user-token.json";


        [FoldoutGroup("Google auth")]
        [ShowIf(nameof(_authMode), GoogleAuthMode.ServiceAccount)]
        [Tooltip("Service-account JSON. Share the sheet with that robot email as Viewer.")]
        [FilePath(Extensions = "json", RequireExistingPath = false)]
        [SerializeField]
        string _serviceAccountCredentialPath = "";

        #endregion


        #region Pull All

        // Blank fails Pull All (PullConfigFolderScope); there is no whole-project fallback.
        [FoldoutGroup("Pull All")]
        [Tooltip("Pull All runs every Pull Config under this folder, recursively. Blank fails.")]
        [FolderPath(RequireExistingPath = false)]
        [SerializeField]
        string _pullAllFolderPath = "";

        #endregion


        #region Public API

        /// <summary>Environment Keys in definition order.</summary>
        public IReadOnlyList<string> EnvironmentKeys => _environmentKeys;


        /// <summary>Feature Keys in definition order.</summary>
        public IReadOnlyList<string> FeatureKeys => _featureKeys;


        /// <summary>The project's active Environment Key (empty until picked).</summary>
        public string ActiveEnvironmentKey => _activeEnvironmentKey;


        /// <summary>Project-relative Config Manifest asset path; blank when the project keeps no manifest.</summary>
        public string ManifestAssetPath => _manifestAssetPath;


        /// <summary>Config Type names the Config Manifest must contain (completeness check).</summary>
        public IReadOnlyList<string> RequiredConfigTypeNames => _requiredConfigTypeNames;


        /// <summary>OAuth user (default) or service-account auth.</summary>
        public GoogleAuthMode AuthMode => _authMode;


        /// <summary>Desktop OAuth client_secrets.json path.</summary>
        public string OAuthClientSecretsPath => _oauthClientSecretsPath;


        /// <summary>Per-machine authorized_user token path.</summary>
        public string OAuthUserTokenPath => _oauthUserTokenPath;


        /// <summary>Service-account credential JSON path.</summary>
        public string ServiceAccountCredentialPath => _serviceAccountCredentialPath;


        /// <summary>Folder Pull All scans for Pull Configs (blank until picked).</summary>
        public string PullAllFolderPath => _pullAllFolderPath;


        /// <summary>
        /// Source Sheet spreadsheet id for the Feature Key in the active environment.
        /// Throws InvalidOperationException when the active environment or Feature Key is not in its list
        /// or no id is bound (see SpreadsheetIdResolver).
        /// </summary>
        public string ResolveSpreadsheetId(string featureKey)
        {
            return SpreadsheetIdResolver.Resolve(
                EnvironmentKeys,
                FeatureKeys,
                ToBindings(),
                _activeEnvironmentKey,
                featureKey);
        }


        /// <summary>
        /// Pull Config asset paths under the Pull All folder (recursive, stable order).
        /// Throws InvalidOperationException when the folder path is blank (see PullConfigFolderScope).
        /// </summary>
        public string[] SelectPullConfigPaths(IReadOnlyList<string> pullConfigAssetPaths)
        {
            return PullConfigFolderScope.Select(_pullAllFolderPath, pullConfigAssetPaths);
        }


        /// <summary>Maps serialized title prefix rows into pure overrides for the Title Check seam.</summary>
        public EnvironmentTitlePrefix[] ToTitlePrefixOverrides()
        {
            return MapEntries(
                _titlePrefixOverrides,
                entry => entry.ToTitlePrefix(),
                () => new EnvironmentTitlePrefix(null, null));
        }


        /// <summary>Maps serialized rows into pure bindings for the resolve seam.</summary>
        public EnvironmentFeatureBinding[] ToBindings()
        {
            return MapEntries(
                _bindings,
                entry => entry.ToBinding(),
                () => new EnvironmentFeatureBinding(null, null, null));
        }

        #endregion


        #region Mapping helpers

        // Serialized rows may contain null slots; map them to a blank pure value so the resolver
        // reports the bad row instead of throwing a NullReferenceException.
        static TPure[] MapEntries<TEntry, TPure>(
            List<TEntry> entries,
            Func<TEntry, TPure> map,
            Func<TPure> createBlank)
            where TEntry : class
        {
            if (entries == null || entries.Count == 0)
                return Array.Empty<TPure>();

            TPure[] mapped = new TPure[entries.Count];
            for (int i = 0; i < entries.Count; i++)
                mapped[i] = entries[i] == null ? createBlank() : map(entries[i]);

            return mapped;
        }

        #endregion


        #region Picker helpers

        IEnumerable<string> ConfigTypeNameChoices()
        {
            foreach (Type configType in PullConfigTypeEntry.FilterConfigTypes())
                yield return configType.Name;
        }

        #endregion


    }
}
