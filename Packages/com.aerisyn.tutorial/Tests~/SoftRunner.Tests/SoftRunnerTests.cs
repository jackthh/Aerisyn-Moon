using System;
using System.Collections.Generic;
using Aerisyn.Tutorial;
using NUnit.Framework;

namespace Aerisyn.Tutorial.Tests
{
    /// <summary>
    /// Soft Runner seam: Start / Report / Stop / Progress Snapshot through the public Runner only.
    /// </summary>
    public sealed class SoftRunnerTests
    {
        #region Fixtures

        const int UpgradeSwordKind = 10;
        const int EquipSwordKind = 11;
        const int NoiseKind = 99;

        static TutorialDefinition TwoSoftSteps() =>
            new TutorialBuilder("onboarding.sword")
                .SoftStep("upgrade", new ReportMatch(UpgradeSwordKind, 1))
                .SoftStep("equip", ReportMatch.AnyParam(EquipSwordKind))
                .Build();

        #endregion


        #region Start and Report advance

        [Test]
        public void MatchingReport_CompletesSoftStep_EmitsStepCompletion_AndAdvances()
        {
            var runner = new TutorialRunner();
            var stepCompletions = new List<(TutorialId TutorialId, int StepIndex, string StepId)>();
            var tutorialCompletions = new List<TutorialId>();
            runner.StepCompleted += (id, index, stepId) => stepCompletions.Add((id, index, stepId));
            runner.TutorialCompleted += id => tutorialCompletions.Add(id);

            runner.Start(TwoSoftSteps());

            Assert.That(runner.IsActive, Is.True);
            Assert.That(runner.ActiveStepIndex, Is.EqualTo(0));

            runner.Report(UpgradeSwordKind, 1);

            Assert.That(stepCompletions, Has.Count.EqualTo(1));
            Assert.That(stepCompletions[0].TutorialId.Value, Is.EqualTo("onboarding.sword"));
            Assert.That(stepCompletions[0].StepIndex, Is.EqualTo(0));
            Assert.That(stepCompletions[0].StepId, Is.EqualTo("upgrade"));
            Assert.That(runner.IsActive, Is.True);
            Assert.That(runner.ActiveStepIndex, Is.EqualTo(1));
            Assert.That(tutorialCompletions, Is.Empty);
        }


        [Test]
        public void MatchingReportOnLastSoftStep_EmitsStepAndTutorialCompletion_AndClearsActive()
        {
            var runner = new TutorialRunner();
            var stepCompletions = new List<(TutorialId TutorialId, int StepIndex, string StepId)>();
            var tutorialCompletions = new List<TutorialId>();
            runner.StepCompleted += (id, index, stepId) => stepCompletions.Add((id, index, stepId));
            runner.TutorialCompleted += id => tutorialCompletions.Add(id);

            runner.Start(TwoSoftSteps());
            runner.Report(UpgradeSwordKind, 1);
            runner.Report(EquipSwordKind, 7);

            Assert.That(stepCompletions, Has.Count.EqualTo(2));
            Assert.That(stepCompletions[1].StepIndex, Is.EqualTo(1));
            Assert.That(stepCompletions[1].StepId, Is.EqualTo("equip"));
            Assert.That(tutorialCompletions, Has.Count.EqualTo(1));
            Assert.That(tutorialCompletions[0].Value, Is.EqualTo("onboarding.sword"));
            Assert.That(runner.IsActive, Is.False);
        }

        #endregion


        #region Unmatched Reports

        [Test]
        public void UnmatchedReport_DoesNotAdvanceOrCorruptActiveStep()
        {
            var runner = new TutorialRunner();
            var stepCompletions = new List<(TutorialId TutorialId, int StepIndex, string StepId)>();
            runner.StepCompleted += (id, index, stepId) => stepCompletions.Add((id, index, stepId));

            runner.Start(TwoSoftSteps());
            runner.Report(NoiseKind, 0);
            runner.Report(UpgradeSwordKind, 2);

            Assert.That(stepCompletions, Is.Empty);
            Assert.That(runner.ActiveStepIndex, Is.EqualTo(0));
            Assert.That(runner.IsActive, Is.True);
        }

        #endregion


        #region Single active

        [Test]
        public void SecondStartWhileActive_IsRejected()
        {
            var runner = new TutorialRunner();
            runner.Start(TwoSoftSteps());

            Assert.Throws<InvalidOperationException>(() => runner.Start(TwoSoftSteps()));
            Assert.That(runner.IsActive, Is.True);
            Assert.That(runner.ActiveStepIndex, Is.EqualTo(0));
        }

        #endregion


        #region Stop

        [Test]
        public void Stop_AbandonsWithoutTutorialCompletion()
        {
            var runner = new TutorialRunner();
            var tutorialCompletions = new List<TutorialId>();
            runner.TutorialCompleted += id => tutorialCompletions.Add(id);

            runner.Start(TwoSoftSteps());
            runner.Stop();

            Assert.That(runner.IsActive, Is.False);
            Assert.That(tutorialCompletions, Is.Empty);
        }

        #endregion


        #region Progress Snapshot

        [Test]
        public void ExportAndApplyProgressSnapshot_ResumesAtStepIndex()
        {
            var runner = new TutorialRunner();
            runner.Start(TwoSoftSteps());
            runner.Report(UpgradeSwordKind, 1);

            ProgressSnapshot snapshot = runner.ExportSnapshot();
            Assert.That(snapshot.TutorialId, Is.EqualTo("onboarding.sword"));
            Assert.That(snapshot.StepIndex, Is.EqualTo(1));

            runner.Stop();
            Assert.That(runner.IsActive, Is.False);

            var stepCompletions = new List<(TutorialId TutorialId, int StepIndex, string StepId)>();
            runner.StepCompleted += (id, index, stepId) => stepCompletions.Add((id, index, stepId));

            runner.Start(TwoSoftSteps(), snapshot);

            Assert.That(runner.IsActive, Is.True);
            Assert.That(runner.ActiveStepIndex, Is.EqualTo(1));

            // Resumed mid-Tutorial: only the remaining Soft Step should fire Completion.
            runner.Report(EquipSwordKind, 0);
            Assert.That(stepCompletions, Has.Count.EqualTo(1));
            Assert.That(stepCompletions[0].StepIndex, Is.EqualTo(1));
            Assert.That(stepCompletions[0].StepId, Is.EqualTo("equip"));
            Assert.That(runner.IsActive, Is.False);
        }

        #endregion
    }
}
