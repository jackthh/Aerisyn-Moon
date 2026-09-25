using System;

namespace Aerisyn.Tutorial
{
    /// <summary>
    /// Pure C# engine that runs at most one Tutorial at a time.
    /// Soft Steps advance on matching Reports; emits Step / Tutorial Completion signals.
    /// Hard Gates and Cue Choreography are out of ticket 01.
    ///
    /// Outline:
    ///   Start / Stop           -> begin or abandon the active Tutorial
    ///   Report                 -> match against the active Soft Step only
    ///   ExportSnapshot         -> Tutorial id + Step index for the game's save pipeline
    /// </summary>
    public sealed class TutorialRunner
    {
        #region Fields

        private TutorialDefinition _activeDefinition;
        private int _activeStepIndex = -1;

        #endregion


        #region State

        /// <summary>True while a Tutorial is running (Started and not yet Completed or Stopped).</summary>
        public bool IsActive => _activeDefinition != null;

        /// <summary>Id of the active Tutorial, or default when inactive.</summary>
        public TutorialId ActiveTutorialId =>
            _activeDefinition != null ? _activeDefinition.Id : default;

        /// <summary>Zero-based index of the active Step, or -1 when inactive.</summary>
        public int ActiveStepIndex => _activeStepIndex;

        #endregion


        #region Events

        /// <summary>
        /// A Soft Step succeeded via Report.
        /// Arguments: Tutorial id, completed Step index, Step id (stable authoring identity).
        /// Fires before the Runner advances to the next Step (or Tutorial Completion).
        /// </summary>
        public event Action<TutorialId, int, string> StepCompleted;

        /// <summary>
        /// The last Soft Step succeeded. Arguments: Tutorial id.
        /// Rewards stay outside this package; the game grants at the listen site.
        /// </summary>
        public event Action<TutorialId> TutorialCompleted;

        #endregion


        #region Start / Stop

        /// <summary>
        /// Starts a Tutorial at Step 0, or at <paramref name="snapshot"/> StepIndex when provided.
        /// Throws if a Tutorial is already active, the definition is null, or the snapshot is out of range.
        /// </summary>
        public void Start(TutorialDefinition definition, ProgressSnapshot? snapshot = null)
        {
            if (definition == null)
                throw new ArgumentNullException(nameof(definition));
            if (IsActive)
                throw new InvalidOperationException(
                    "A Tutorial is already active. Stop it before starting another.");

            var resumeIndex = 0;
            if (snapshot.HasValue)
            {
                ProgressSnapshot restore = snapshot.Value;
                if (!string.Equals(restore.TutorialId, definition.Id.Value, StringComparison.Ordinal))
                {
                    throw new ArgumentException(
                        "Progress Snapshot TutorialId '" + restore.TutorialId +
                        "' does not match definition '" + definition.Id + "'.",
                        nameof(snapshot));
                }

                if (restore.StepIndex < 0 || restore.StepIndex >= definition.StepCount)
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(snapshot),
                        "Progress Snapshot StepIndex " + restore.StepIndex +
                        " is outside Tutorial Step count " + definition.StepCount + ".");
                }

                resumeIndex = restore.StepIndex;
            }

            _activeDefinition = definition;
            _activeStepIndex = resumeIndex;
        }


        /// <summary>
        /// Abandons the active Tutorial without emitting Tutorial Completion.
        /// No-op when inactive.
        /// </summary>
        public void Stop()
        {
            ClearActive();
        }

        #endregion


        #region Report

        /// <summary>
        /// Gameplay fact. Matches only the active Soft Step; unmatched Reports are ignored.
        /// On match: emits Step Completion, then advances or emits Tutorial Completion.
        /// </summary>
        public void Report(int kind, int param = 0)
        {
            if (!IsActive)
                return;

            StepDefinition activeStep = _activeDefinition.Steps[_activeStepIndex];
            if (!activeStep.ReportMatch.Matches(kind, param))
                return;

            TutorialId tutorialId = _activeDefinition.Id;
            int completedIndex = _activeStepIndex;
            string completedStepId = activeStep.Id;

            // Emit Step Completion before advancing so listeners see the finished beat.
            StepCompleted?.Invoke(tutorialId, completedIndex, completedStepId);

            int nextIndex = completedIndex + 1;
            if (nextIndex >= _activeDefinition.StepCount)
            {
                ClearActive();
                TutorialCompleted?.Invoke(tutorialId);
                return;
            }

            _activeStepIndex = nextIndex;
        }

        #endregion


        #region Progress Snapshot

        /// <summary>
        /// Exports Tutorial id + active Step index for the game's save pipeline.
        /// Throws when no Tutorial is active.
        /// </summary>
        public ProgressSnapshot ExportSnapshot()
        {
            if (!IsActive)
                throw new InvalidOperationException("No active Tutorial to export a Progress Snapshot from.");

            return new ProgressSnapshot(_activeDefinition.Id, _activeStepIndex);
        }

        #endregion


        #region Private helpers

        private void ClearActive()
        {
            _activeDefinition = null;
            _activeStepIndex = -1;
        }

        #endregion
    }
}
