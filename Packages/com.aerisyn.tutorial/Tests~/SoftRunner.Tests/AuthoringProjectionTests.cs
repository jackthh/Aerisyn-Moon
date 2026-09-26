using System.Collections.Generic;
using Aerisyn.Tutorial;
using Aerisyn.Tutorial.Authoring;
using NUnit.Framework;

namespace Aerisyn.Tutorial.Tests
{
    /// <summary>
    /// Authoring projection seam: serializable Step / Cue data → same TutorialDefinition as TutorialBuilder.
    /// Runner accepts projected definitions; no second runtime model.
    /// </summary>
    public sealed class AuthoringProjectionTests
    {
        #region Fixtures

        const int SoftReportKind = 11;
        const int HardReportKind = 10;
        const int HardReportParam = 1;

        /// <summary>Authored Soft + Hard mix matching the code-first demo shape.</summary>
        static AuthoredStep[] DemoAuthoredSteps() =>
            new[]
            {
                new AuthoredStep
                {
                    Id = "coach",
                    Enforcement = Enforcement.Soft,
                    ReportKind = SoftReportKind,
                    MatchAnyParam = true,
                    CueGroups = new[]
                    {
                        new AuthoredCueGroup
                        {
                            Kind = CueGroupKind.Sequential,
                            CueIds = new[] { "highlight.menu", "text.coach" },
                        },
                    },
                },
                new AuthoredStep
                {
                    Id = "upgrade",
                    Enforcement = Enforcement.Hard,
                    ReportKind = HardReportKind,
                    ReportParam = HardReportParam,
                    MatchAnyParam = false,
                    CueGroups = new[]
                    {
                        new AuthoredCueGroup
                        {
                            Kind = CueGroupKind.Concurrent,
                            CueIds = new[] { "glow.slot", "sfx.chime" },
                        },
                        new AuthoredCueGroup
                        {
                            Kind = CueGroupKind.Sequential,
                            CueIds = new[] { "highlight.button" },
                        },
                    },
                },
            };


        static TutorialDefinition BuilderDemo() =>
            new TutorialBuilder("sample.sword")
                .SoftStep(
                    "coach",
                    ReportMatch.AnyParam(SoftReportKind),
                    ChoreographyDefinition.Sequential("highlight.menu", "text.coach"))
                .HardStep(
                    "upgrade",
                    new ReportMatch(HardReportKind, HardReportParam),
                    ChoreographyDefinition.Mix(
                        ChoreographyDefinition.Concurrent("glow.slot", "sfx.chime"),
                        ChoreographyDefinition.Sequential("highlight.button")))
                .Build();

        #endregion


        #region Projection shape

        [Test]
        public void Project_MapsEnforcementReportAndChoreography_1to1WithBuilder()
        {
            TutorialDefinition fromAuthoring =
                TutorialAuthoringProjection.Project("sample.sword", DemoAuthoredSteps());
            TutorialDefinition fromBuilder = BuilderDemo();

            Assert.That(fromAuthoring.Id.Value, Is.EqualTo(fromBuilder.Id.Value));
            Assert.That(fromAuthoring.StepCount, Is.EqualTo(fromBuilder.StepCount));

            for (var i = 0; i < fromBuilder.StepCount; i++)
            {
                StepDefinition authored = fromAuthoring.Steps[i];
                StepDefinition built = fromBuilder.Steps[i];

                Assert.That(authored.Id, Is.EqualTo(built.Id));
                Assert.That(authored.Enforcement, Is.EqualTo(built.Enforcement));
                Assert.That(authored.ReportMatch, Is.EqualTo(built.ReportMatch));
                Assert.That(authored.Choreography.Groups.Count, Is.EqualTo(built.Choreography.Groups.Count));

                for (var g = 0; g < built.Choreography.Groups.Count; g++)
                {
                    CueGroup authoredGroup = authored.Choreography.Groups[g];
                    CueGroup builtGroup = built.Choreography.Groups[g];
                    Assert.That(authoredGroup.Kind, Is.EqualTo(builtGroup.Kind));
                    Assert.That(authoredGroup.CueIds, Is.EqualTo(builtGroup.CueIds));
                }
            }
        }


        [Test]
        public void Project_EmptyCueGroups_YieldsEmptyChoreography()
        {
            var steps = new[]
            {
                new AuthoredStep
                {
                    Id = "solo",
                    Enforcement = Enforcement.Soft,
                    ReportKind = SoftReportKind,
                    MatchAnyParam = true,
                    CueGroups = null,
                },
            };

            TutorialDefinition definition = TutorialAuthoringProjection.Project("solo.tut", steps);

            Assert.That(definition.Steps[0].Choreography.IsEmpty, Is.True);
        }

        #endregion


        #region Runner accepts projected definitions

        [Test]
        public void Runner_AcceptsProjectedDefinition_SameSignalsAsBuilder()
        {
            var fromAuthoring = TutorialAuthoringProjection.Project("sample.sword", DemoAuthoredSteps());
            var fromBuilder = BuilderDemo();

            IReadOnlyList<string> authoredLog = DriveDemo(fromAuthoring);
            IReadOnlyList<string> builderLog = DriveDemo(fromBuilder);

            Assert.That(authoredLog, Is.EqualTo(builderLog));
        }


        /// <summary>
        /// Soft Cue Done → Soft Report → Hard Report; returns Cue / Gate / Completion lines.
        /// </summary>
        static IReadOnlyList<string> DriveDemo(TutorialDefinition tutorial)
        {
            var log = new List<string>();
            var runner = new TutorialRunner();
            runner.Cue += (tutorialId, stepIndex, stepId, cueId) =>
                log.Add("Cue " + stepId + ":" + cueId);
            runner.Gate += (tutorialId, stepIndex, stepId, phase) =>
                log.Add("Gate " + phase + " " + stepId);
            runner.StepCompleted += (tutorialId, stepIndex, stepId) =>
                log.Add("StepCompleted " + stepId);
            runner.TutorialCompleted += tutorialId =>
                log.Add("TutorialCompleted " + tutorialId.Value);

            runner.Start(tutorial);
            runner.CueDone("highlight.menu");
            runner.Report(SoftReportKind, 0);
            runner.Report(HardReportKind, HardReportParam);

            return log;
        }

        #endregion


        #region Validation

        [Test]
        public void Project_NullOrEmptyTutorialId_Throws()
        {
            Assert.Throws<System.ArgumentException>(() =>
                TutorialAuthoringProjection.Project("", DemoAuthoredSteps()));
        }


        [Test]
        public void Project_NullStepSlot_Throws()
        {
            var steps = new AuthoredStep[] { null };
            Assert.Throws<System.ArgumentException>(() =>
                TutorialAuthoringProjection.Project("bad", steps));
        }

        #endregion
    }
}
