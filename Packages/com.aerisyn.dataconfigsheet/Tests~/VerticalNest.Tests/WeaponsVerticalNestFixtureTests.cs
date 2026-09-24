using Aerisyn.DataConfigSheet;
using NUnit.Framework;

namespace Aerisyn.DataConfigSheet.Tests
{
    /// <summary>
    /// Parse-seam fixtures: in-memory grids → object graph (no Google, no AssetDatabase).
    /// </summary>
    public sealed class WeaponsVerticalNestFixtureTests
    {


        #region Fixture types (mirror game Config Types without Unity SO)

        public sealed class WeaponsConfig
        {
            public List<Weapon> items = new List<Weapon>();
        }


        public sealed class Weapon
        {
            public string id = "";
            public string name = "";
            public List<WeaponUpgrade> upgrades = new List<WeaponUpgrade>();
        }


        public sealed class WeaponUpgrade
        {
            public int upgrade_level;
            public List<int> bonus_stats = new List<int>();
        }

        #endregion


        #region Weapons nested fixture

        /// <summary>
        /// Weapons → upgrade levels → bonus stats with Preamble and !!! note column.
        /// </summary>
        [Test]
        public void ParseInto_WeaponsNestedGrid_BuildsObjectGraphAndSkipsIgnoreColumns()
        {
            // Preamble + !!! above designer_note; Field Headers; blank-parent Vertical Nest
            SheetGrid grid = SheetGrid.FromRows(new[]
            {
                new[] { "Weapons table", "", "designer instructions", "", "" },
                new[] { "Preamble can span multiple rows", "", "", "", "" },
                new[] { "", "", "!!!", "", "" },
                new[] { "id", "name", "designer_note", "upgrade_level", "bonus_stats" },
                new[] { "sword", "Iron Sword", "do not pull", "1", "10" },
                new[] { "", "", "still notes only", "", "15" },
                new[] { "", "", "", "2", "20" },
                new[] { "axe", "Battle Axe", "ignored", "1", "5" },
            });

            WeaponsConfig target = new WeaponsConfig();

            VerticalNestParseResult result = VerticalNestParser.ParseInto(target, grid);

            Assert.That(result.Success, Is.True, () => FormatErrors(result));
            Assert.That(target.items, Has.Count.EqualTo(2));

            Weapon sword = target.items[0];
            Assert.That(sword.id, Is.EqualTo("sword"));
            Assert.That(sword.name, Is.EqualTo("Iron Sword"));
            Assert.That(sword.upgrades, Has.Count.EqualTo(2));
            Assert.That(sword.upgrades[0].upgrade_level, Is.EqualTo(1));
            Assert.That(sword.upgrades[0].bonus_stats, Is.EqualTo(new[] { 10, 15 }));
            Assert.That(sword.upgrades[1].upgrade_level, Is.EqualTo(2));
            Assert.That(sword.upgrades[1].bonus_stats, Is.EqualTo(new[] { 20 }));

            Weapon axe = target.items[1];
            Assert.That(axe.id, Is.EqualTo("axe"));
            Assert.That(axe.name, Is.EqualTo("Battle Axe"));
            Assert.That(axe.upgrades, Has.Count.EqualTo(1));
            Assert.That(axe.upgrades[0].upgrade_level, Is.EqualTo(1));
            Assert.That(axe.upgrades[0].bonus_stats, Is.EqualTo(new[] { 5 }));
        }

        #endregion


        #region Helpers

        static string FormatErrors(VerticalNestParseResult result)
        {
            if (result.Errors == null || result.Errors.Count == 0)
                return "Parse reported failure with no errors.";

            List<string> lines = new List<string>();
            for (int i = 0; i < result.Errors.Count; i++)
            {
                VerticalNestParseError error = result.Errors[i];
                lines.Add($"R{error.Row}:C{error.Column} {error.Message}");
            }

            return string.Join("; ", lines);
        }

        #endregion


    }
}
