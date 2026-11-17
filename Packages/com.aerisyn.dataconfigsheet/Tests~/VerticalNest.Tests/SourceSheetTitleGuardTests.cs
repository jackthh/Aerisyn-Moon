using Aerisyn.DataConfigSheet;
using NUnit.Framework;

namespace Aerisyn.DataConfigSheet.Tests
{
    /// <summary>
    /// Title Check inject seam (SourceSheetTitleGuard): the Source Sheet title is read and checked
    /// before any grid fetch, so a mismatch aborts the Pull before parse or write. Title and grid
    /// fetchers are injected fakes; no live Google.
    /// </summary>
    public sealed class SourceSheetTitleGuardTests
    {


        #region Fixtures

        sealed class FakeSheet
        {
            public string Title = "";
            public int TitleFetchCount;
            public int GridFetchCount;
            public string? TitleFetchedFor;
            public string? GridsFetchedFor;


            public Task<string> FetchTitleAsync(string spreadsheetId)
            {
                TitleFetchCount++;
                TitleFetchedFor = spreadsheetId;
                return Task.FromResult(Title);
            }


            public Task<string> FetchGridsAsync(string spreadsheetId)
            {
                GridFetchCount++;
                GridsFetchedFor = spreadsheetId;
                return Task.FromResult("grids");
            }
        }

        #endregion


        #region Match

        [Test]
        public async Task FetchAfterTitleCheckAsync_TitleHasActivePrefix_FetchesGridsForSameId()
        {
            FakeSheet sheet = new FakeSheet { Title = "[Dev]_AI_SHIP" };

            string result = await SourceSheetTitleGuard.FetchAfterTitleCheckAsync(
                "dev-ai-ship-id", "Dev", null, sheet.FetchTitleAsync, sheet.FetchGridsAsync);

            Assert.That(result, Is.EqualTo("grids"));
            Assert.That(sheet.TitleFetchedFor, Is.EqualTo("dev-ai-ship-id"));
            Assert.That(sheet.GridsFetchedFor, Is.EqualTo("dev-ai-ship-id"));
        }


        [Test]
        public async Task FetchAfterTitleCheckAsync_OverridePrefix_AcceptsOverrideTitle()
        {
            FakeSheet sheet = new FakeSheet { Title = "[QA]_AI_SHIP" };
            EnvironmentTitlePrefix[] overrides = { new EnvironmentTitlePrefix("Test", "[QA]") };

            await SourceSheetTitleGuard.FetchAfterTitleCheckAsync(
                "id", "Test", overrides, sheet.FetchTitleAsync, sheet.FetchGridsAsync);

            Assert.That(sheet.GridFetchCount, Is.EqualTo(1));
        }

        #endregion


        #region Mismatch

        [Test]
        public void FetchAfterTitleCheckAsync_TitleHasOtherEnvironmentPrefix_AbortsBeforeGridFetch()
        {
            // Production id pasted while Dev is active: must not reach grid fetch (so no parse, no write).
            FakeSheet sheet = new FakeSheet { Title = "[Production]_AI_SHIP" };

            Assert.ThrowsAsync<InvalidOperationException>(
                () => SourceSheetTitleGuard.FetchAfterTitleCheckAsync(
                    "prod-id", "Dev", null, sheet.FetchTitleAsync, sheet.FetchGridsAsync));

            Assert.That(sheet.TitleFetchCount, Is.EqualTo(1));
            Assert.That(sheet.GridFetchCount, Is.EqualTo(0));
        }


        [Test]
        public void FetchAfterTitleCheckAsync_Mismatch_MessageNamesExpectedPrefixAndActualTitle()
        {
            FakeSheet sheet = new FakeSheet { Title = "[Production]_AI_SHIP" };

            InvalidOperationException? error = Assert.ThrowsAsync<InvalidOperationException>(
                () => SourceSheetTitleGuard.FetchAfterTitleCheckAsync(
                    "prod-id", "Dev", null, sheet.FetchTitleAsync, sheet.FetchGridsAsync));

            Assert.That(error!.Message, Does.Contain("[Dev]"));
            Assert.That(error.Message, Does.Contain("[Production]_AI_SHIP"));
            Assert.That(error.Message, Does.Contain("Dev"));
        }


        [Test]
        public void FetchAfterTitleCheckAsync_PrefixCaseDiffers_Aborts()
        {
            FakeSheet sheet = new FakeSheet { Title = "[dev]_AI_SHIP" };

            Assert.ThrowsAsync<InvalidOperationException>(
                () => SourceSheetTitleGuard.FetchAfterTitleCheckAsync(
                    "id", "Dev", null, sheet.FetchTitleAsync, sheet.FetchGridsAsync));

            Assert.That(sheet.GridFetchCount, Is.EqualTo(0));
        }


        [Test]
        public void FetchAfterTitleCheckAsync_CustomEnvironmentWithoutPrefix_AbortsBeforeAnyFetch()
        {
            FakeSheet sheet = new FakeSheet { Title = "[Staging]_AI_SHIP" };

            Assert.ThrowsAsync<InvalidOperationException>(
                () => SourceSheetTitleGuard.FetchAfterTitleCheckAsync(
                    "id", "Staging", null, sheet.FetchTitleAsync, sheet.FetchGridsAsync));

            Assert.That(sheet.TitleFetchCount, Is.EqualTo(0));
            Assert.That(sheet.GridFetchCount, Is.EqualTo(0));
        }

        #endregion


        #region Skip Title Check (documented exception sheets)

        [Test]
        public async Task FetchAfterTitleCheckAsync_SkipTitleCheck_FetchesGridsWithoutReadingTitle()
        {
            // e.g. a "[Production]_CONVENTIONS" sheet pulled while Dev is active.
            FakeSheet sheet = new FakeSheet { Title = "[Production]_CONVENTIONS" };

            string result = await SourceSheetTitleGuard.FetchAfterTitleCheckAsync(
                "conventions-id", "Dev", null, sheet.FetchTitleAsync, sheet.FetchGridsAsync, skipTitleCheck: true);

            Assert.That(result, Is.EqualTo("grids"));
            Assert.That(sheet.TitleFetchCount, Is.EqualTo(0));
            Assert.That(sheet.GridsFetchedFor, Is.EqualTo("conventions-id"));
        }


        [Test]
        public async Task FetchAfterTitleCheckAsync_SkipTitleCheck_CustomEnvironmentWithoutPrefixStillPulls()
        {
            // The opt-out also covers the "custom key has no prefix" abort: nothing is checked.
            FakeSheet sheet = new FakeSheet { Title = "anything" };

            await SourceSheetTitleGuard.FetchAfterTitleCheckAsync(
                "id", "Staging", null, sheet.FetchTitleAsync, sheet.FetchGridsAsync, skipTitleCheck: true);

            Assert.That(sheet.GridFetchCount, Is.EqualTo(1));
        }


        [Test]
        public void FetchAfterTitleCheckAsync_SkipTitleCheckFalse_StillAbortsOnMismatch()
        {
            FakeSheet sheet = new FakeSheet { Title = "[Production]_AI_SHIP" };

            Assert.ThrowsAsync<InvalidOperationException>(
                () => SourceSheetTitleGuard.FetchAfterTitleCheckAsync(
                    "prod-id", "Dev", null, sheet.FetchTitleAsync, sheet.FetchGridsAsync, skipTitleCheck: false));

            Assert.That(sheet.GridFetchCount, Is.EqualTo(0));
        }

        #endregion


    }
}
