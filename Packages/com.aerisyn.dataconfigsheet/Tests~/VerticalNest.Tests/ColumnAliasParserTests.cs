using Aerisyn.DataConfigSheet;
using NUnit.Framework;

namespace Aerisyn.DataConfigSheet.Tests
{
    /// <summary>Parse seam: Column Alias accepted on Header Row match and field bind.</summary>
    public sealed class ColumnAliasParserTests
    {


        #region Types

        public sealed class AliasConfig
        {
            public List<AliasRow> items = new List<AliasRow>();
        }


        public sealed class AliasRow
        {
            [ColumnAlias("Weapons")]
            public string id = "";

            [ColumnAlias("Power Rating")]
            public int power;
        }


        public sealed class NotesAliasConfig
        {
            public List<NotesAliasRow> items = new List<NotesAliasRow>();
        }


        public sealed class NotesAliasRow
        {
            [ColumnAlias("Weapons")]
            public string id = "";

            public int power;

            [ColumnAlias("Designer Notes")]
            public string notes = "";
        }

        #endregion


        #region Tests

        [Test]
        public void ParseInto_HeaderUsesColumnAliases_BindsFields()
        {
            SheetGrid grid = SheetGrid.FromRows(new[]
            {
                new[] { "Weapons", "Power Rating" },
                new[] { "sword", "7" },
            });

            AliasConfig target = new AliasConfig();
            VerticalNestParseResult result = VerticalNestParser.ParseInto(target, grid);

            Assert.That(result.Success, Is.True, () => result.Errors[0].Message);
            Assert.That(target.items, Has.Count.EqualTo(1));
            Assert.That(target.items[0].id, Is.EqualTo("sword"));
            Assert.That(target.items[0].power, Is.EqualTo(7));
        }


        [Test]
        public void ParseInto_HeaderMixesFieldNameAndAlias_BindsFields()
        {
            // Canonical Field Header for id; alias for power
            SheetGrid grid = SheetGrid.FromRows(new[]
            {
                new[] { "id", "Power Rating" },
                new[] { "axe", "3" },
            });

            AliasConfig target = new AliasConfig();
            VerticalNestParseResult result = VerticalNestParser.ParseInto(target, grid);

            Assert.That(result.Success, Is.True, () => result.Errors[0].Message);
            Assert.That(target.items[0].id, Is.EqualTo("axe"));
            Assert.That(target.items[0].power, Is.EqualTo(3));
        }


        [Test]
        public void ParseInto_CellValueContainsComma_DoesNotBreakParse()
        {
            // Sheets API / in-memory grid path: commas are cell content, not CSV delimiters
            SheetGrid grid = SheetGrid.FromRows(new[]
            {
                new[] { "Weapons", "Power Rating" },
                new[] { "sword, iron", "7" },
            });

            AliasConfig target = new AliasConfig();
            VerticalNestParseResult result = VerticalNestParser.ParseInto(target, grid);

            Assert.That(result.Success, Is.True, () => result.Errors[0].Message);
            Assert.That(target.items[0].id, Is.EqualTo("sword, iron"));
        }


        [Test]
        public void ParseInto_IgnoreMarkerOnAliasColumn_SkipsThatColumn()
        {
            SheetGrid grid = SheetGrid.FromRows(new[]
            {
                new[] { "", "", "!!!" },
                new[] { "Weapons", "power", "Designer Notes" },
                new[] { "sword", "7", "SECRET" },
            });

            NotesAliasConfig target = new NotesAliasConfig();
            VerticalNestParseResult result = VerticalNestParser.ParseInto(target, grid);

            Assert.That(result.Success, Is.True, () => result.Errors[0].Message);
            Assert.That(target.items[0].id, Is.EqualTo("sword"));
            Assert.That(target.items[0].power, Is.EqualTo(7));
            Assert.That(target.items[0].notes, Is.EqualTo(""));
        }

        #endregion


    }
}
