using Aerisyn.DataConfigSheet;
using NUnit.Framework;

namespace Aerisyn.DataConfigSheet.Tests
{
    /// <summary>
    /// Config Manifest seams: snapshot built from Pull All results, completeness against a required
    /// Config Type list, Drive metadata lookup (injected fake), content hashing, and the Production
    /// build gate rule. No Google and no Unity Editor.
    /// </summary>
    public sealed class ConfigManifestTests
    {


        #region Fixtures

        static readonly DateTime FixedUtc = new DateTime(2026, 11, 17, 12, 0, 0, DateTimeKind.Utc);


        static PullJobResult PassedJob(string name, string featureKey, string sheetId, params string[] configTypeNames)
        {
            ManifestAssetEntry[] assets = configTypeNames
                .Select(typeName => new ManifestAssetEntry(typeName, "Assets/Config/" + typeName + ".asset", "hash-" + typeName))
                .ToArray();
            ManifestFeatureEntry feature = new ManifestFeatureEntry(
                featureKey, sheetId, "[Production]_" + featureKey, "2026-11-16T09:30:00.000Z", "42");
            return PullJobResult.Passed(name, "ok", feature, assets);
        }

        #endregion


        #region Snapshot from jobs

        [Test]
        public void FromJobs_PassedJobs_RecordsEnvironmentTimestampFeaturesAndAssetHashes()
        {
            ConfigManifestSnapshot snapshot = ConfigManifestSnapshot.FromJobs(
                "Production",
                FixedUtc,
                new[]
                {
                    PassedJob("ShipPull", "ai_ship", "sheet-ship", "ShipConfig", "ShipTuning"),
                    PassedJob("CannonPull", "cannon", "sheet-cannon", "CannonConfig"),
                });

            Assert.That(snapshot.EnvironmentKey, Is.EqualTo("Production"));
            Assert.That(snapshot.GeneratedAtUtc, Is.EqualTo("2026-11-17T12:00:00.0000000Z"));
            Assert.That(snapshot.Features.Select(feature => feature.FeatureKey), Is.EqualTo(new[] { "ai_ship", "cannon" }));
            Assert.That(snapshot.Features[0].SpreadsheetId, Is.EqualTo("sheet-ship"));
            Assert.That(snapshot.Features[0].SheetTitle, Is.EqualTo("[Production]_ai_ship"));
            Assert.That(snapshot.Features[0].DriveModifiedTime, Is.EqualTo("2026-11-16T09:30:00.000Z"));
            Assert.That(snapshot.Features[0].DriveVersion, Is.EqualTo("42"));
            Assert.That(snapshot.Assets.Select(asset => asset.ConfigTypeName),
                Is.EqualTo(new[] { "ShipConfig", "ShipTuning", "CannonConfig" }));
            Assert.That(snapshot.Assets[0].ContentHash, Is.EqualTo("hash-ShipConfig"));
            Assert.That(snapshot.Assets[0].AssetPath, Is.EqualTo("Assets/Config/ShipConfig.asset"));
        }


        [Test]
        public void FromJobs_FailedJob_ContributesNothing()
        {
            ConfigManifestSnapshot snapshot = ConfigManifestSnapshot.FromJobs(
                "Dev",
                FixedUtc,
                new[]
                {
                    PassedJob("ShipPull", "ai_ship", "sheet-ship", "ShipConfig"),
                    PullJobResult.Failed("CannonPull", "Title Check failed"),
                });

            Assert.That(snapshot.Features.Select(feature => feature.FeatureKey), Is.EqualTo(new[] { "ai_ship" }));
            Assert.That(snapshot.Assets.Select(asset => asset.ConfigTypeName), Is.EqualTo(new[] { "ShipConfig" }));
        }


        [Test]
        public void FromJobs_TwoPullConfigsShareFeature_ListsFeatureOnce()
        {
            ConfigManifestSnapshot snapshot = ConfigManifestSnapshot.FromJobs(
                "Dev",
                FixedUtc,
                new[]
                {
                    PassedJob("ShipPullA", "ai_ship", "sheet-ship", "ShipConfig"),
                    PassedJob("ShipPullB", "ai_ship", "sheet-ship", "ShipTuning"),
                });

            Assert.That(snapshot.Features, Has.Count.EqualTo(1));
            Assert.That(snapshot.Assets, Has.Count.EqualTo(2));
        }


        [Test]
        public void FromJobs_PassedJobWithoutMetadata_StillSucceedsWithEmptyAssets()
        {
            // A Pull that predates manifest data (or an inject Pull) passes but adds no rows.
            ConfigManifestSnapshot snapshot = ConfigManifestSnapshot.FromJobs(
                "Dev",
                FixedUtc,
                new[] { PullJobResult.Passed("ShipPull", "ok") });

            Assert.That(snapshot.Features, Is.Empty);
            Assert.That(snapshot.Assets, Is.Empty);
        }

        #endregion


        #region Completeness

        [Test]
        public void Check_EveryRequiredTypePresentInExpectedEnvironment_NoProblems()
        {
            ConfigManifestSnapshot snapshot = ConfigManifestSnapshot.FromJobs(
                "Production",
                FixedUtc,
                new[] { PassedJob("ShipPull", "ai_ship", "sheet-ship", "ShipConfig", "CannonConfig") });

            IReadOnlyList<string> problems = ManifestCompleteness.Check(
                snapshot, "Production", new[] { "ShipConfig", "CannonConfig" });

            Assert.That(problems, Is.Empty);
        }


        [Test]
        public void Check_RequiredTypeMissing_ProblemNamesTheType()
        {
            ConfigManifestSnapshot snapshot = ConfigManifestSnapshot.FromJobs(
                "Production",
                FixedUtc,
                new[] { PassedJob("ShipPull", "ai_ship", "sheet-ship", "ShipConfig") });

            IReadOnlyList<string> problems = ManifestCompleteness.Check(
                snapshot, "Production", new[] { "ShipConfig", "CannonConfig" });

            Assert.That(problems, Has.Count.EqualTo(1));
            Assert.That(problems[0], Does.Contain("CannonConfig"));
        }


        [Test]
        public void Check_EveryMissingTypeIsReported()
        {
            ConfigManifestSnapshot snapshot = ConfigManifestSnapshot.FromJobs("Production", FixedUtc, Array.Empty<PullJobResult>());

            IReadOnlyList<string> problems = ManifestCompleteness.Check(
                snapshot, "Production", new[] { "ShipConfig", "CannonConfig" });

            Assert.That(problems, Has.Count.EqualTo(2));
        }


        [Test]
        public void Check_WrongEnvironment_ProblemNamesBothEnvironments()
        {
            ConfigManifestSnapshot snapshot = ConfigManifestSnapshot.FromJobs(
                "Dev",
                FixedUtc,
                new[] { PassedJob("ShipPull", "ai_ship", "sheet-ship", "ShipConfig") });

            IReadOnlyList<string> problems = ManifestCompleteness.Check(
                snapshot, "Production", new[] { "ShipConfig" });

            Assert.That(problems, Has.Count.EqualTo(1));
            Assert.That(problems[0], Does.Contain("'Dev'"));
            Assert.That(problems[0], Does.Contain("'Production'"));
        }


        [Test]
        public void Check_EnvironmentMatchIsCaseSensitive()
        {
            ConfigManifestSnapshot snapshot = ConfigManifestSnapshot.FromJobs("production", FixedUtc, Array.Empty<PullJobResult>());

            IReadOnlyList<string> problems = ManifestCompleteness.Check(snapshot, "Production", Array.Empty<string>());

            Assert.That(problems, Has.Count.EqualTo(1));
        }


        [Test]
        public void Check_NullManifest_ProblemSaysManifestIsMissing()
        {
            IReadOnlyList<string> problems = ManifestCompleteness.Check(null, "Production", new[] { "ShipConfig" });

            Assert.That(problems, Has.Count.EqualTo(1));
            Assert.That(problems[0], Does.Contain("Config Manifest"));
        }


        [Test]
        public void Check_BlankExpectedEnvironment_IsAProblem()
        {
            ConfigManifestSnapshot snapshot = ConfigManifestSnapshot.FromJobs("Production", FixedUtc, Array.Empty<PullJobResult>());

            Assert.That(ManifestCompleteness.Check(snapshot, "", Array.Empty<string>()), Has.Count.EqualTo(1));
        }

        #endregion


        #region Completeness of a Pull All run

        [Test]
        public void CheckRun_EveryRequiredTypeBakedByPassedJobs_NoProblems()
        {
            IReadOnlyList<string> problems = ManifestCompleteness.CheckRun(
                "Production",
                new[] { "ShipConfig", "CannonConfig" },
                new[]
                {
                    PassedJob("ShipPull", "ai_ship", "sheet-ship", "ShipConfig"),
                    PassedJob("CannonPull", "cannon", "sheet-cannon", "CannonConfig"),
                });

            Assert.That(problems, Is.Empty);
        }


        [Test]
        public void CheckRun_FailedJobOwnedTheRequiredType_ReportsItMissing()
        {
            // Mixed run: CannonPull failed, so CannonConfig has no successful output.
            IReadOnlyList<string> problems = ManifestCompleteness.CheckRun(
                "Production",
                new[] { "ShipConfig", "CannonConfig" },
                new[]
                {
                    PassedJob("ShipPull", "ai_ship", "sheet-ship", "ShipConfig"),
                    PullJobResult.Failed("CannonPull", "Title Check failed"),
                });

            Assert.That(problems, Has.Count.EqualTo(1));
            Assert.That(problems[0], Does.Contain("CannonConfig"));
        }


        [Test]
        public void CheckRun_AllJobsFailed_EveryRequiredTypeReportedMissing()
        {
            IReadOnlyList<string> problems = ManifestCompleteness.CheckRun(
                "Production",
                new[] { "ShipConfig", "CannonConfig" },
                new[] { PullJobResult.Failed("ShipPull", "boom"), PullJobResult.Failed("CannonPull", "boom") });

            Assert.That(problems, Has.Count.EqualTo(2));
        }


        [Test]
        public void CheckRun_NoRequiredList_NoProblems()
        {
            IReadOnlyList<string> problems = ManifestCompleteness.CheckRun(
                "Production", null, new[] { PullJobResult.Failed("ShipPull", "boom") });

            Assert.That(problems, Is.Empty);
        }


        [Test]
        public void Summary_FailedJobAndMissingRequiredType_FormatShowsBoth()
        {
            PullJobResult[] results =
            {
                PassedJob("ShipPull", "ai_ship", "sheet-ship", "ShipConfig"),
                PullJobResult.Failed("CannonPull", "Title Check failed"),
            };
            IReadOnlyList<string> problems = ManifestCompleteness.CheckRun(
                "Production", new[] { "ShipConfig", "CannonConfig" }, results);

            PullAllSummary summary = PullAllSummary.Create(results, problems);
            string text = summary.Format();

            Assert.That(summary.OverallSuccess, Is.False);
            Assert.That(text, Does.Contain("Title Check failed"));
            Assert.That(text, Does.Contain("missing required Config Type 'CannonConfig'"));
        }

        #endregion


        #region Drive metadata seam

        [Test]
        public async Task TryFetchAsync_FetcherReturnsMetadata_PassesItThrough()
        {
            string? askedFor = null;

            SheetRevisionMetadata metadata = await SheetRevisionLookup.TryFetchAsync(
                id =>
                {
                    askedFor = id;
                    return Task.FromResult(new SheetRevisionMetadata("2026-11-16T09:30:00.000Z", "42"));
                },
                "sheet-ship");

            Assert.That(askedFor, Is.EqualTo("sheet-ship"));
            Assert.That(metadata.ModifiedTime, Is.EqualTo("2026-11-16T09:30:00.000Z"));
            Assert.That(metadata.Version, Is.EqualTo("42"));
        }


        [Test]
        public async Task TryFetchAsync_FetcherThrows_ReturnsEmptyMetadataInsteadOfFailingPull()
        {
            // Drive revision fields are "when available": a Drive hiccup must not fail an otherwise good Pull.
            SheetRevisionMetadata metadata = await SheetRevisionLookup.TryFetchAsync(
                id => throw new InvalidOperationException("Drive 403"),
                "sheet-ship");

            Assert.That(metadata.ModifiedTime, Is.EqualTo(""));
            Assert.That(metadata.Version, Is.EqualTo(""));
        }


        [Test]
        public async Task TryFetchAsync_NullFetcher_ReturnsEmptyMetadata()
        {
            SheetRevisionMetadata metadata = await SheetRevisionLookup.TryFetchAsync(null, "sheet-ship");

            Assert.That(metadata.ModifiedTime, Is.EqualTo(""));
        }

        #endregion


        #region Content hash

        [Test]
        public void Sha256Hex_KnownVector_MatchesPublishedDigest()
        {
            // FIPS 180-2 test vector for "abc".
            byte[] bytes = System.Text.Encoding.ASCII.GetBytes("abc");

            Assert.That(
                ManifestContentHash.Sha256Hex(bytes),
                Is.EqualTo("ba7816bf8f01cfea414140de5dae2223b00361a396177a9cb410ff61f20015ad"));
        }


        [Test]
        public void Sha256Hex_DifferentBytes_DifferentHash()
        {
            Assert.That(
                ManifestContentHash.Sha256Hex(new byte[] { 1 }),
                Is.Not.EqualTo(ManifestContentHash.Sha256Hex(new byte[] { 2 })));
        }

        #endregion


        #region Production build gate

        [Test]
        public void CheckProductionBuild_ProductionActive_NoProblem()
        {
            Assert.That(ProductionBuildGate.Check("Production"), Is.Null);
        }


        [TestCase("Dev")]
        [TestCase("Test")]
        [TestCase("production")]
        [TestCase("")]
        [TestCase(null)]
        public void CheckProductionBuild_NotProduction_ProblemNamesActiveEnvironment(string? activeEnvironment)
        {
            string? problem = ProductionBuildGate.Check(activeEnvironment);

            Assert.That(problem, Is.Not.Null);
            Assert.That(problem, Does.Contain("Production"));
        }

        #endregion


    }
}
