using System;
using System.Collections.Generic;
using Aerisyn.DataConfigSheet;
using Cathei.BakingSheet;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Aerisyn.DataConfigSheet.Samples.BasicBake
{
    /// <summary>
    /// Sample single-file bake target: editable Items list (no BakingSheet row sub-assets).
    /// </summary>
    [CreateAssetMenu(
        fileName = "DemoBakedData",
        menuName = "Aerisyn/Data Config Sheet/Demo Baked Data",
        order = 13)]
    public sealed class DemoBakedDataAsset : BakedSheetContainerAsset
    {
        #region Serialized data

        [Serializable]
        public sealed class ItemRow
        {
            public string Id;
            public string Name;
            public int Price;
        }


        [InfoBox(
            "Editable for fast tests. Re-bake from Google overwrites this list.",
            InfoMessageType.Info)]
        [ListDrawerSettings(ShowIndexLabels = true, DraggableItems = true)]
        [SerializeField]
        List<ItemRow> _items = new List<ItemRow>();

        #endregion


        #region Public API

        public IReadOnlyList<ItemRow> Items => _items;

        #endregion


        #region Bake apply

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
