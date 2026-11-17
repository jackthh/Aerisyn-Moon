#nullable disable
namespace Aerisyn.DataConfigSheet
{
    /// <summary>
    /// One per-Environment-Key override of the expected Source Sheet title prefix.
    /// Pure seam type (no Unity serialization); Data Config Settings maps its serialized rows into this.
    /// </summary>
    public readonly struct EnvironmentTitlePrefix
    {


        #region Fields

        readonly string _environmentKey;
        readonly string _prefix;

        #endregion


        public EnvironmentTitlePrefix(string environmentKey, string prefix)
        {
            _environmentKey = environmentKey;
            _prefix = prefix;
        }


        #region Public API

        /// <summary>Environment Key this override applies to.</summary>
        public string EnvironmentKey => _environmentKey;


        /// <summary>Title prefix the Source Sheet must start with (case-sensitive, brackets included).</summary>
        public string Prefix => _prefix;

        #endregion


    }
}
