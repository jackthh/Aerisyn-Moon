using System.Collections.Generic;
using UnityEngine;

namespace Aerisyn.Tutorial.Samples.RunnerSmoke
{
    /// <summary>
    /// Samples~ MonoBehaviour facade over <see cref="TutorialRunner"/>.
    /// Logs Cue / Gate / Completion stubs to the Console; use context menus to Report and Cue Done.
    /// Not part of Core: games own their own presentation adapters.
    ///
    /// Flow:
    ///   OnEnable  -> Start demo Tutorial, log first Cues / Gate
    ///   Context menus -> CueDone / Report Soft / Report Hard / Stop / scripted smoke
    ///   OnDisable -> Stop if still active
    /// </summary>
    public sealed class SampleRunnerSmoke : MonoBehaviour
    {
        #region Fields

        private readonly TutorialRunner _runner = new TutorialRunner();
        private bool _loggingAttached;

        #endregion


        #region Lifecycle

        private void OnEnable()
        {
            EnsureLogging();
            if (_runner.IsActive)
                return;

            _runner.Start(RunnerSmokeDriver.BuildDemoTutorial());
            Debug.Log("[Tutorial sample] Started '" + _runner.ActiveTutorialId + "' at Step " + _runner.ActiveStepIndex);
        }


        private void OnDisable()
        {
            if (_runner.IsActive)
                _runner.Stop();
        }

        #endregion


        #region Context menus (Play Mode smoke)

        [ContextMenu("Cue Done / highlight.menu")]
        private void ContextCueDoneHighlightMenu() =>
            ApplyCueDone(RunnerSmokeDriver.CueHighlightMenu);


        [ContextMenu("Cue Done / highlight.button")]
        private void ContextCueDoneHighlightButton() =>
            ApplyCueDone(RunnerSmokeDriver.CueHighlightButton);


        [ContextMenu("Report / Soft open menu")]
        private void ContextReportOpenMenu()
        {
            EnsureLogging();
            _runner.Report(RunnerSmokeDriver.ReportOpenMenu, 0);
        }


        [ContextMenu("Report / Hard upgrade sword")]
        private void ContextReportUpgradeSword()
        {
            EnsureLogging();
            _runner.Report(RunnerSmokeDriver.ReportUpgradeSword, RunnerSmokeDriver.UpgradeSwordParam);
        }


        [ContextMenu("Stop Tutorial")]
        private void ContextStop()
        {
            EnsureLogging();
            _runner.Stop();
            Debug.Log("[Tutorial sample] Stopped (no Tutorial Completion)");
        }


        [ContextMenu("Run scripted smoke (full path)")]
        private void ContextRunScriptedSmoke()
        {
            if (_runner.IsActive)
                _runner.Stop();

            IReadOnlyList<string> lines = RunnerSmokeDriver.RunScripted();
            for (var i = 0; i < lines.Count; i++)
                Debug.Log("[Tutorial sample] " + lines[i]);
        }

        #endregion


        #region Private helpers

        private void ApplyCueDone(string cueId)
        {
            EnsureLogging();
            _runner.CueDone(cueId);
            Debug.Log("[Tutorial sample] CueDone " + cueId);
        }


        /// <summary>Stub presentation: print Runner signals to the Console once per component lifetime.</summary>
        private void EnsureLogging()
        {
            if (_loggingAttached)
                return;

            RunnerSmokeDriver.AttachLogging(
                _runner,
                line => Debug.Log("[Tutorial sample] " + line));
            _loggingAttached = true;
        }

        #endregion
    }
}
