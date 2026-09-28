using System;
using System.Collections.Generic;
using Aerisyn.Tutorial.Authoring;

namespace Aerisyn.Tutorial.Samples.RunnerSmoke
{
    /// <summary>
    /// Pure C# smoke path over the Runner seam: Soft coach + Hard upgrade with Cue Choreography.
    /// Samples~ MonoBehaviour facades and EditMode fixtures both call this; Core stays free of Unity types.
    ///
    /// Outline:
    ///   BuildDemoTutorial              -> Soft Sequential Cue, then Hard Mix (Concurrent + Sequential)
    ///   BuildDemoAuthoredSteps / FromAuthoring -> same shape via SO projection seam
    ///   AttachLogging                  -> Cue / Gate / Completion lines into a sink
    ///   RunScripted                    -> Start → CueDone → Reports → Tutorial Completion
    ///   RunScriptedCodeThenAuthoring   -> same Runner: code-built then SO-projected
    /// </summary>
    public static class RunnerSmokeDriver
    {
        #region Report kinds (sample-owned opaque ints)

        /// <summary>Soft Step succeeds when the player opens the coach menu.</summary>
        public const int ReportOpenMenu = 11;

        /// <summary>Hard Step succeeds when the player upgrades the sword (param 1).</summary>
        public const int ReportUpgradeSword = 10;

        /// <summary>Param paired with <see cref="ReportUpgradeSword"/> for the Hard Report match.</summary>
        public const int UpgradeSwordParam = 1;

        #endregion


        #region Cue ids (sample-owned opaque strings)

        /// <summary>Soft Sequential Cue: highlight the coach menu control.</summary>
        public const string CueHighlightMenu = "highlight.menu";

        /// <summary>Soft Sequential Cue: coach copy (after menu highlight).</summary>
        public const string CueTextCoach = "text.coach";

        /// <summary>Hard Concurrent Cue: glow the upgrade slot.</summary>
        public const string CueGlowSlot = "glow.slot";

        /// <summary>Hard Concurrent Cue: chime SFX with the glow.</summary>
        public const string CueSfxChime = "sfx.chime";

        /// <summary>Hard Sequential Cue: highlight the upgrade button.</summary>
        public const string CueHighlightButton = "highlight.button";

        #endregion


        #region Public API

        /// <summary>Builds the Soft/Hard + Cue demo Tutorial used by the sample (code-first builder).</summary>
        public static TutorialDefinition BuildDemoTutorial()
        {
            return new TutorialBuilder("sample.sword")
                .SoftStep(
                    "coach",
                    ReportMatch.AnyParam(ReportOpenMenu),
                    ChoreographyDefinition.Sequential(CueHighlightMenu, CueTextCoach))
                .HardStep(
                    "upgrade",
                    new ReportMatch(ReportUpgradeSword, UpgradeSwordParam),
                    ChoreographyDefinition.Mix(
                        ChoreographyDefinition.Concurrent(CueGlowSlot, CueSfxChime),
                        ChoreographyDefinition.Sequential(CueHighlightButton)))
                .Build();
        }


        /// <summary>
        /// Same demo as <see cref="BuildDemoTutorial"/> expressed as list-shaped Authoring data
        /// (what a <see cref="TutorialAsset"/> serializes). No per-beat subclass.
        /// </summary>
        public static AuthoredStep[] BuildDemoAuthoredSteps() =>
            new[]
            {
                new AuthoredStep
                {
                    Id = "coach",
                    Enforcement = Enforcement.Soft,
                    ReportKind = ReportOpenMenu,
                    MatchAnyParam = true,
                    CueGroups = new[]
                    {
                        new AuthoredCueGroup
                        {
                            Kind = CueGroupKind.Sequential,
                            CueIds = new[] { CueHighlightMenu, CueTextCoach },
                        },
                    },
                },
                new AuthoredStep
                {
                    Id = "upgrade",
                    Enforcement = Enforcement.Hard,
                    ReportKind = ReportUpgradeSword,
                    ReportParam = UpgradeSwordParam,
                    MatchAnyParam = false,
                    CueGroups = new[]
                    {
                        new AuthoredCueGroup
                        {
                            Kind = CueGroupKind.Concurrent,
                            CueIds = new[] { CueGlowSlot, CueSfxChime },
                        },
                        new AuthoredCueGroup
                        {
                            Kind = CueGroupKind.Sequential,
                            CueIds = new[] { CueHighlightButton },
                        },
                    },
                },
            };


        /// <summary>Projects the authored demo Steps into the same Core definition the builder produces.</summary>
        public static TutorialDefinition BuildDemoTutorialFromAuthoring() =>
            TutorialAuthoringProjection.Project("sample.sword", BuildDemoAuthoredSteps());


        /// <summary>
        /// Wires Cue / Gate / Completion handlers that write stub presentation lines via <paramref name="write"/>.
        /// </summary>
        public static void AttachLogging(TutorialRunner runner, Action<string> write)
        {
            if (runner == null)
                throw new ArgumentNullException(nameof(runner));
            if (write == null)
                throw new ArgumentNullException(nameof(write));

            runner.Cue += (tutorialId, stepIndex, stepId, cueId) =>
                write("Cue " + stepId + ":" + cueId);

            runner.Gate += (tutorialId, stepIndex, stepId, phase) =>
                write("Gate " + phase + " " + stepId);

            runner.StepCompleted += (tutorialId, stepIndex, stepId) =>
                write("StepCompleted " + stepId);

            runner.TutorialCompleted += tutorialId =>
                write("TutorialCompleted " + tutorialId.Value);
        }


        /// <summary>
        /// Drives Start → Cue Done (first Soft Cue) → Soft Report → Hard Report to Tutorial Completion.
        /// Returns the stub presentation log. Does not require finishing every Sequential Cue (Report-driven).
        /// </summary>
        public static IReadOnlyList<string> RunScripted() =>
            RunScripted(BuildDemoTutorial());


        /// <summary>
        /// Same scripted path as <see cref="RunScripted()"/> but for an arbitrary definition
        /// (builder or Authoring projection).
        /// </summary>
        public static IReadOnlyList<string> RunScripted(TutorialDefinition tutorial)
        {
            var log = new List<string>();
            var runner = new TutorialRunner();
            AttachLogging(runner, line => log.Add(line));
            DriveToCompletion(runner, tutorial, line => log.Add(line));
            return log;
        }


        /// <summary>
        /// One Runner: complete the code-built demo, then the Authoring-projected demo.
        /// Proves SO projection feeds the same seam without a second runtime model.
        /// </summary>
        public static IReadOnlyList<string> RunScriptedCodeThenAuthoring()
        {
            var log = new List<string>();
            var runner = new TutorialRunner();
            AttachLogging(runner, line => log.Add(line));

            log.Add("source:builder");
            DriveToCompletion(runner, BuildDemoTutorial(), line => log.Add(line));

            log.Add("source:authoring");
            DriveToCompletion(runner, BuildDemoTutorialFromAuthoring(), line => log.Add(line));

            return log;
        }

        #endregion


        #region Private helpers

        /// <summary>
        /// Start → Cue Done (first Soft Cue) → Soft Report → Hard Report on <paramref name="runner"/>.
        /// Prefixed CueDone lines are for readable smoke logs (Runner has no outbound CueDone signal).
        /// </summary>
        static void DriveToCompletion(TutorialRunner runner, TutorialDefinition tutorial, Action<string> write)
        {
            runner.Start(tutorial);
            write("CueDone coach:" + CueHighlightMenu);
            runner.CueDone(CueHighlightMenu);
            runner.Report(ReportOpenMenu, 0);
            runner.Report(ReportUpgradeSword, UpgradeSwordParam);
        }

        #endregion
    }
}
