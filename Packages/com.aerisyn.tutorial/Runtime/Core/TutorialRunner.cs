using System;

namespace Aerisyn.Tutorial
{
    /// <summary>
    /// Pure C# engine that runs at most one Tutorial at a time.
    /// Soft/Hard Steps advance on matching Reports; emits Cue, Gate, and Completion signals.
    /// Choreography schedules Sequential (await Cue Done) and Concurrent (fire-and-forget) Cue groups.
    ///
    /// Outline:
    ///   Start / Stop           -> begin or abandon; Gate + Cue groups on enter
    ///   Report                 -> match against the active Step (independent of unfinished Cues)
    ///   CueDone                -> advance Sequential groups when the awaited Cue finishes
    ///   ExportSnapshot         -> Tutorial id + Step index for the game's save pipeline
    /// </summary>
    public sealed class TutorialRunner
    {
        #region Fields

        private TutorialDefinition _activeDefinition;
        private int _activeStepIndex = -1;

        // Index of the Cue group currently being scheduled; -1 when idle / Choreography finished.
        private int _activeGroupIndex = -1;

        // Index into a Sequential group's CueIds while awaiting Cue Done; -1 when not awaiting.
        private int _awaitingCueIndex = -1;

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

        /// <summary>
        /// Presentation Cue for the game to bind (highlight / text / etc.).
        /// Arguments: Tutorial id, Step index, Step id, opaque Cue id.
        /// </summary>
        public event Action<TutorialId, int, string, string> Cue;

        #endregion


        #region Start / Stop

        /// <summary>
        /// Starts a Tutorial at Step 0, or at <paramref name="snapshot"/> StepIndex when provided.
        /// Throws if a Tutorial is already active, the definition is null, or the snapshot is out of range.
        /// Emits Gate Started when the entered Step is Hard, then schedules that Step's Choreography.
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
            EnterActiveStep();
        }


        /// <summary>
        /// Abandons the active Tutorial without emitting Tutorial Completion.
        /// Emits Gate Ended when the active Step is Hard. Clears awaiting Cue state. No-op when inactive.
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
        /// On match: emits Step Completion even if Choreography is unfinished,
        /// ends Hard Gate if any, then advances or Tutorial Completion.
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

            // Report success abandons unfinished Choreography for this Step.
            ClearChoreographyState();

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
            EnterActiveStep();
        }

        #endregion


        #region Cue Done

        /// <summary>
        /// Presentation finished the awaited sequential Cue.
        /// Ignores when inactive, when no Cue is awaited, or when <paramref name="cueId"/> does not match.
        /// On match: emits the next sequential Cue, finishes the Sequential group and continues
        /// into following Concurrent / Sequential groups, or clears await when Choreography ends.
        /// </summary>
        public void CueDone(string cueId)
        {
            if (!IsActive || _awaitingCueIndex < 0 || _activeGroupIndex < 0)
                return;

            StepDefinition step = _activeDefinition.Steps[_activeStepIndex];
            var groups = step.Choreography.Groups;
            if (_activeGroupIndex >= groups.Count)
                return;

            CueGroup group = groups[_activeGroupIndex];
            if (group.Kind != CueGroupKind.Sequential)
                return;

            var cueIds = group.CueIds;
            if (_awaitingCueIndex >= cueIds.Count)
                return;

            string expected = cueIds[_awaitingCueIndex];
            if (!string.Equals(expected, cueId, StringComparison.Ordinal))
                return;

            int nextCueIndex = _awaitingCueIndex + 1;
            if (nextCueIndex < cueIds.Count)
            {
                _awaitingCueIndex = nextCueIndex;
                EmitCue(step, cueIds[_awaitingCueIndex]);
                return;
            }

            // Sequential group finished; continue into later groups (or idle).
            _awaitingCueIndex = -1;
            _activeGroupIndex++;
            ScheduleFromActiveGroup(step);
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

        /// <summary>Gate Started (Hard) then Cue groups for the Step now at <see cref="_activeStepIndex"/>.</summary>
        private void EnterActiveStep()
        {
            EmitGateIfHard(GatePhase.Started);
            BeginChoreography();
        }


        /// <summary>
        /// Starts Choreography for the active Step: Concurrent groups emit all Cues immediately;
        /// Sequential groups emit the first Cue and await Cue Done.
        /// </summary>
        private void BeginChoreography()
        {
            ClearChoreographyState();
            if (!IsActive)
                return;

            StepDefinition step = _activeDefinition.Steps[_activeStepIndex];
            if (step.Choreography.IsEmpty)
                return;

            _activeGroupIndex = 0;
            ScheduleFromActiveGroup(step);
        }


        /// <summary>
        /// Walks Cue groups from <see cref="_activeGroupIndex"/>: Concurrent fire-and-forget advances
        /// immediately; Sequential emits the first Cue and returns until Cue Done.
        /// </summary>
        private void ScheduleFromActiveGroup(StepDefinition step)
        {
            var groups = step.Choreography.Groups;
            while (_activeGroupIndex >= 0 && _activeGroupIndex < groups.Count)
            {
                CueGroup group = groups[_activeGroupIndex];
                if (group.Kind == CueGroupKind.Concurrent)
                {
                    // Concurrent: emit every Cue, then continue to the next group without awaiting.
                    for (var i = 0; i < group.CueIds.Count; i++)
                        EmitCue(step, group.CueIds[i]);

                    _activeGroupIndex++;
                    continue;
                }

                // Sequential: emit first Cue and wait for Cue Done.
                _awaitingCueIndex = 0;
                EmitCue(step, group.CueIds[0]);
                return;
            }

            // No more groups (or empty Choreography finished).
            ClearChoreographyState();
        }


        private void EmitCue(StepDefinition step, string cueId)
        {
            Cue?.Invoke(_activeDefinition.Id, _activeStepIndex, step.Id, cueId);
        }


        /// <summary>Raises Gate for the active Step when it is Hard; Soft is a no-op.</summary>
        private void EmitGateIfHard(GatePhase phase)
        {
            if (!IsActive)
                return;

            StepDefinition step = _activeDefinition.Steps[_activeStepIndex];
            if (step.Enforcement != Enforcement.Hard)
                return;

            Gate?.Invoke(_activeDefinition.Id, _activeStepIndex, step.Id, phase);
        }


        private void ClearChoreographyState()
        {
            _activeGroupIndex = -1;
            _awaitingCueIndex = -1;
        }


        private void ClearActive()
        {
            _activeDefinition = null;
            _activeStepIndex = -1;
            ClearChoreographyState();
        }

        #endregion
    }
}
