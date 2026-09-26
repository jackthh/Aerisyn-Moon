using System;
using System.Collections.Generic;

namespace Aerisyn.Tutorial.Samples.RunnerSmoke
{
    /// <summary>
    /// Pure C# smoke path over the Runner seam: Soft coach + Hard upgrade with Cue Choreography.
    /// Samples~ MonoBehaviour facades and EditMode fixtures both call this; Core stays free of Unity types.
    ///
    /// Outline:
    ///   BuildDemoTutorial   -> Soft Sequential Cue, then Hard Mix (Concurrent + Sequential)
    ///   AttachLogging       -> Cue / Gate / Completion lines into a list
    ///   RunScripted         -> Start → CueDone → Reports → Tutorial Completion
    /// </summary>
    public static class RunnerSmokeDriver
    {
        #region Report kinds (sample-owned opaque ints)

        /// <summary>Soft Step succeeds when the player opens the coach menu.</summary>
        public const int ReportOpenMenu = 11;

        /// <summary>Hard Step succeeds when the player upgrades the sword (param 1).</summary>
        public const int ReportUpgradeSword = 10;

        public const int UpgradeSwordParam = 1;

        #endregion


        #region Cue ids (sample-owned opaque strings)

        public const string CueHighlightMenu = "highlight.menu";
        public const string CueTextCoach = "text.coach";
        public const string CueGlowSlot = "glow.slot";
        public const string CueSfxChime = "sfx.chime";
        public const string CueHighlightButton = "highlight.button";

        #endregion


        #region Public API

        /// <summary>Builds the Soft/Hard + Cue demo Tutorial used by the sample.</summary>
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
        /// Wires Cue / Gate / Completion handlers that append stub presentation lines to <paramref name="log"/>.
        /// </summary>
        public static void AttachLogging(TutorialRunner runner, IList<string> log)
        {
            if (runner == null)
                throw new ArgumentNullException(nameof(runner));
            if (log == null)
                throw new ArgumentNullException(nameof(log));

            runner.Cue += (tutorialId, stepIndex, stepId, cueId) =>
                log.Add("Cue " + stepId + ":" + cueId);

            runner.Gate += (tutorialId, stepIndex, stepId, phase) =>
                log.Add("Gate " + phase + " " + stepId);

            runner.StepCompleted += (tutorialId, stepIndex, stepId) =>
                log.Add("StepCompleted " + stepId);

            runner.TutorialCompleted += tutorialId =>
                log.Add("TutorialCompleted " + tutorialId.Value);
        }


        /// <summary>
        /// Drives Start → Cue Done (first Soft Cue) → Soft Report → Hard Report to Tutorial Completion.
        /// Returns the stub presentation log. Does not require finishing every Sequential Cue (Report-driven).
        /// </summary>
        public static IReadOnlyList<string> RunScripted()
        {
            var log = new List<string>();
            var runner = new TutorialRunner();
            AttachLogging(runner, log);

            runner.Start(BuildDemoTutorial());
            log.Add("CueDone coach:" + CueHighlightMenu);
            runner.CueDone(CueHighlightMenu);

            runner.Report(ReportOpenMenu, 0);
            runner.Report(ReportUpgradeSword, UpgradeSwordParam);

            return log;
        }

        #endregion
    }
}
