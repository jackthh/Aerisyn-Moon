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


        [Test]
        public void RunScriptedFromAuthoring_MatchesBuilderSmokeLog()
        {
            IReadOnlyList<string> fromBuilder = RunnerSmokeDriver.RunScripted();
            IReadOnlyList<string> fromAuthoring =
                RunnerSmokeDriver.RunScripted(RunnerSmokeDriver.BuildDemoTutorialFromAuthoring());

            Assert.That(fromAuthoring, Is.EqualTo(fromBuilder));
        }


        [Test]
        public void RunScriptedCodeThenAuthoring_CompletesBothOnOneRunner()
        {
            IReadOnlyList<string> log = RunnerSmokeDriver.RunScriptedCodeThenAuthoring();

            Assert.That(log, Does.Contain("source:builder"));
            Assert.That(log, Does.Contain("source:authoring"));

            int builderMarker = IndexOf(log, "source:builder");
            int authoringMarker = IndexOf(log, "source:authoring");
            Assert.That(builderMarker, Is.LessThan(authoringMarker));

            // Two Tutorial Completions: one per source on the same Runner.
            var completions = 0;
            for (var i = 0; i < log.Count; i++)
            {
                if (log[i] == "TutorialCompleted sample.sword")
                    completions++;
            }

            Assert.That(completions, Is.EqualTo(2));
            Assert.That(authoringMarker, Is.LessThan(IndexOf(log, "TutorialCompleted sample.sword", authoringMarker)));
        }


        static int IndexOf(IReadOnlyList<string> log, string line, int startIndex = 0)
        {
            for (var i = startIndex; i < log.Count; i++)
            {
                if (log[i] == line)
                    return i;
            }

            return -1;
        }
    }
}
