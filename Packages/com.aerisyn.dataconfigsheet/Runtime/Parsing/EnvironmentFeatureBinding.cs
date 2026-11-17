#nullable disable
namespace Aerisyn.DataConfigSheet
{
    /// <summary>
    /// One Environment Profile cell: Environment Key × Feature Key → Source Sheet spreadsheet id.
    /// Pure seam type (no Unity serialization); the profile's serialized rows map into this for resolve.
    /// </summary>
    public readonly struct EnvironmentFeatureBinding
    {


        #region Fields

        readonly string _environmentKey;
        readonly string _featureKey;
        readonly string _spreadsheetId;

        #endregion


        public EnvironmentFeatureBinding(string environmentKey, string featureKey, string spreadsheetId)
        {
            _environmentKey = environmentKey;
            _featureKey = featureKey;
            _spreadsheetId = spreadsheetId;
        }


        #region Public API

        /// <summary>Environment Key this binding applies to.</summary>
        public string EnvironmentKey => _environmentKey;


        /// <summary>Feature Key this binding applies to.</summary>
        public string FeatureKey => _featureKey;


        /// <summary>Google Spreadsheet id (the Source Sheet) for this environment and feature.</summary>
        public string SpreadsheetId => _spreadsheetId;

        #endregion


    }
}
