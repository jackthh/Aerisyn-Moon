using Cathei.BakingSheet;
using UnityEngine;

namespace Aerisyn.DataConfigSheet
{
    /// <summary>
    /// Game-owned bake destination: one ScriptableObject file with editable row data.
    /// Prefer this over BakingSheet's ScriptableObjectSheetExporter (which creates read-only row sub-assets).
    /// </summary>
    /// <remarks>
    /// Google Sheet remains the official source of truth. Local edits on this asset are for fast tests only
    /// and are overwritten on the next bake. Never push SO edits back to Google Sheets.
    /// </remarks>
    public abstract class BakedSheetContainerAsset : ScriptableObject
    {
        /// <summary>
        /// Replaces local data from a freshly baked SheetContainer.
        /// Implementations should clear and refill serializable lists (Inspector-editable).
        /// </summary>
        public abstract void ApplyFromContainer(SheetContainerBase container);
    }
}
