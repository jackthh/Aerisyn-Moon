using System.Collections.Generic;
using Aerisyn.Tutorial.Samples.RunnerSmoke;
using NUnit.Framework;

namespace Aerisyn.Tutorial.Tests
{
    /// <summary>
    /// Scripted Samples~ smoke path at the Runner seam (Soft/Hard + Cue → Completion).
    /// </summary>
    public sealed class RunnerSmokeTests
    {
        [Test]
        public void RunScripted_EmitsCueGateAndCompletions_InOrder()
        {
            IReadOnlyList<string> log = RunnerSmokeDriver.RunScripted();

            Assert.That(log, Does.Contain("Cue coach:highlight.menu"));
            Assert.That(log, Does.Contain("CueDone coach:highlight.menu"));
            Assert.That(log, Does.Contain("StepCompleted coach"));
            Assert.That(log, Does.Contain("Gate Started upgrade"));
            Assert.That(log, Does.Contain("Cue upgrade:glow.slot"));
            Assert.That(log, Does.Contain("Cue upgrade:sfx.chime"));
            Assert.That(log, Does.Contain("Cue upgrade:highlight.button"));
            Assert.That(log, Does.Contain("StepCompleted upgrade"));
            Assert.That(log, Does.Contain("Gate Ended upgrade"));
            Assert.That(log, Does.Contain("TutorialCompleted sample.sword"));

            int cueMenu = IndexOf(log, "Cue coach:highlight.menu");
            int stepCoach = IndexOf(log, "StepCompleted coach");
            int gateStart = IndexOf(log, "Gate Started upgrade");
            int tutorialDone = IndexOf(log, "TutorialCompleted sample.sword");

            Assert.That(cueMenu, Is.LessThan(stepCoach));
            Assert.That(stepCoach, Is.LessThan(gateStart));
            Assert.That(gateStart, Is.LessThan(tutorialDone));
        }


        static int IndexOf(IReadOnlyList<string> log, string line)
        {
            for (var i = 0; i < log.Count; i++)
            {
                if (log[i] == line)
                    return i;
            }

            return -1;
        }
    }
}
