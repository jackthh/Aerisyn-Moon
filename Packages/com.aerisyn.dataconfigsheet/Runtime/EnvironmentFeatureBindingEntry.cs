using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Aerisyn.DataConfigSheet
{
    /// <summary>
    /// One serialized Data Config Settings row: Environment Key × Feature Key → Source Sheet spreadsheet id.
    /// Both keys are picked from the Data Config Settings key lists; the id is the only free-typed value.
    /// </summary>
    [Serializable]
    [HideReferenceObjectPicker]
    [InlineProperty]
    public sealed class EnvironmentFeatureBindingEntry
    {


        #region Fields

        [HorizontalGroup("Binding", Width = 110)]
        [HideLabel]
        [ValueDropdown(EnvironmentKeyPicker, DropdownTitle = "Environment Key")]
        [SerializeField]
        string _environmentKey = "";


        [HorizontalGroup("Binding", Width = 140)]
        [HideLabel]
        [ValueDropdown(FeatureKeyPicker, DropdownTitle = "Feature Key")]
        [SerializeField]
        string _featureKey = "";


        [HorizontalGroup("Binding")]
        [HideLabel]
        [Tooltip("Google Spreadsheet id from the URL (.../d/{id}/...).")]
        [SerializeField]
        string _spreadsheetId = "";

        #endregion


        #region Public API

        /// <summary>Environment Key this row applies to.</summary>
        public string EnvironmentKey => _environmentKey;


        /// <summary>Feature Key this row applies to.</summary>
        public string FeatureKey => _featureKey;


        /// <summary>Source Sheet spreadsheet id for this environment and feature.</summary>
        public string SpreadsheetId => _spreadsheetId;


        /// <summary>Maps this row into the pure resolve seam type.</summary>
        public EnvironmentFeatureBinding ToBinding()
        {
            return new EnvironmentFeatureBinding(_environmentKey, _featureKey, _spreadsheetId);
        }

        #endregion


        #region Pickers

        // Odin expression dropdowns read the Data Config Settings (the Inspector tree root) because
        // a row has no direct reference to it. Keys come only from the Data Config Settings key lists, never free text.
        public const string EnvironmentKeyPicker =
            "@((Aerisyn.DataConfigSheet.DataConfigSettings)$property.Tree.WeakTargets[0]).EnvironmentKeys";


        public const string FeatureKeyPicker =
            "@((Aerisyn.DataConfigSheet.DataConfigSettings)$property.Tree.WeakTargets[0]).FeatureKeys";

        #endregion


    }
}
