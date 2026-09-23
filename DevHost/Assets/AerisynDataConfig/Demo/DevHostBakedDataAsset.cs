using System;
using System.Collections.Generic;
using Aerisyn.DataConfigSheet;
using Cathei.BakingSheet;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Aerisyn.DataConfigSheet.DevHost
{
    /// <summary>
    /// Single editable bake target for DevHost: one SO file, rows as a serializable list.
    /// Change values here for fast local tests; re-bake from Google overwrites this list.
    /// </summary>
    [CreateAssetMenu(
        fileName = "DevHostBakedData",
        menuName = "Aerisyn/Data Config Sheet/DevHost Baked Data",
        order = 12)]
    public sealed class DevHostBakedDataAsset : BakedSheetContainerAsset
    {
        #region Serialized data

        [Serializable]
        public sealed class ItemRow
        {
            [Tooltip("Matches the Id column on the Google Sheet.")]
            public string Id;

            public string Name;
            public int Price;
        }


        [InfoBox(
            "Editable for fast tests. Google Sheet is still the official source of truth; " +
            "the next bake overwrites this list. Do not push edits back to Google.",
            InfoMessageType.Info)]
        [ListDrawerSettings(ShowIndexLabels = true, DraggableItems = true, ShowFoldout = true)]
        [SerializeField]
        List<ItemRow> _items = new List<ItemRow>();

        #endregion


        #region Public API

        /// <summary>Baked item rows (Inspector-editable).</summary>
        public IReadOnlyList<ItemRow> Items => _items;

        #endregion


        #region Bake apply

        /// <summary>
        /// Clears and refills Items from the demo SheetContainer.
        /// </summary>
        public override void ApplyFromContainer(SheetContainerBase container)
        {
            DemoSheetContainer demo = container as DemoSheetContainer;
            if (demo == null)
                throw new InvalidOperationException(
                    $"Expected {nameof(DemoSheetContainer)}, got '{container?.GetType().Name ?? "null"}'.");

            if (demo.Items == null)
                throw new InvalidOperationException("DemoSheetContainer.Items is null after bake.");

            _items.Clear();
            foreach (DemoItemsSheet.Row row in demo.Items)
            {
                _items.Add(new ItemRow
                {
                    Id = row.Id,
                    Name = row.Name,
                    Price = row.Price,
                });
            }
        }

        #endregion
    }
}
