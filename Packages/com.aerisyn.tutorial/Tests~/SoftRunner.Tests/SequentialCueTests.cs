using System.Collections.Generic;
using Aerisyn.Tutorial;
using NUnit.Framework;

namespace Aerisyn.Tutorial.Tests
{
    /// <summary>
    /// Sequential Cue Choreography at the Runner seam: Cue / Cue Done await, Report independence.
    /// </summary>
    public sealed class SequentialCueTests
    {
        #region Fixtures

        const int UpgradeSwordKind = 10;
        const int EquipSwordKind = 11;

        static TutorialDefinition OneStepTwoSequentialCues() =>
            new TutorialBuilder("onboarding.cues")
                .SoftStep(
                    "upgrade",
                    new ReportMatch(UpgradeSwordKind, 1),
                    ChoreographyDefinition.Sequential("highlight.sword", "text.upgrade"))
                .Build();

        static TutorialDefinition TwoStepsWithCues() =>
            new TutorialBuilder("onboarding.multi-cue")
                .SoftStep(
                    "upgrade",
                    new ReportMatch(UpgradeSwordKind, 1),
                    ChoreographyDefinition.Sequential("cue.a", "cue.b"))
                .SoftStep(
                    "equip",
                    ReportMatch.AnyParam(EquipSwordKind),
                    ChoreographyDefinition.Sequential("cue.c"))
                .Build();

        #endregion


        #region Await semantics

        [Test]
        public void EnteringStep_EmitsFirstSequentialCue_Only()
        {
            var runner = new TutorialRunner();
            var cues = new List<string>();
            runner.Cue += (tutorialId, stepIndex, stepId, cueId) => cues.Add(cueId);

            runner.Start(OneStepTwoSequentialCues());

            Assert.That(cues, Has.Count.EqualTo(1));
            Assert.That(cues[0], Is.EqualTo("highlight.sword"));
        }


        [Test]
        public void CueDone_EmitsNextSequentialCue()
        {
            var runner = new TutorialRunner();
            var cues = new List<(string StepId, string CueId)>();
            runner.Cue += (tutorialId, stepIndex, stepId, cueId) => cues.Add((stepId, cueId));

            runner.Start(OneStepTwoSequentialCues());
            runner.CueDone("highlight.sword");

            Assert.That(cues, Has.Count.EqualTo(2));
            Assert.That(cues[1].StepId, Is.EqualTo("upgrade"));
            Assert.That(cues[1].CueId, Is.EqualTo("text.upgrade"));
        }


        [Test]
        public void CueDone_OnLastCue_DoesNotEmitFurtherCues_OrCompleteStep()
        {
            var runner = new TutorialRunner();
            var cues = new List<string>();
            var stepCompletions = new List<string>();
            runner.Cue += (tutorialId, stepIndex, stepId, cueId) => cues.Add(cueId);
            runner.StepCompleted += (id, index, stepId) => stepCompletions.Add(stepId);

            runner.Start(OneStepTwoSequentialCues());
            runner.CueDone("highlight.sword");
            runner.CueDone("text.upgrade");

            Assert.That(cues, Has.Count.EqualTo(2));
            Assert.That(stepCompletions, Is.Empty);
            Assert.That(runner.IsActive, Is.True);
            Assert.That(runner.ActiveStepIndex, Is.EqualTo(0));
        }


        [Test]
        public void MismatchedCueDone_IsIgnored()
        {
            var runner = new TutorialRunner();
            var cues = new List<string>();
            runner.Cue += (tutorialId, stepIndex, stepId, cueId) => cues.Add(cueId);

            runner.Start(OneStepTwoSequentialCues());
            runner.CueDone("text.upgrade");
            runner.CueDone("noise");

            Assert.That(cues, Has.Count.EqualTo(1));
            Assert.That(cues[0], Is.EqualTo("highlight.sword"));
        }

        #endregion


        #region Report vs Choreography independence

        [Test]
        public void MatchingReport_CompletesStep_EvenIfSequentialCuesUnfinished()
        {
            var runner = new TutorialRunner();
            var cues = new List<string>();
            var stepCompletions = new List<string>();
            runner.Cue += (tutorialId, stepIndex, stepId, cueId) => cues.Add(cueId);
            runner.StepCompleted += (id, index, stepId) => stepCompletions.Add(stepId);

            runner.Start(OneStepTwoSequentialCues());
            Assert.That(cues, Has.Count.EqualTo(1));

            // Report before finishing sequential Choreography.
            runner.Report(UpgradeSwordKind, 1);

            Assert.That(stepCompletions, Has.Count.EqualTo(1));
            Assert.That(stepCompletions[0], Is.EqualTo("upgrade"));
            Assert.That(cues, Has.Count.EqualTo(1), "unfinished second Cue must not emit after Report");
            Assert.That(runner.IsActive, Is.False);
        }


        [Test]
        public void AdvancingToNextStep_StartsItsSequentialChoreography()
        {
            var runner = new TutorialRunner();
            var cues = new List<(string StepId, string CueId)>();
            runner.Cue += (tutorialId, stepIndex, stepId, cueId) => cues.Add((stepId, cueId));

            runner.Start(TwoStepsWithCues());
            runner.Report(UpgradeSwordKind, 1);

            Assert.That(cues, Has.Count.EqualTo(2));
            Assert.That(cues[0].CueId, Is.EqualTo("cue.a"));
            Assert.That(cues[1].StepId, Is.EqualTo("equip"));
            Assert.That(cues[1].CueId, Is.EqualTo("cue.c"));
        }

        #endregion
    }
}
