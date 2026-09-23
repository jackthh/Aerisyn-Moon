using Cathei.BakingSheet;
using UnityEngine;

namespace Aerisyn.DataConfigSheet
{
    /// <summary>
    /// Game-owned factory that builds the BakingSheet SheetContainer used during editor bake.
    /// Assign an instance on BakeConfig so the package menu never hardcodes sheet schema.
    /// </summary>
    public abstract class SheetContainerFactory : ScriptableObject
    {
        /// <summary>
        /// Creates a fresh SheetContainer for one bake pass.
        /// Property names on the container must match Google Sheet tab names.
        /// </summary>
        /// <remarks>
        /// Uses Microsoft.Extensions.Logging.ILogger (BakingSheet), not UnityEngine.ILogger.
        /// </remarks>
        public abstract SheetContainerBase Create(global::Microsoft.Extensions.Logging.ILogger logger);
    }
}
