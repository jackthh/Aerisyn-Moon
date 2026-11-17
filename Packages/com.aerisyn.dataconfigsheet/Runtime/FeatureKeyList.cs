using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Aerisyn.DataConfigSheet
{
    /// <summary>
    /// The single definition site for Feature Keys. Empty by default: games own their keys
    /// (e.g. ai_ship, cannon). Environment Profile and Pull Config pick from this list and never
    /// accept a free-typed feature string.
    /// </summary>
    [CreateAssetMenu(
        fileName = "FeatureKeyList",
        menuName = "Aerisyn/Data Config Sheet/Feature Key List",
        order = 2)]
    [InfoBox(
        "Single definition of Feature Keys. Empty by default; add your game's own keys. " +
        "Everything else only picks from this list.",
        InfoMessageType.Info)]
    public sealed class FeatureKeyList : ScriptableObject
    {


        #region Fields

        [Tooltip("Feature Keys games can pick. Empty until the game adds its own.")]
        [SerializeField]
        List<string> _keys = new List<string>(KeyListRules.CreateSeedFeatureKeys());

        #endregion


        #region Public API

        /// <summary>Feature Keys in definition order.</summary>
        public IReadOnlyList<string> Keys => _keys;


        /// <summary>True when the key is defined in this list (ordinal, case-sensitive).</summary>
        public bool Contains(string featureKey)
        {
            return KeyListRules.IsMember(_keys, featureKey);
        }

        #endregion


    }
}
