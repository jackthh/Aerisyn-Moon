using System.Collections.Generic;
using Aerisyn.Tutorial.Authoring;
using UnityEngine;

namespace Aerisyn.Tutorial.Samples.RunnerSmoke
{
    /// <summary>
    /// Samples~ MonoBehaviour facade over <see cref="TutorialRunner"/>.
    /// Logs Cue / Gate / Completion stubs to the Console; use context menus to Report and Cue Done.
    /// Not part of Core: games own their own presentation adapters.
    ///
    /// Flow:
    ///   OnEnable  -> Start code-built demo Tutorial, log first Cues / Gate
    ///   Context menus -> CueDone / Report / Stop / scripted smoke / SO authoring path
    ///   OnDisable -> Stop if still active
    ///
    /// Optional <see cref="_tutorialAsset"/>: assign a TutorialAsset to Start from SO projection;
    /// when null, CreateInstance + Populate still demos the Authoring path without a .asset file.
    /// </summary>
    public sealed class SampleRunnerSmoke : MonoBehaviour
    {
        #region Fields

        [Tooltip("Optional TutorialAsset. When set, 'Start from TutorialAsset' uses it; otherwise a runtime-populated demo asset is used.")]
        [SerializeField]
        private TutorialAsset _tutorialAsset;

        private readonly TutorialRunner _runner = new TutorialRunner();
        private bool _loggingAttached;

        // Runtime-only demo asset when no Inspector reference is assigned (destroyed OnDisable).
        private TutorialAsset _runtimeDemoAsset;

        #endregion


        #region Lifecycle

        private void OnEnable()
        {
            EnsureLogging();
            if (_runner.IsActive)
                return;

            _runner.Start(RunnerSmokeDriver.BuildDemoTutorial());
            Debug.Log("[Tutorial sample] Started code-built '" + _runner.ActiveTutorialId + "' at Step " + _runner.ActiveStepIndex);
        }


        private void OnDisable()
        {
            if (_runner.IsActive)
                _runner.Stop();

            if (_runtimeDemoAsset != null)
            {
                Destroy(_runtimeDemoAsset);
                _runtimeDemoAsset = null;
            }
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


        [ContextMenu("Start from code builder")]
        private void ContextStartFromBuilder()
        {
            EnsureLogging();
            if (_runner.IsActive)
                _runner.Stop();

            _runner.Start(RunnerSmokeDriver.BuildDemoTutorial());
            Debug.Log("[Tutorial sample] Started code-built '" + _runner.ActiveTutorialId + "'");
        }


        [ContextMenu("Start from TutorialAsset (SO)")]
        private void ContextStartFromAsset()
        {
            EnsureLogging();
            if (_runner.IsActive)
                _runner.Stop();

            TutorialAsset asset = ResolveTutorialAsset();
            _runner.Start(asset.Build());
            Debug.Log("[Tutorial sample] Started SO-projected '" + _runner.ActiveTutorialId + "'");
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


        [ContextMenu("Run scripted smoke (code then SO on one Runner)")]
        private void ContextRunScriptedCodeThenAuthoring()
        {
            if (_runner.IsActive)
                _runner.Stop();

            IReadOnlyList<string> lines = RunnerSmokeDriver.RunScriptedCodeThenAuthoring();
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


        /// <summary>
        /// Prefer the Inspector-assigned asset; otherwise CreateInstance + Populate the demo Steps
        /// so the SO path works without shipping a .asset in Samples~.
        /// </summary>
        private TutorialAsset ResolveTutorialAsset()
        {
            if (_tutorialAsset != null)
                return _tutorialAsset;

            if (_runtimeDemoAsset == null)
            {
                _runtimeDemoAsset = ScriptableObject.CreateInstance<TutorialAsset>();
                _runtimeDemoAsset.name = "RuntimeDemoTutorial";
                _runtimeDemoAsset.Populate("sample.sword", RunnerSmokeDriver.BuildDemoAuthoredSteps());
            }

            return _runtimeDemoAsset;
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
