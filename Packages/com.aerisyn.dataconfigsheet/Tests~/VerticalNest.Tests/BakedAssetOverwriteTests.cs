using Aerisyn.DataConfigSheet;
using NUnit.Framework;

namespace Aerisyn.DataConfigSheet.Tests
{
    /// <summary>
    /// Baked Asset path + in-place overwrite helpers (GUID-stable re-Pull policy without AssetDatabase).
    /// </summary>
    public sealed class BakedAssetOverwriteTests
    {


        #region Types

        public sealed class SampleConfig
        {
            public List<SampleRow> items = new List<SampleRow>();
        }


        public sealed class SampleRow
        {
            public string id = "";
        }

        #endregion


        #region Tests

        [Test]
        public void ForConfigType_UsesSharedFolderAndTypeName()
        {
            string path = BakedAssetPath.ForConfigType("Assets/AerisynDataConfig/Baked", typeof(SampleConfig));
            Assert.That(path, Is.EqualTo("Assets/AerisynDataConfig/Baked/SampleConfig.asset"));
        }


        [Test]
        public void CopyRootItems_OverwritesDestinationList_PreservingDestinationIdentity()
        {
            SampleConfig existing = new SampleConfig();
            existing.items.Add(new SampleRow { id = "old" });
            object identity = existing;

            SampleConfig scratch = new SampleConfig();
            scratch.items.Add(new SampleRow { id = "new-a" });
            scratch.items.Add(new SampleRow { id = "new-b" });

            BakedAssetItemsCopy.CopyRootItems(scratch, existing);

            Assert.That(ReferenceEquals(identity, existing), Is.True);
            Assert.That(existing.items, Has.Count.EqualTo(2));
            Assert.That(existing.items[0].id, Is.EqualTo("new-a"));
            Assert.That(existing.items[1].id, Is.EqualTo("new-b"));
        }


        [Test]
        public void ParseInto_ThenCopyRootItems_RoundTripsWeaponsFixtureOntoExistingTarget()
        {
            SheetGrid grid = SheetGrid.FromRows(new[]
            {
                new[] { "", "", "!!!", "", "" },
                new[] { "id", "name", "note", "upgrade_level", "bonus_stats" },
                new[] { "sword", "Iron Sword", "x", "1", "10" },
                new[] { "", "", "", "", "15" },
            });

            var scratch = new WeaponsVerticalNestFixtureTests.WeaponsConfig();
            Assert.That(VerticalNestParser.ParseInto(scratch, grid).Success, Is.True);

            var existing = new WeaponsVerticalNestFixtureTests.WeaponsConfig();
            existing.items.Add(new WeaponsVerticalNestFixtureTests.Weapon { id = "stale" });

            BakedAssetItemsCopy.CopyRootItems(scratch, existing);

            Assert.That(existing.items, Has.Count.EqualTo(1));
            Assert.That(existing.items[0].id, Is.EqualTo("sword"));
            Assert.That(existing.items[0].upgrades[0].bonus_stats, Is.EqualTo(new[] { 10, 15 }));
        }

        #endregion


    }
}
