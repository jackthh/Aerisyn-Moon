using Aerisyn.DataConfigSheet;
using NUnit.Framework;

namespace Aerisyn.DataConfigSheet.Tests
{
    /// <summary>
    /// Parse seam: Local Only fields skip Header Row match, never bind, warn-and-ignore when present.
    /// </summary>
    public sealed class LocalOnlyParserTests
    {


        #region Types

        public sealed class LocalOnlyConfig
        {
            public List<LocalOnlyRow> items = new List<LocalOnlyRow>();
        }


        public sealed class LocalOnlyRow
        {
            public string id = "";
            public int turnSpeed;

            [LocalOnly]
            public int accelerateTurnSpeed;
        }


        public sealed class LocalOnlyAliasConfig
        {
            public List<LocalOnlyAliasRow> items = new List<LocalOnlyAliasRow>();
        }


        public sealed class LocalOnlyAliasRow
        {
            public string id = "";
            public int turnSpeed;

            [LocalOnly]
            [ColumnAlias("Accel Turn")]
            public int accelerateTurnSpeed;
        }


        public sealed class NestLocalOnlyConfig
        {
            public List<NestLocalOnlyWeapon> items = new List<NestLocalOnlyWeapon>();
        }


        public sealed class NestLocalOnlyWeapon
        {
            public string id = "";
            public List<NestLocalOnlyUpgrade> upgrades = new List<NestLocalOnlyUpgrade>();
        }


        public sealed class NestLocalOnlyUpgrade
        {
            public int upgrade_level;

            [LocalOnly]
            public int derivedBonus;

            public List<int> bonus_stats = new List<int>();
        }


        public sealed class LocalOnlyArrayConfig
        {
            public List<LocalOnlyArrayRow> items = new List<LocalOnlyArrayRow>();
        }


        public sealed class LocalOnlyArrayRow
        {
            public string id = "";

            [LocalOnly]
            public List<int> localBonuses = new List<int>();

            public int power;
        }

        #endregion


        #region Absent Local Only

        [Test]
        public void ParseInto_LocalOnlyColumnAbsent_SucceedsAndLeavesDefault()
        {
            // Sheet omits accelerateTurnSpeed; Header Row match must still succeed
            SheetGrid grid = SheetGrid.FromRows(new[]
            {
                new[] { "id", "turnSpeed" },
                new[] { "dash", "12" },
            });

            LocalOnlyConfig target = new LocalOnlyConfig();
            VerticalNestParseResult result = VerticalNestParser.ParseInto(target, grid);

            Assert.That(result.Success, Is.True, () => result.Errors[0].Message);
            Assert.That(target.items, Has.Count.EqualTo(1));
            Assert.That(target.items[0].id, Is.EqualTo("dash"));
            Assert.That(target.items[0].turnSpeed, Is.EqualTo(12));
            Assert.That(target.items[0].accelerateTurnSpeed, Is.EqualTo(0));
        }

        #endregion


        #region Present Local Only warn-and-ignore

        [Test]
        public void ParseInto_LocalOnlyColumnPresent_WarnsIgnoresAndLeavesDefault()
        {
            // Accidental full Header Row: Local Only column present with a cell value
            SheetGrid grid = SheetGrid.FromRows(new[]
            {
                new[] { "id", "turnSpeed", "accelerateTurnSpeed" },
                new[] { "dash", "12", "99" },
            });

            LocalOnlyConfig target = new LocalOnlyConfig();
            VerticalNestParseResult result = VerticalNestParser.ParseInto(target, grid);

            Assert.That(result.Success, Is.True, () => result.Errors[0].Message);
            Assert.That(target.items[0].accelerateTurnSpeed, Is.EqualTo(0));
            Assert.That(result.Warnings, Has.Count.EqualTo(1));
            Assert.That(result.Warnings[0], Does.Contain("Local Only"));
            Assert.That(result.Warnings[0], Does.Contain("accelerateTurnSpeed"));
        }


        [Test]
        public void ParseInto_LocalOnlyAliasColumnPresent_WarningMentionsAlias()
        {
            SheetGrid grid = SheetGrid.FromRows(new[]
            {
                new[] { "id", "turnSpeed", "Accel Turn" },
                new[] { "dash", "12", "99" },
            });

            LocalOnlyAliasConfig target = new LocalOnlyAliasConfig();
            VerticalNestParseResult result = VerticalNestParser.ParseInto(target, grid);

            Assert.That(result.Success, Is.True, () => result.Errors[0].Message);
            Assert.That(target.items[0].accelerateTurnSpeed, Is.EqualTo(0));
            Assert.That(result.Warnings, Has.Count.EqualTo(1));
            Assert.That(result.Warnings[0], Does.Contain("Accel Turn"));
            Assert.That(result.Warnings[0], Does.Contain("accelerateTurnSpeed"));
        }

        #endregion


        #region Nest-level Local Only

        [Test]
        public void ParseInto_NestLevelLocalOnlyAbsent_Succeeds()
        {
            // Child nest Local Only field omitted from sheet
            SheetGrid grid = SheetGrid.FromRows(new[]
            {
                new[] { "id", "upgrade_level", "bonus_stats" },
                new[] { "sword", "1", "10" },
                new[] { "", "", "15" },
            });

            NestLocalOnlyConfig target = new NestLocalOnlyConfig();
            VerticalNestParseResult result = VerticalNestParser.ParseInto(target, grid);

            Assert.That(result.Success, Is.True, () => result.Errors[0].Message);
            Assert.That(target.items, Has.Count.EqualTo(1));
            Assert.That(target.items[0].upgrades, Has.Count.EqualTo(1));
            Assert.That(target.items[0].upgrades[0].upgrade_level, Is.EqualTo(1));
            Assert.That(target.items[0].upgrades[0].bonus_stats, Is.EqualTo(new[] { 10, 15 }));
            Assert.That(target.items[0].upgrades[0].derivedBonus, Is.EqualTo(0));
            Assert.That(result.Warnings, Is.Empty);
        }


        [Test]
        public void ParseInto_NestLevelLocalOnlyPresent_WarnsAndLeavesDefault()
        {
            SheetGrid grid = SheetGrid.FromRows(new[]
            {
                new[] { "id", "upgrade_level", "derivedBonus", "bonus_stats" },
                new[] { "sword", "1", "77", "10" },
            });

            NestLocalOnlyConfig target = new NestLocalOnlyConfig();
            VerticalNestParseResult result = VerticalNestParser.ParseInto(target, grid);

            Assert.That(result.Success, Is.True, () => result.Errors[0].Message);
            Assert.That(target.items[0].upgrades[0].derivedBonus, Is.EqualTo(0));
            Assert.That(target.items[0].upgrades[0].bonus_stats, Is.EqualTo(new[] { 10 }));
            Assert.That(result.Warnings, Has.Count.EqualTo(1));
            Assert.That(result.Warnings[0], Does.Contain("derivedBonus"));
        }

        #endregion


        #region Primitive array Local Only

        [Test]
        public void ParseInto_LocalOnlyPrimitiveArrayAbsent_Succeeds()
        {
            SheetGrid grid = SheetGrid.FromRows(new[]
            {
                new[] { "id", "power" },
                new[] { "dash", "5" },
            });

            LocalOnlyArrayConfig target = new LocalOnlyArrayConfig();
            VerticalNestParseResult result = VerticalNestParser.ParseInto(target, grid);

            Assert.That(result.Success, Is.True, () => result.Errors[0].Message);
            Assert.That(target.items[0].power, Is.EqualTo(5));
            Assert.That(target.items[0].localBonuses, Is.Empty);
        }


        [Test]
        public void ParseInto_LocalOnlyPrimitiveArrayPresent_WarnsAndLeavesEmpty()
        {
            SheetGrid grid = SheetGrid.FromRows(new[]
            {
                new[] { "id", "localBonuses", "power" },
                new[] { "dash", "9", "5" },
            });

            LocalOnlyArrayConfig target = new LocalOnlyArrayConfig();
            VerticalNestParseResult result = VerticalNestParser.ParseInto(target, grid);

            Assert.That(result.Success, Is.True, () => result.Errors[0].Message);
            Assert.That(target.items[0].power, Is.EqualTo(5));
            Assert.That(target.items[0].localBonuses, Is.Empty);
            Assert.That(result.Warnings, Has.Count.EqualTo(1));
            Assert.That(result.Warnings[0], Does.Contain("localBonuses"));
        }

        #endregion


        #region Regression

        [Test]
        public void ParseInto_UnmarkedMissingHeader_StillFails()
        {
            // Local Only must not weaken Header Row match for unmarked fields
            SheetGrid grid = SheetGrid.FromRows(new[]
            {
                new[] { "id" },
                new[] { "dash" },
            });

            LocalOnlyConfig target = new LocalOnlyConfig();
            VerticalNestParseResult result = VerticalNestParser.ParseInto(target, grid);

            Assert.That(result.Success, Is.False);
            Assert.That(result.Errors[0].Message, Does.Contain("Header Row"));
            Assert.That(result.Errors[0].Message, Does.Contain("turnSpeed"));
        }

        #endregion


    }
}
