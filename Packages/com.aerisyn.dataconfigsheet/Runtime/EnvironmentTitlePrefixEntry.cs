using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Aerisyn.DataConfigSheet
{
    /// <summary>
    /// One serialized Data Config Settings row overriding the expected Source Sheet title prefix
    /// for an Environment Key. The key is picked from the Data Config Settings Environment Key list; the
    /// prefix is the only free-typed value.
    /// </summary>
    [Serializable]
    [HideReferenceObjectPicker]
    [InlineProperty]
    public sealed class EnvironmentTitlePrefixEntry
    {


        #region Fields

        [HorizontalGroup("TitlePrefix", Width = 110)]
        [HideLabel]
        [ValueDropdown(EnvironmentFeatureBindingEntry.EnvironmentKeyPicker, DropdownTitle = "Environment Key")]
        [SerializeField]
        string _environmentKey = "";


        [HorizontalGroup("TitlePrefix")]
        [HideLabel]
        [Tooltip("Required Source Sheet title prefix, case-sensitive, brackets included (e.g. [Staging]).")]
        [SerializeField]
        string _prefix = "";

        #endregion


        #region Public API

        /// <summary>Environment Key this override applies to.</summary>
        public string EnvironmentKey => _environmentKey;


        /// <summary>Title prefix the Source Sheet must start with.</summary>
        public string Prefix => _prefix;


        /// <summary>Maps this row into the pure Title Check seam type.</summary>
        public EnvironmentTitlePrefix ToTitlePrefix()
        {
            return new EnvironmentTitlePrefix(_environmentKey, _prefix);
        }

        #endregion


    }
}
