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


        #region Public API

        /// <summary>The project's active Environment Key (empty until picked).</summary>
        public string ActiveEnvironmentKey => _activeEnvironmentKey;


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


        /// <summary>Maps serialized rows into pure bindings for the resolve seam.</summary>
        public EnvironmentFeatureBinding[] ToBindings()
        {
            if (_bindings == null || _bindings.Count == 0)
                return Array.Empty<EnvironmentFeatureBinding>();

            EnvironmentFeatureBinding[] bindings = new EnvironmentFeatureBinding[_bindings.Count];
            for (int i = 0; i < _bindings.Count; i++)
            {
                EnvironmentFeatureBindingEntry entry = _bindings[i];
                bindings[i] = entry == null
                    ? new EnvironmentFeatureBinding(null, null, null)
                    : entry.ToBinding();
            }

            return bindings;
        }

        #endregion


    }
}
