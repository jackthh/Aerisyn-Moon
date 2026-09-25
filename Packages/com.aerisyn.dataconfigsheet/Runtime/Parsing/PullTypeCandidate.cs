#nullable disable
using System;

namespace Aerisyn.DataConfigSheet
{
    /// <summary>
    /// One Pull Config candidate for inclusion rules: Config Type plus Include In Pull.
    /// Pure seam type (no Unity serialization); PullConfig entries map into this for validation/fetch.
    /// </summary>
    public readonly struct PullTypeCandidate
    {


        #region Fields

        readonly Type _configType;
        readonly bool _includeInPull;

        #endregion


        public PullTypeCandidate(Type configType, bool includeInPull)
        {
            _configType = configType;
            _includeInPull = includeInPull;
        }


        #region Public API

        /// <summary>Config Type this candidate would Pull when included.</summary>
        public Type ConfigType => _configType;


        /// <summary>When true, this candidate is fetched and baked on the next Pull.</summary>
        public bool IncludeInPull => _includeInPull;

        #endregion


    }
}
