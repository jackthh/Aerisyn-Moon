using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Aerisyn.DataConfigSheet
{
    /// <summary>
    /// Maps each Environment Key × Feature Key to a Source Sheet spreadsheet id and stores the one
    /// active Environment Key for the project. Pull reads the active environment here and resolves
    /// the spreadsheet id from a Feature Key (SpreadsheetIdResolver). Keys are only picked from the
    /// referenced key lists.
    /// </summary>
    [CreateAssetMenu(
        fileName = "EnvironmentProfile",
        menuName = "Aerisyn/Data Config Sheet/Environment Profile",
        order = 3)]
    [InfoBox(
        "One active environment for the project. Each row binds Environment Key × Feature Key to a " +
        "Source Sheet spreadsheet id. Pull resolves the id from the active environment.",
        InfoMessageType.Info)]
    public sealed class EnvironmentProfile : ScriptableObject
    {


        #region Key lists

        [FoldoutGroup("Key lists")]
        [Tooltip("The single Environment Key definition site.")]
        [SerializeField]
        EnvironmentKeyList _environmentKeyList;


        [FoldoutGroup("Key lists")]
        [Tooltip("The single Feature Key definition site.")]
        [SerializeField]
        FeatureKeyList _featureKeyList;

        #endregion


        #region Active environment

        // Stored once here; Pull and Pull All read it instead of keeping their own copy.
        [Tooltip("The project's active Environment Key. Pick from the Environment Key list.")]
        [ValueDropdown("@EnvironmentKeys")]
        [SerializeField]
        string _activeEnvironmentKey = "";

        #endregion


        #region Bindings

        [Tooltip("Environment Key × Feature Key → Source Sheet spreadsheet id.")]
        [ListDrawerSettings(ShowFoldout = false, DraggableItems = true)]
        [SerializeField]
        List<EnvironmentFeatureBindingEntry> _bindings = new List<EnvironmentFeatureBindingEntry>();

        #endregion


        #region Title prefixes

        // Empty by default: Dev→[Dev], Test→[Tester], Production→[Production] come from the package.
        // Add a row to change one, or to guard a game-added Environment Key.
        [Tooltip(
            "Optional Source Sheet title prefix per Environment Key. Package defaults: " +
            "Dev → [Dev], Test → [Tester], Production → [Production]. Required for game-added keys.")]
        [ListDrawerSettings(ShowFoldout = false, DraggableItems = true)]
        [SerializeField]
        List<EnvironmentTitlePrefixEntry> _titlePrefixOverrides = new List<EnvironmentTitlePrefixEntry>();

        #endregion


        #region Config Manifest

        // Project-chosen path; Pull All overwrites it after a fully successful run. Blank = no manifest.
        [FoldoutGroup("Config Manifest")]
        [Tooltip(
            "Project-relative path of the Config Manifest asset (e.g. Assets/Config/ConfigManifest.asset). " +
            "Pull All creates or overwrites it after every Pull Config passes. Leave blank to skip the manifest.")]
        [FilePath(Extensions = "asset", RequireExistingPath = false)]
        [SerializeField]
        string _manifestAssetPath = "";


        // Config Type names (= Baked Asset file names) the manifest must contain.
        [FoldoutGroup("Config Manifest")]
        [Tooltip(
            "Config Types that must appear in the Config Manifest. Pull All and the build gate fail when one is missing, " +
            "so a Pull Config that was never pulled cannot go unnoticed.")]
        [ValueDropdown(nameof(ConfigTypeNameChoices), DropdownTitle = "Required Config Types")]
        [ListDrawerSettings(ShowFoldout = false, DraggableItems = true)]
        [SerializeField]
        List<string> _requiredConfigTypeNames = new List<string>();

        #endregion


        #region Public API

        /// <summary>The project's active Environment Key (empty until picked).</summary>
        public string ActiveEnvironmentKey => _activeEnvironmentKey;


        /// <summary>Project-relative Config Manifest asset path; blank when the project keeps no manifest.</summary>
        public string ManifestAssetPath => _manifestAssetPath;


        /// <summary>Config Type names the Config Manifest must contain (completeness check).</summary>
        public IReadOnlyList<string> RequiredConfigTypeNames => _requiredConfigTypeNames;


        /// <summary>Environment Keys defined by the referenced list; empty when no list is assigned.</summary>
        public IReadOnlyList<string> EnvironmentKeys =>
            _environmentKeyList == null ? Array.Empty<string>() : _environmentKeyList.Keys;


        /// <summary>Feature Keys defined by the referenced list; empty when no list is assigned.</summary>
        public IReadOnlyList<string> FeatureKeys =>
            _featureKeyList == null ? Array.Empty<string>() : _featureKeyList.Keys;


        /// <summary>
        /// Source Sheet spreadsheet id for the Feature Key in the active environment.
        /// Throws InvalidOperationException when the key lists are missing, the active environment
        /// or Feature Key is not in its list, or no id is bound (see SpreadsheetIdResolver).
        /// </summary>
        public string ResolveSpreadsheetId(string featureKey)
        {
            if (_environmentKeyList == null)
                throw new InvalidOperationException($"EnvironmentProfile '{name}' has no Environment Key List assigned.");

            if (_featureKeyList == null)
                throw new InvalidOperationException($"EnvironmentProfile '{name}' has no Feature Key List assigned.");

            return SpreadsheetIdResolver.Resolve(
                EnvironmentKeys,
                FeatureKeys,
                ToBindings(),
                _activeEnvironmentKey,
                featureKey);
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

        // Serialized rows may contain null slots (Odin/Inspector); map them to a blank pure value so the
        // downstream resolver reports the bad row instead of throwing a NullReferenceException.
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

        // Names of concrete Config Types (same scan as the Pull Config type picker); the name is what
        // Baked Asset files and manifest rows use.
        IEnumerable<string> ConfigTypeNameChoices()
        {
            foreach (Type configType in PullConfigTypeEntry.FilterConfigTypes())
                yield return configType.Name;
        }

        #endregion


    }
}
