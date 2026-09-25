using System;
using System.Collections.Generic;
using System.Reflection;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Aerisyn.DataConfigSheet
{
    /// <summary>
    /// One owned Config Type candidate on a Pull Config, with Include In Pull gate.
    /// New entries default included so adding a type matches "I want this Pulled."
    /// </summary>
    [Serializable]
    public sealed class PullConfigTypeEntry
    {


        #region Fields

        // Bare checkbox (no long "Include In Pull" label) keeps the candidate list compact.
        [HorizontalGroup("Entry", Width = 18)]
        [HideLabel]
        [SerializeField]
        bool _includeInPull = true;


        [HorizontalGroup("Entry")]
        [HideLabel]
        [ValueDropdown(
            nameof(FilterConfigTypes),
            IsUniqueList = true,
            DrawDropdownForListElements = true,
            DropdownTitle = "Config Types")]
        [SerializeField]
        Type _configType;

        #endregion


        #region Construction

        public PullConfigTypeEntry()
        {
        }


        public PullConfigTypeEntry(Type configType, bool includeInPull = true)
        {
            _configType = configType;
            _includeInPull = includeInPull;
        }

        #endregion


        #region Public API

        /// <summary>When true, this candidate is fetched and baked on the next Pull.</summary>
        public bool IncludeInPull => _includeInPull;


        /// <summary>Config Type asset type for this candidate (may be null until picked).</summary>
        public Type ConfigType => _configType;


        /// <summary>Maps this Inspector entry into the pure inclusion/validation seam.</summary>
        public PullTypeCandidate ToCandidate()
        {
            return new PullTypeCandidate(_configType, _includeInPull);
        }

        #endregion


        #region Type filter

        /// <summary>
        /// ValueDropdown candidates only (concrete ConfigTypeAsset subclasses).
        /// Does not choose what Pull runs; that is Include In Pull on each entry.
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
