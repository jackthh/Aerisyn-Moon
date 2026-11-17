using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Aerisyn.DataConfigSheet
{
    /// <summary>
    /// The single definition site for Environment Keys. A new asset is seeded with Dev, Test and
    /// Production (KeyListRules); games may add more. Environment Profile and other Inspectors pick
    /// from this list and never accept a free-typed environment string.
    /// </summary>
    [CreateAssetMenu(
        fileName = "EnvironmentKeyList",
        menuName = "Aerisyn/Data Config Sheet/Environment Key List",
        order = 1)]
    [InfoBox(
        "Single definition of Environment Keys (seeded Dev, Test, Production). " +
        "Add your own stages here; everything else only picks from this list.",
        InfoMessageType.Info)]
    public sealed class EnvironmentKeyList : ScriptableObject
    {


        #region Fields

        // Field initializer (not Reset) so the seed applies to every CreateInstance path.
        // Unity overwrites it with the saved list when an existing asset deserializes.
        [Tooltip("Environment Keys games can pick. Seeded with Dev, Test, Production.")]
        [SerializeField]
        List<string> _keys = new List<string>(KeyListRules.CreateSeedEnvironmentKeys());

        #endregion


        #region Public API

        /// <summary>Environment Keys in definition order.</summary>
        public IReadOnlyList<string> Keys => _keys;


        /// <summary>True when the key is defined in this list (ordinal, case-sensitive).</summary>
        public bool Contains(string environmentKey)
        {
            return KeyListRules.IsMember(_keys, environmentKey);
        }

        #endregion


    }
}
