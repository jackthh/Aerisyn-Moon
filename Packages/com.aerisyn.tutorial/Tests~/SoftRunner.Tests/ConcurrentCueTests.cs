using System.Collections.Generic;
using Aerisyn.Tutorial;
using NUnit.Framework;

namespace Aerisyn.Tutorial.Tests
{
    /// <summary>
    /// Concurrent and mixed Cue Choreography at the Runner seam (Forget + await mix).
    /// </summary>
    public sealed class ConcurrentCueTests
    {
        #region Fixtures

        const int UpgradeSwordKind = 10;

        static TutorialDefinition OneStepConcurrentCues() =>
            new TutorialBuilder("onboarding.concurrent")
                .SoftStep(
                    "upgrade",
                    new ReportMatch(UpgradeSwordKind, 1),
                    ChoreographyDefinition.Concurrent("glow.sword", "sfx.chime", "badge.new"))
                .Build();


        static TutorialDefinition OneStepMixedChoreography() =>
            new TutorialBuilder("onboarding.mixed")
                .SoftStep(
                    "upgrade",
                    new ReportMatch(UpgradeSwordKind, 1),
                    ChoreographyDefinition.Mix(
                        ChoreographyDefinition.Concurrent("glow", "sfx"),
                        ChoreographyDefinition.Sequential("highlight", "text"),
                        ChoreographyDefinition.Concurrent("confetti")))
                .Build();


        static TutorialDefinition SequentialThenConcurrent() =>
            new TutorialBuilder("onboarding.seq-then-conc")
                .SoftStep(
                    "upgrade",
                    new ReportMatch(UpgradeSwordKind, 1),
                    ChoreographyDefinition.Mix(
                        ChoreographyDefinition.Sequential("cue.a", "cue.b"),
                        ChoreographyDefinition.Concurrent("cue.c", "cue.d")))
                .Build();

        #endregion


        #region Concurrent Forget

        [Test]
        public void EnteringStep_EmitsAllConcurrentCues_WithoutCueDone()
        {
            var runner = new TutorialRunner();
            var cues = new List<string>();
            runner.Cue += (tutorialId, stepIndex, stepId, cueId) => cues.Add(cueId);

            runner.Start(OneStepConcurrentCues());

            Assert.That(cues, Is.EqualTo(new[] { "glow.sword", "sfx.chime", "badge.new" }));
        }


        [Test]
        public void CueDone_OnConcurrentCues_IsIgnored()
        {
            var runner = new TutorialRunner();
            var cues = new List<string>();
            runner.Cue += (tutorialId, stepIndex, stepId, cueId) => cues.Add(cueId);

            runner.Start(OneStepConcurrentCues());
            runner.CueDone("glow.sword");
            runner.CueDone("sfx.chime");

            Assert.That(cues, Has.Count.EqualTo(3), "Cue Done must not re-emit or append for Concurrent");
        }

        #endregion


        #region Mixed Choreography

        [Test]
        public void EnteringMixedStep_EmitsConcurrentThenFirstSequential()
        {
            var runner = new TutorialRunner();
            var cues = new List<string>();
            runner.Cue += (tutorialId, stepIndex, stepId, cueId) => cues.Add(cueId);

            runner.Start(OneStepMixedChoreography());

            // Forget glow+sfx, then await first Sequential Cue; confetti waits for Sequential Done.
            Assert.That(cues, Is.EqualTo(new[] { "glow", "sfx", "highlight" }));
        }


        [Test]
        public void CueDone_ThroughMixedSequential_EmitsConfettiConcurrentGroup()
        {
            var runner = new TutorialRunner();
            var cues = new List<string>();
            runner.Cue += (tutorialId, stepIndex, stepId, cueId) => cues.Add(cueId);

            runner.Start(OneStepMixedChoreography());
            runner.CueDone("highlight");
            runner.CueDone("text");

            Assert.That(
                cues,
                Is.EqualTo(new[] { "glow", "sfx", "highlight", "text", "confetti" }));
            Assert.That(runner.IsActive, Is.True, "Choreography alone must not complete the Step");
        }


        [Test]
        public void SequentialThenConcurrent_EmitsConcurrentOnlyAfterSequentialDone()
        {
            var runner = new TutorialRunner();
            var cues = new List<string>();
            runner.Cue += (tutorialId, stepIndex, stepId, cueId) => cues.Add(cueId);

            runner.Start(SequentialThenConcurrent());
            Assert.That(cues, Is.EqualTo(new[] { "cue.a" }));

            runner.CueDone("cue.a");
            Assert.That(cues, Is.EqualTo(new[] { "cue.a", "cue.b" }));

            runner.CueDone("cue.b");
            Assert.That(cues, Is.EqualTo(new[] { "cue.a", "cue.b", "cue.c", "cue.d" }));
        }


        [Test]
        public void MatchingReport_CompletesMixedStep_AbandoningUnfinishedSequential()
        {
            var runner = new TutorialRunner();
            var cues = new List<string>();
            var stepCompletions = new List<string>();
            runner.Cue += (tutorialId, stepIndex, stepId, cueId) => cues.Add(cueId);
            runner.StepCompleted += (id, index, stepId) => stepCompletions.Add(stepId);

            runner.Start(OneStepMixedChoreography());
            Assert.That(cues, Is.EqualTo(new[] { "glow", "sfx", "highlight" }));

            runner.Report(UpgradeSwordKind, 1);

            Assert.That(stepCompletions, Is.EqualTo(new[] { "upgrade" }));
            Assert.That(cues, Has.Count.EqualTo(3), "must not emit text or confetti after Report");
            Assert.That(runner.IsActive, Is.False);
        }

        #endregion


        #region Report vs Choreography independence

        [Test]
        public void MatchingReport_CompletesStep_EvenIfConcurrentCuesWereEmitted()
        {
            var runner = new TutorialRunner();
            var cues = new List<string>();
            var stepCompletions = new List<string>();
            runner.Cue += (tutorialId, stepIndex, stepId, cueId) => cues.Add(cueId);
            runner.StepCompleted += (id, index, stepId) => stepCompletions.Add(stepId);

            runner.Start(OneStepConcurrentCues());
            Assert.That(cues, Has.Count.EqualTo(3));

            runner.Report(UpgradeSwordKind, 1);

            Assert.That(stepCompletions, Is.EqualTo(new[] { "upgrade" }));
            Assert.That(cues, Has.Count.EqualTo(3), "Report must not emit further Cues");
            Assert.That(runner.IsActive, Is.False);
        }

        #endregion
    }
}
