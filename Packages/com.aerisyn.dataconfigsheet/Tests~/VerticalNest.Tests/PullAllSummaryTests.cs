using Aerisyn.DataConfigSheet;
using NUnit.Framework;

namespace Aerisyn.DataConfigSheet.Tests
{
    /// <summary>
    /// Pull All orchestration and summary seam: every job runs even after a failure, and the
    /// per-job results aggregate into one report. Jobs are fakes; no Google and no Unity Editor.
    /// </summary>
    public sealed class PullAllSummaryTests
    {


        #region Run all jobs

        [Test]
        public async Task RunAllAsync_MiddleJobFails_StillRunsEveryJobInOrder()
        {
            List<string> ran = new List<string>();
            PullAllJob[] jobs =
            {
                new PullAllJob("ShipPull", () => { ran.Add("ShipPull"); return Task.FromResult(PullJobResult.Passed("ShipPull", "2 Baked Assets")); }),
                new PullAllJob("CannonPull", () => { ran.Add("CannonPull"); return Task.FromResult(PullJobResult.Failed("CannonPull", "Title Check failed")); }),
                new PullAllJob("LootPull", () => { ran.Add("LootPull"); return Task.FromResult(PullJobResult.Passed("LootPull", "1 Baked Asset")); }),
            };

            IReadOnlyList<PullJobResult> results = await PullAllRunner.RunAllAsync(jobs);

            Assert.That(ran, Is.EqualTo(new[] { "ShipPull", "CannonPull", "LootPull" }));
            Assert.That(results.Select(result => result.Name), Is.EqualTo(new[] { "ShipPull", "CannonPull", "LootPull" }));
            Assert.That(results.Select(result => result.Success), Is.EqualTo(new[] { true, false, true }));
        }


        [Test]
        public async Task RunAllAsync_JobThrows_RecordsFailureWithMessageAndContinues()
        {
            PullAllJob[] jobs =
            {
                new PullAllJob("ShipPull", () => throw new InvalidOperationException("PullConfig 'ShipPull' has no Feature Key.")),
                new PullAllJob("LootPull", () => Task.FromResult(PullJobResult.Passed("LootPull", "1 Baked Asset"))),
            };

            IReadOnlyList<PullJobResult> results = await PullAllRunner.RunAllAsync(jobs);

            Assert.That(results, Has.Count.EqualTo(2));
            Assert.That(results[0].Success, Is.False);
            Assert.That(results[0].Detail, Does.Contain("has no Feature Key"));
            Assert.That(results[1].Success, Is.True);
        }


        [Test]
        public async Task RunAllAsync_JobFaultsAsynchronously_RecordsFailureAndContinues()
        {
            PullAllJob[] jobs =
            {
                new PullAllJob("ShipPull", async () =>
                {
                    await Task.Yield();
                    throw new InvalidOperationException("sheet is gone");
                }),
                new PullAllJob("LootPull", () => Task.FromResult(PullJobResult.Passed("LootPull", "ok"))),
            };

            IReadOnlyList<PullJobResult> results = await PullAllRunner.RunAllAsync(jobs);

            Assert.That(results[0].Success, Is.False);
            Assert.That(results[0].Detail, Does.Contain("sheet is gone"));
            Assert.That(results[1].Success, Is.True);
        }

        #endregion


        #region Summary

        [Test]
        public void Create_AllJobsPassed_OverallSuccessWithCounts()
        {
            PullAllSummary summary = PullAllSummary.Create(
                new[]
                {
                    PullJobResult.Passed("ShipPull", "2 Baked Assets"),
                    PullJobResult.Passed("LootPull", "1 Baked Asset"),
                },
                null);

            Assert.That(summary.OverallSuccess, Is.True);
            Assert.That(summary.PassedCount, Is.EqualTo(2));
            Assert.That(summary.FailedCount, Is.EqualTo(0));
        }


        [Test]
        public void Create_OneJobFailed_OverallFailureAndTextListsPassedAndFailed()
        {
            PullAllSummary summary = PullAllSummary.Create(
                new[]
                {
                    PullJobResult.Passed("ShipPull", "2 Baked Assets"),
                    PullJobResult.Failed("CannonPull", "Title Check failed: expected [Dev]"),
                },
                null);

            string text = summary.Format();

            Assert.That(summary.OverallSuccess, Is.False);
            Assert.That(summary.PassedCount, Is.EqualTo(1));
            Assert.That(summary.FailedCount, Is.EqualTo(1));
            Assert.That(text, Does.Contain("1 passed"));
            Assert.That(text, Does.Contain("1 failed"));
            Assert.That(text, Does.Contain("ShipPull"));
            Assert.That(text, Does.Contain("CannonPull"));
            Assert.That(text, Does.Contain("Title Check failed: expected [Dev]"));
        }


        [Test]
        public void Create_ManifestFindingsPresent_OverallFailureEvenWhenEveryJobPassed()
        {
            PullAllSummary summary = PullAllSummary.Create(
                new[] { PullJobResult.Passed("ShipPull", "2 Baked Assets") },
                new[] { "Config Manifest is missing required Config Type 'CannonConfig'." });

            Assert.That(summary.OverallSuccess, Is.False);
            Assert.That(summary.Format(), Does.Contain("Config Manifest is missing required Config Type 'CannonConfig'."));
        }


        [Test]
        public void Create_NoJobs_IsNotSuccess()
        {
            // Nothing pulled must never read as a green Pull All.
            PullAllSummary summary = PullAllSummary.Create(Array.Empty<PullJobResult>(), null);

            Assert.That(summary.OverallSuccess, Is.False);
            Assert.That(summary.Format(), Does.Contain("No Pull Configs"));
        }


        [Test]
        public void Create_NoJobsWithFinding_FormatShowsTheFinding()
        {
            // Blank folder / empty folder reasons must reach the dialog, not just "No Pull Configs ran".
            PullAllSummary summary = PullAllSummary.Create(
                Array.Empty<PullJobResult>(),
                new[] { "Pull All folder is blank." });

            Assert.That(summary.OverallSuccess, Is.False);
            Assert.That(summary.Format(), Does.Contain("Pull All folder is blank."));
        }

        #endregion


    }
}
