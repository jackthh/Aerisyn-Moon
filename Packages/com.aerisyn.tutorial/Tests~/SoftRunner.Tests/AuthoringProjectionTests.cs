using System.Collections.Generic;
using Aerisyn.Tutorial;
using Aerisyn.Tutorial.Authoring;
using Aerisyn.Tutorial.Samples.RunnerSmoke;
using NUnit.Framework;

namespace Aerisyn.Tutorial.Tests
{
    /// <summary>
    /// Authoring projection seam: serializable Step / Cue data → same TutorialDefinition as TutorialBuilder.
    /// Runner accepts projected definitions; no second runtime model.
    /// </summary>
    public sealed class AuthoringProjectionTests
    {
        #region Projection shape

        [Test]
        public void Project_MapsEnforcementReportAndChoreography_1to1WithBuilder()
        {
            TutorialDefinition fromAuthoring = RunnerSmokeDriver.BuildDemoTutorialFromAuthoring();
            TutorialDefinition fromBuilder = RunnerSmokeDriver.BuildDemoTutorial();

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
                    ReportKind = RunnerSmokeDriver.ReportOpenMenu,
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
            IReadOnlyList<string> authoredLog =
                RunnerSmokeDriver.RunScripted(RunnerSmokeDriver.BuildDemoTutorialFromAuthoring());
            IReadOnlyList<string> builderLog = RunnerSmokeDriver.RunScripted();

            Assert.That(authoredLog, Is.EqualTo(builderLog));
        }

        #endregion


        #region Validation

        [Test]
        public void Project_NullOrEmptyTutorialId_Throws()
        {
            Assert.Throws<System.ArgumentException>(() =>
                TutorialAuthoringProjection.Project("", RunnerSmokeDriver.BuildDemoAuthoredSteps()));
        }


        [Test]
        public void Project_NullStepSlot_Throws()
        {
            var steps = new AuthoredStep[] { null };
            Assert.Throws<System.ArgumentException>(() =>
                TutorialAuthoringProjection.Project("bad", steps));
        }


        [Test]
        public void Project_BlankCueId_ThrowsLikeCore()
        {
            var steps = new[]
            {
                new AuthoredStep
                {
                    Id = "broken",
                    Enforcement = Enforcement.Soft,
                    ReportKind = 1,
                    MatchAnyParam = true,
                    CueGroups = new[]
                    {
                        new AuthoredCueGroup
                        {
                            Kind = CueGroupKind.Sequential,
                            CueIds = new[] { "ok", "" },
                        },
                    },
                },
            };

            Assert.Throws<System.ArgumentException>(() =>
                TutorialAuthoringProjection.Project("bad.cues", steps));
        }

        #endregion
    }
}
