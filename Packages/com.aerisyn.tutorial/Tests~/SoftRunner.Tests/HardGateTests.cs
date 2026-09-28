using System.Collections.Generic;
using Aerisyn.Tutorial;
using NUnit.Framework;

namespace Aerisyn.Tutorial.Tests
{
    /// <summary>
    /// Hard Gate seam: Gate started/ended through the public Runner only.
    /// Soft Steps must not emit Gate lock semantics.
    /// </summary>
    public sealed class HardGateTests
    {
        #region Fixtures

        const int UpgradeSwordKind = 10;
        const int EquipSwordKind = 11;
        const int ClaimRewardKind = 12;

        static TutorialDefinition OneHardStep() =>
            new TutorialBuilder("onboarding.hard-gate")
                .HardStep("upgrade", new ReportMatch(UpgradeSwordKind, 1))
                .Build();

        static TutorialDefinition SoftThenHard() =>
            new TutorialBuilder("onboarding.mixed")
                .SoftStep("coach", ReportMatch.AnyParam(EquipSwordKind))
                .HardStep("upgrade", new ReportMatch(UpgradeSwordKind, 1))
                .Build();

        static TutorialDefinition HardThenSoft() =>
            new TutorialBuilder("onboarding.hard-then-soft")
                .HardStep("upgrade", new ReportMatch(UpgradeSwordKind, 1))
                .SoftStep("coach", ReportMatch.AnyParam(ClaimRewardKind))
                .Build();

        #endregion


        #region Enter Hard Step

        [Test]
        public void EnteringHardStep_EmitsGateStarted()
        {
            var runner = new TutorialRunner();
            var gates = new List<(TutorialId TutorialId, int StepIndex, string StepId, GatePhase Phase)>();
            runner.Gate += (id, index, stepId, phase) => gates.Add((id, index, stepId, phase));

            runner.Start(OneHardStep());

            Assert.That(gates, Has.Count.EqualTo(1));
            Assert.That(gates[0].TutorialId.Value, Is.EqualTo("onboarding.hard-gate"));
            Assert.That(gates[0].StepIndex, Is.EqualTo(0));
            Assert.That(gates[0].StepId, Is.EqualTo("upgrade"));
            Assert.That(gates[0].Phase, Is.EqualTo(GatePhase.Started));
        }

        #endregion


        #region Leave Hard Step

        [Test]
        public void CompletingHardStep_EmitsGateEnded_ThenTutorialCompletion()
        {
            var runner = new TutorialRunner();
            var gates = new List<(string StepId, GatePhase Phase)>();
            var tutorialCompletions = new List<TutorialId>();
            runner.Gate += (id, index, stepId, phase) => gates.Add((stepId, phase));
            runner.TutorialCompleted += id => tutorialCompletions.Add(id);

            runner.Start(OneHardStep());
            runner.Report(UpgradeSwordKind, 1);

            Assert.That(gates, Has.Count.EqualTo(2));
            Assert.That(gates[0].Phase, Is.EqualTo(GatePhase.Started));
            Assert.That(gates[1].StepId, Is.EqualTo("upgrade"));
            Assert.That(gates[1].Phase, Is.EqualTo(GatePhase.Ended));
            Assert.That(tutorialCompletions, Has.Count.EqualTo(1));
            Assert.That(runner.IsActive, Is.False);
        }


        [Test]
        public void StopOnHardStep_EmitsGateEnded_WithoutTutorialCompletion()
        {
            var runner = new TutorialRunner();
            var gates = new List<GatePhase>();
            var tutorialCompletions = new List<TutorialId>();
            runner.Gate += (id, index, stepId, phase) => gates.Add(phase);
            runner.TutorialCompleted += id => tutorialCompletions.Add(id);

            runner.Start(OneHardStep());
            runner.Stop();

            Assert.That(gates, Has.Count.EqualTo(2));
            Assert.That(gates[0], Is.EqualTo(GatePhase.Started));
            Assert.That(gates[1], Is.EqualTo(GatePhase.Ended));
            Assert.That(tutorialCompletions, Is.Empty);
            Assert.That(runner.IsActive, Is.False);
        }

        #endregion


        #region Soft Steps emit no Gate

        [Test]
        public void SoftSteps_DoNotEmitGateSignals()
        {
            var runner = new TutorialRunner();
            var gates = new List<(string StepId, GatePhase Phase)>();
            runner.Gate += (id, index, stepId, phase) => gates.Add((stepId, phase));

            runner.Start(
                new TutorialBuilder("onboarding.soft-only")
                    .SoftStep("coach", ReportMatch.AnyParam(EquipSwordKind))
                    .Build());
            runner.Report(EquipSwordKind, 0);
            // Soft-only Tutorial: Start + Report + complete, never a Gate.
            Assert.That(gates, Is.Empty);
        }

        #endregion


        #region Soft / Hard mix

        [Test]
        public void SoftThenHard_GatesOnlyTrackActiveHardStep()
        {
            var runner = new TutorialRunner();
            var gates = new List<(string StepId, GatePhase Phase)>();
            runner.Gate += (id, index, stepId, phase) => gates.Add((stepId, phase));

            runner.Start(SoftThenHard());
            Assert.That(gates, Is.Empty, "Soft enter must not emit Gate");

            runner.Report(EquipSwordKind, 0);
            Assert.That(gates, Has.Count.EqualTo(1));
            Assert.That(gates[0].StepId, Is.EqualTo("upgrade"));
            Assert.That(gates[0].Phase, Is.EqualTo(GatePhase.Started));

            runner.Report(UpgradeSwordKind, 1);
            Assert.That(gates, Has.Count.EqualTo(2));
            Assert.That(gates[1].StepId, Is.EqualTo("upgrade"));
            Assert.That(gates[1].Phase, Is.EqualTo(GatePhase.Ended));
        }


        [Test]
        public void HardThenSoft_EndingHardDoesNotGateSoft()
        {
            var runner = new TutorialRunner();
            var gates = new List<(string StepId, GatePhase Phase)>();
            runner.Gate += (id, index, stepId, phase) => gates.Add((stepId, phase));

            runner.Start(HardThenSoft());
            Assert.That(gates, Has.Count.EqualTo(1));
            Assert.That(gates[0].Phase, Is.EqualTo(GatePhase.Started));

            runner.Report(UpgradeSwordKind, 1);
            Assert.That(gates, Has.Count.EqualTo(2));
            Assert.That(gates[1].Phase, Is.EqualTo(GatePhase.Ended));
            Assert.That(runner.ActiveStepIndex, Is.EqualTo(1));

            runner.Report(ClaimRewardKind, 0);
            // Soft Completion must not add another Gate.
            Assert.That(gates, Has.Count.EqualTo(2));
            Assert.That(runner.IsActive, Is.False);
        }

        #endregion
    }
}
