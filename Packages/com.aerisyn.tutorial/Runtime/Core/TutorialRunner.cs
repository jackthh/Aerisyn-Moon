using System;

namespace Aerisyn.Tutorial
{
    /// <summary>
    /// Pure C# engine that runs at most one Tutorial at a time.
    /// Soft/Hard Steps advance on matching Reports; emits Gate, Step, and Tutorial Completion signals.
    /// Cue Choreography is out of this ticket.
    ///
    /// Outline:
    ///   Start / Stop           -> begin or abandon; Gate Started/Ended for Hard Steps
    ///   Report                 -> match against the active Step
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
        /// A Step succeeded via Report.
        /// Arguments: Tutorial id, completed Step index, Step id (stable authoring identity).
        /// Fires before Gate Ended (for Hard) and before the Runner advances.
        /// </summary>
        public event Action<TutorialId, int, string> StepCompleted;

        /// <summary>
        /// The last Step succeeded. Arguments: Tutorial id.
        /// Rewards stay outside this package; the game grants at the listen site.
        /// </summary>
        public event Action<TutorialId> TutorialCompleted;

        /// <summary>
        /// Hard Step Gate for the game to lock/unlock input/UI.
        /// Arguments: Tutorial id, Step index, Step id, GatePhase (Started or Ended).
        /// Soft Steps never raise this.
        /// </summary>
        public event Action<TutorialId, int, string, GatePhase> Gate;

        #endregion


        #region Start / Stop

        /// <summary>
        /// Starts a Tutorial at Step 0, or at <paramref name="snapshot"/> StepIndex when provided.
        /// Throws if a Tutorial is already active, the definition is null, or the snapshot is out of range.
        /// Emits Gate Started when the entered Step is Hard.
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
            EmitGateIfHard(GatePhase.Started);
        }


        /// <summary>
        /// Abandons the active Tutorial without emitting Tutorial Completion.
        /// Emits Gate Ended when the active Step is Hard. No-op when inactive.
        /// </summary>
        public void Stop()
        {
            if (!IsActive)
                return;

            EmitGateIfHard(GatePhase.Ended);
            ClearActive();
        }

        #endregion


        #region Report

        /// <summary>
        /// Gameplay fact. Matches only the active Step; unmatched Reports are ignored.
        /// On match: emits Step Completion, ends Hard Gate if any, then advances or Tutorial Completion.
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

            // Leaving a Hard Step ends its Gate before the next Step (or Tutorial Completion).
            EmitGateIfHard(GatePhase.Ended);

            int nextIndex = completedIndex + 1;
            if (nextIndex >= _activeDefinition.StepCount)
            {
                ClearActive();
                TutorialCompleted?.Invoke(tutorialId);
                return;
            }

            _activeStepIndex = nextIndex;
            EmitGateIfHard(GatePhase.Started);
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

        /// <summary>Raises Gate for the active Step when it is Hard; Soft is a no-op.</summary>
        private void EmitGateIfHard(GatePhase phase)
        {
            if (!IsActive)
                return;

            StepDefinition step = _activeDefinition.Steps[_activeStepIndex];
            if (step.Enforcement != Enforcement.Hard)
                return;

            EmitGate(_activeDefinition.Id, _activeStepIndex, step.Id, phase);
        }


        private void EmitGate(TutorialId tutorialId, int stepIndex, string stepId, GatePhase phase)
        {
            Gate?.Invoke(tutorialId, stepIndex, stepId, phase);
        }


        private void ClearActive()
        {
            _activeDefinition = null;
            _activeStepIndex = -1;
        }

        #endregion
    }
}
