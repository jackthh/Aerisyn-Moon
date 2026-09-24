using Aerisyn.DataConfigSheet;
using NUnit.Framework;

namespace Aerisyn.DataConfigSheet.Tests
{
    /// <summary>Focused parse-seam cases beyond the weapons fixture.</summary>
    public sealed class VerticalNestParserTests
    {


        #region Types

        public sealed class FlatConfig
        {
            public List<FlatRow> items = new List<FlatRow>();
        }


        public sealed class FlatRow
        {
            public string id = "";
            public int power;
        }


        public sealed class NotesConfig
        {
            public List<NotesRow> items = new List<NotesRow>();
        }


        public sealed class NotesRow
        {
            public string id = "";
            public int power;
            public string notes = "";
        }

        #endregion


        #region Tests

        [Test]
        public void ParseInto_SkipsPreambleUntilFieldHeadersMatch()
        {
            SheetGrid grid = SheetGrid.FromRows(new[]
            {
                new[] { "instructions", "more notes" },
                new[] { "still preamble", "" },
                new[] { "id", "power" },
                new[] { "a", "3" },
            });

            FlatConfig target = new FlatConfig();
            VerticalNestParseResult result = VerticalNestParser.ParseInto(target, grid);

            Assert.That(result.Success, Is.True);
            Assert.That(target.items, Has.Count.EqualTo(1));
            Assert.That(target.items[0].id, Is.EqualTo("a"));
            Assert.That(target.items[0].power, Is.EqualTo(3));
        }


        [Test]
        public void ParseInto_IgnoreMarkerOnFieldHeader_SkipsThatColumn()
        {
            // !!! on Field Header `notes`: header still matches; data must not populate notes
            SheetGrid grid = SheetGrid.FromRows(new[]
            {
                new[] { "", "", "!!!" },
                new[] { "id", "power", "notes" },
                new[] { "a", "3", "SECRET" },
            });

            NotesConfig target = new NotesConfig();
            VerticalNestParseResult result = VerticalNestParser.ParseInto(target, grid);

            Assert.That(result.Success, Is.True, () => result.Errors[0].Message);
            Assert.That(target.items, Has.Count.EqualTo(1));
            Assert.That(target.items[0].id, Is.EqualTo("a"));
            Assert.That(target.items[0].power, Is.EqualTo(3));
            Assert.That(target.items[0].notes, Is.EqualTo(""));
        }


        [Test]
        public void ParseInto_BadInt_ReturnsErrorWithCoordinates()
        {
            SheetGrid grid = SheetGrid.FromRows(new[]
            {
                new[] { "id", "power" },
                new[] { "a", "not-a-number" },
            });

            FlatConfig target = new FlatConfig();
            VerticalNestParseResult result = VerticalNestParser.ParseInto(target, grid);

            Assert.That(result.Success, Is.False);
            Assert.That(result.Errors, Has.Count.EqualTo(1));
            Assert.That(result.Errors[0].Row, Is.EqualTo(1));
            Assert.That(result.Errors[0].Column, Is.EqualTo(1));
            Assert.That(result.Errors[0].Message, Does.Contain("power"));
        }


        [Test]
        public void ParseInto_MissingHeader_FailsWithStructuredError()
        {
            SheetGrid grid = SheetGrid.FromRows(new[]
            {
                new[] { "id", "wrong" },
                new[] { "a", "1" },
            });

            FlatConfig target = new FlatConfig();
            VerticalNestParseResult result = VerticalNestParser.ParseInto(target, grid);

            Assert.That(result.Success, Is.False);
            Assert.That(result.Errors[0].Message, Does.Contain("Header Row"));
        }

        #endregion


    }
}
