using Aerisyn.DataConfigSheet;
using NUnit.Framework;

namespace Aerisyn.DataConfigSheet.Tests
{
    /// <summary>
    /// Pure Pull report seam: success summary + failure lines with A1 sheet coordinates.
    /// </summary>
    public sealed class PullReportTests
    {


        #region Failure formatting

        [Test]
        public void Format_FailedPull_IncludesA1CellCoordinatesAndMessages()
        {
            // 0-based parse indices → designer-facing A1 (row 1 col 1 = B2)
            VerticalNestParseError[] errors =
            {
                new VerticalNestParseError(1, 1, "[Weapons] Cannot parse 'power' as int."),
                new VerticalNestParseError(4, -1, "[Weapons] Nested parent missing."),
                new VerticalNestParseError(-1, -1, "[Weapons] No injected grid."),
            };

            PullReport report = PullReport.Failed("DemoPull", errors);

            string text = report.Format();

            Assert.That(report.Success, Is.False);
            Assert.That(text, Does.Contain("Pull failed for 'DemoPull'"));
            Assert.That(text, Does.Contain("B2"));
            Assert.That(text, Does.Contain("row 5"));
            Assert.That(text, Does.Contain("Cannot parse 'power' as int."));
            Assert.That(text, Does.Contain("No injected grid."));
        }


        [Test]
        public void ToA1_MapsZeroBasedIndicesToSheetCells()
        {
            Assert.That(PullReport.ToA1(0, 0), Is.EqualTo("A1"));
            Assert.That(PullReport.ToA1(2, 25), Is.EqualTo("Z3"));
            Assert.That(PullReport.ToA1(0, 26), Is.EqualTo("AA1"));
            Assert.That(PullReport.ToA1(-1, 0), Is.Null);
            Assert.That(PullReport.ToA1(0, -1), Is.Null);
        }


        [Test]
        public void FormatCoordinate_FallsBackToRowOrColumnWhenPartial()
        {
            Assert.That(PullReport.FormatCoordinate(4, -1), Is.EqualTo("row 5"));
            Assert.That(PullReport.FormatCoordinate(-1, 2), Is.EqualTo("column C"));
            Assert.That(PullReport.FormatCoordinate(-1, -1), Is.Null);
        }

        #endregion


        #region Success formatting

        [Test]
        public void Format_SuccessfulPull_SummarizesCreatedAndUpdatedAssets()
        {
            PullWriteAction[] writes =
            {
                new PullWriteAction("WeaponsConfig", "Assets/Baked/WeaponsConfig.asset", PullWriteKind.Created),
                new PullWriteAction("LevelsConfig", "Assets/Baked/LevelsConfig.asset", PullWriteKind.Updated),
            };

            PullReport report = PullReport.Succeeded("DemoPull", writes);

            string text = report.Format();

            Assert.That(report.Success, Is.True);
            Assert.That(text, Does.Contain("Pull complete for 'DemoPull'"));
            Assert.That(text, Does.Contain("2 Baked Asset"));
            Assert.That(text, Does.Contain("WeaponsConfig: created"));
            Assert.That(text, Does.Contain("WeaponsConfig.asset"));
            Assert.That(text, Does.Contain("LevelsConfig: updated"));
            Assert.That(text, Does.Contain("LevelsConfig.asset"));
        }


        [Test]
        public void Format_SuccessfulPull_WithLocalOnlyWarnings_ListsThemAfterWrites()
        {
            // Accidental Local Only column on the Source Sheet: Pull succeeds but must warn
            PullWriteAction[] writes =
            {
                new PullWriteAction("MovementConfig", "Assets/Baked/MovementConfig.asset", PullWriteKind.Updated),
            };
            string[] warnings =
            {
                "[Movement] Header 'accelerateTurnSpeed' matches Local Only field 'accelerateTurnSpeed'; ignored.",
            };

            PullReport report = PullReport.Succeeded("DemoPull", writes, warnings);

            string text = report.Format();

            Assert.That(report.Success, Is.True);
            Assert.That(report.Warnings, Has.Count.EqualTo(1));
            Assert.That(text, Does.Contain("Pull complete for 'DemoPull'"));
            Assert.That(text, Does.Contain("MovementConfig: updated"));
            Assert.That(text, Does.Contain("Warning"));
            Assert.That(text, Does.Contain("Local Only"));
            Assert.That(text, Does.Contain("accelerateTurnSpeed"));
        }


        [Test]
        public void ParseInto_LocalOnlyColumn_HandoffToSucceededReport_SurfacesWarning()
        {
            // PullFromGrids handoff without AssetDatabase: parse warnings → tab prefix → PullReport
            SheetGrid grid = SheetGrid.FromRows(new[]
            {
                new[] { "id", "turnSpeed", "accelerateTurnSpeed" },
                new[] { "dash", "12", "99" },
            });

            var scratch = new LocalOnlyParserTests.LocalOnlyConfig();
            VerticalNestParseResult parseResult = VerticalNestParser.ParseInto(scratch, grid);
            Assert.That(parseResult.Success, Is.True);
            Assert.That(parseResult.Warnings, Has.Count.EqualTo(1));

            string tabTitle = "LocalOnlyConfig";
            var pullWarnings = new System.Collections.Generic.List<string>();
            PullReport.AppendTabPrefixedWarnings(tabTitle, parseResult.Warnings, pullWarnings);

            PullWriteAction[] writes =
            {
                new PullWriteAction("LocalOnlyConfig", "Assets/Baked/LocalOnlyConfig.asset", PullWriteKind.Created),
            };

            PullReport report = PullReport.Succeeded("DemoPull", writes, pullWarnings);
            string text = report.Format();

            Assert.That(report.Success, Is.True);
            Assert.That(pullWarnings, Has.Count.EqualTo(1));
            Assert.That(text, Does.Contain("[" + tabTitle + "]"));
            Assert.That(text, Does.Contain("Local Only"));
            Assert.That(text, Does.Contain("accelerateTurnSpeed"));
            Assert.That(scratch.items[0].accelerateTurnSpeed, Is.EqualTo(0));
        }

        #endregion


    }
}
