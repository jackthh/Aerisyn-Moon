using System.Collections.Generic;

namespace Aerisyn.DataConfigSheet
{
    /// <summary>
    /// Owner of the Environment Key and Feature Key lists. Binding and title-prefix rows pick keys
    /// from whichever source is the Inspector root, so no row accepts a free-typed key.
    /// </summary>
    public interface IDataConfigKeySource
    {


        /// <summary>Environment Keys in definition order.</summary>
        IReadOnlyList<string> EnvironmentKeys { get; }


        /// <summary>Feature Keys in definition order.</summary>
        IReadOnlyList<string> FeatureKeys { get; }


    }
}
