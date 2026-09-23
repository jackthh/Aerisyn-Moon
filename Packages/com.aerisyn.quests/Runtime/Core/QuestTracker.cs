using System;
using System.Collections.Generic;

namespace Aerisyn.Quests
{
    /// <summary>
    /// In-memory quest engine. Games open Boards (definitions + saved snapshots), report facts,
    /// and claim Steps. The Tracker indexes open Quests by Objective kind, applies accumulation,
    /// derives claimability, and raises events for UI. It never grants rewards and never saves.
    ///
    /// Outline:
    ///   OpenBoard / CloseBoard      -> register or drop a Board's Quests
    ///   Report                      -> update every matching open Quest
    ///   TryClaim                    -> mark a Step claimed; caller grants the reward when it returns true
    ///   ExportBoard / TryGetQuest   -> read progress for saving or display
    /// Pure C#: no UnityEngine dependency.
    /// </summary>
    public sealed class QuestTracker
    {
        #region Nested types

        /// <summary>One Quest on an open Board: identity, immutable rules, and live progress.</summary>
        private sealed class TrackedQuest
        {
            public QuestId Id;
            public QuestDefinition Definition;
            public QuestProgress Progress;
        }

        /// <summary>An open Board and two views of the same Quests: ordered for export/UI, keyed for lookup.</summary>
        private sealed class OpenBoardState
        {
            public BoardId Id;
            public readonly List<TrackedQuest> QuestsInDefinitionOrder = new();
            public readonly Dictionary<int, TrackedQuest> QuestsByLocalId = new();
        }

        /// <summary>A Quest whose value moved during the current <see cref="Report"/>, queued so events fire after all updates.</summary>
        private struct ReportedChange
        {
            public TrackedQuest Quest;

            /// <summary>Bit i set means Step i went from not reached to reached during this report.</summary>
            public ulong NewlyClaimableStepsMask;
        }

        #endregion


        #region Fields

        private readonly Dictionary<BoardId, OpenBoardState> _openBoards = new Dictionary<BoardId, OpenBoardState>();

        /// <summary>Every open Quest grouped by <see cref="Objective.Kind"/>, so a Report only scans Quests that could match.</summary>
        private readonly Dictionary<int, List<TrackedQuest>> _questsByObjectiveKind = new();

        // Scratch buffers reused per Report so steady-state reporting does not allocate.
        private readonly List<ReportedChange> _reportedChangesBuffer = new();
        private readonly HashSet<BoardId> _changedBoardsBuffer = new();

        #endregion


        #region Events

        /// <summary>A Quest's value or claimed Steps changed. Refresh its UI. Argument: the Quest.</summary>
        public event Action<QuestId> ProgressChanged;

        /// <summary>A Step just crossed its threshold and can be claimed. Show the claim button. Arguments: the Quest, the step index.</summary>
        public event Action<QuestId, int> StepBecameClaimable;

        /// <summary>
        /// A Step was claimed via <see cref="TryClaim"/>. Arguments: the Quest, the step index.
        /// Rewards are granted by the caller of TryClaim, not here.
        /// </summary>
        public event Action<QuestId, int> StepClaimed;

        /// <summary>Durable progress on this Board changed. Call <see cref="ExportBoard"/> and hand it to your save pipeline.</summary>
        public event Action<BoardId> BoardChanged;

        #endregion


        #region Boards

        public bool IsBoardOpen(BoardId boardId) => _openBoards.ContainsKey(boardId);


        /// <summary>
        /// Register a Board's Quests so Reports reach them.
        /// Steps: validate arguments, build the Board (rejecting null or duplicate local ids),
        /// restore <paramref name="savedSnapshots"/> by local id, then commit to the indexes.
        /// Unknown snapshot ids are ignored so stale saves load safely. Throws if the Board is already open.
        /// </summary>
        public void OpenBoard(
            BoardId boardId,
            IReadOnlyList<QuestDefinition> definitions,
            IReadOnlyList<ProgressSnapshot> savedSnapshots = null)
        {
            if (!boardId.IsValid)
                throw new ArgumentException("BoardId is not valid.", nameof(boardId));
            if (definitions == null)
                throw new ArgumentNullException(nameof(definitions));
            if (_openBoards.ContainsKey(boardId))
                throw new InvalidOperationException("Board '" + boardId +
                                                    "' is already open. Close it before reopening.");

            //  NOTE:   Build the Board off to the side; nothing is visible to Report until the final commit.
            var newBoard = new OpenBoardState { Id = boardId };

            for (var definitionIndex = 0; definitionIndex < definitions.Count; definitionIndex++)
            {
                var definition = definitions[definitionIndex];
                if (definition == null)
                    throw new ArgumentException("Quest definition at index " + definitionIndex + " is null.",
                        nameof(definitions));
                if (newBoard.QuestsByLocalId.ContainsKey(definition.LocalId))
                    throw new ArgumentException(
                        "Board '" + boardId + "' has duplicate quest local id " + definition.LocalId + ".",
                        nameof(definitions));

                var trackedQuest = new TrackedQuest
                {
                    Id = new QuestId(boardId, definition.LocalId),
                    Definition = definition,
                    Progress = new QuestProgress()
                };
                newBoard.QuestsInDefinitionOrder.Add(trackedQuest);
                newBoard.QuestsByLocalId.Add(definition.LocalId, trackedQuest);
            }

            //  NOTE:   Restore saved progress; snapshots for Quests no longer on the Board are skipped.
            if (savedSnapshots != null)
            {
                for (var snapshotIndex = 0; snapshotIndex < savedSnapshots.Count; snapshotIndex++)
                {
                    var snapshot = savedSnapshots[snapshotIndex];
                    if (newBoard.QuestsByLocalId.TryGetValue(snapshot.LocalId, out var trackedQuest))
                        trackedQuest.Progress.Restore(in snapshot, trackedQuest.Definition);
                }
            }

            //  NOTE:   Only commit to the indexes once validation passed, so a failed open leaves no partial state.
            _openBoards.Add(boardId, newBoard);
            for (var questIndex = 0; questIndex < newBoard.QuestsInDefinitionOrder.Count; questIndex++)
                AddToObjectiveKindIndex(newBoard.QuestsInDefinitionOrder[questIndex]);
        }


        /// <summary>
        /// Drop a Board from the Tracker. Progress in memory is discarded; export first if you need it.
        /// Returns false when the Board was not open.
        /// </summary>
        public bool CloseBoard(BoardId boardId)
        {
            if (!_openBoards.TryGetValue(boardId, out var openBoard))
                return false;

            for (var i = 0; i < openBoard.QuestsInDefinitionOrder.Count; i++)
                RemoveFromObjectiveKindIndex(openBoard.QuestsInDefinitionOrder[i]);

            _openBoards.Remove(boardId);
            return true;
        }


        /// <summary>Snapshot every Quest on the Board, in definition order, ready to save. Throws if the Board is not open.</summary>
        public ProgressSnapshot[] ExportBoard(BoardId boardId)
        {
            var openBoard = GetOpenBoardOrThrow(boardId);
            var snapshots = new ProgressSnapshot[openBoard.QuestsInDefinitionOrder.Count];
            for (var questIndex = 0; questIndex < snapshots.Length; questIndex++)
            {
                var trackedQuest = openBoard.QuestsInDefinitionOrder[questIndex];
                snapshots[questIndex] = trackedQuest.Progress.ToSnapshot(trackedQuest.Id.LocalId);
            }

            return snapshots;
        }

        #endregion


        #region Reading

        /// <summary>Read one Quest. Returns false when its Board is not open or the local id is unknown.</summary>
        public bool TryGetQuest(QuestId questId, out QuestView view)
        {
            if (TryFindTrackedQuest(questId, out var trackedQuest))
            {
                view = CreateView(trackedQuest);
                return true;
            }

            view = default;
            return false;
        }


        /// <summary>
        /// Fill <paramref name="results"/> with a view per Quest on the Board, in definition order.
        /// The list is always cleared first; returns false (list left empty) when the Board is not open.
        /// </summary>
        public bool TryGetBoard(BoardId boardId, List<QuestView> results)
        {
            if (results == null)
                throw new ArgumentNullException(nameof(results));

            results.Clear();
            if (!_openBoards.TryGetValue(boardId, out var openBoard))
                return false;

            for (var questIndex = 0; questIndex < openBoard.QuestsInDefinitionOrder.Count; questIndex++)
                results.Add(CreateView(openBoard.QuestsInDefinitionOrder[questIndex]));

            return true;
        }

        #endregion


        #region Reporting and claiming

        /// <summary>
        /// Gameplay reports a fact: "<paramref name="objectiveKind"/> happened, about <paramref name="objectiveParam"/>,
        /// worth <paramref name="reportedValue"/>". Every open Quest whose Objective matches is updated under its own Accumulation.
        /// Steps:
        ///   1) scan Quests indexed under this kind, skipping ones that do not match or cannot move;
        ///   2) apply accumulation and record which Steps became reached;
        ///   3) after all Quests are updated, raise ProgressChanged / StepBecameClaimable per Quest,
        ///      then BoardChanged once per affected Board, so handlers observe a consistent state.
        /// </summary>
        public void Report(int objectiveKind, int objectiveParam, long reportedValue)
        {
            if (!_questsByObjectiveKind.TryGetValue(objectiveKind, out var candidateQuests) ||
                candidateQuests.Count == 0)
                return;

            _reportedChangesBuffer.Clear();
            _changedBoardsBuffer.Clear();

            //  NOTE:   Phase 1 + 2: mutate progress, queue changes.
            for (var candidateIndex = 0; candidateIndex < candidateQuests.Count; candidateIndex++)
            {
                var trackedQuest = candidateQuests[candidateIndex];
                var definition = trackedQuest.Definition;
                var progress = trackedQuest.Progress;

                if (!definition.Objective.Matches(objectiveKind, objectiveParam))
                    continue;
                if (!QuestRules.CanAccumulate(definition, progress.ProgressValue, progress.ClaimedStepsMask,
                        progress.CompletedCycles))
                    continue;

                var valueBefore = progress.ProgressValue;
                var valueAfter = QuestRules.Accumulate(definition.Accumulation, valueBefore, reportedValue);
                if (valueAfter == valueBefore)
                    continue;

                progress.ProgressValue = valueAfter;

                //  NOTE:   Steps that were not reached before this report and are reached now.
                ulong newlyClaimableStepsMask = 0;
                for (var stepIndex = 0; stepIndex < definition.StepCount; stepIndex++)
                {
                    if (progress.IsStepClaimed(stepIndex))
                        continue;
                    if (!QuestRules.IsStepReached(definition, valueBefore, stepIndex) &&
                        QuestRules.IsStepReached(definition, valueAfter, stepIndex))
                        newlyClaimableStepsMask |= 1UL << stepIndex;
                }

                _reportedChangesBuffer.Add(new ReportedChange
                    { Quest = trackedQuest, NewlyClaimableStepsMask = newlyClaimableStepsMask });
                _changedBoardsBuffer.Add(trackedQuest.Id.BoardId);
            }

            if (_reportedChangesBuffer.Count == 0)
                return;

            // Copy out before raising events: a handler may Report again and reuse the buffers.
            var reportedChanges = _reportedChangesBuffer.ToArray();
            var changedBoards = new BoardId[_changedBoardsBuffer.Count];
            _changedBoardsBuffer.CopyTo(changedBoards);

            //  NOTE:   Phase 3: notify.
            for (var changeIndex = 0; changeIndex < reportedChanges.Length; changeIndex++)
            {
                var change = reportedChanges[changeIndex];
                ProgressChanged?.Invoke(change.Quest.Id);

                if (change.NewlyClaimableStepsMask == 0)
                    continue;

                for (var stepIndex = 0; stepIndex < change.Quest.Definition.StepCount; stepIndex++)
                {
                    if ((change.NewlyClaimableStepsMask & (1UL << stepIndex)) != 0)
                        StepBecameClaimable?.Invoke(change.Quest.Id, stepIndex);
                }
            }

            for (var boardIndex = 0; boardIndex < changedBoards.Length; boardIndex++)
                BoardChanged?.Invoke(changedBoards[boardIndex]);
        }


        /// <summary>
        /// Claim a reached, unclaimed Step (<paramref name="stepIndex"/> is 0-based). Returns true when the
        /// claim was accepted; grant the reward at that call site. When this claim completes the last Step,
        /// the cycle counter increments, and under <see cref="ClaimPolicy.RepeatWithReset"/> a new cycle
        /// starts (value back to 0) unless the repeat limit is now exhausted.
        /// </summary>
        public bool TryClaim(QuestId questId, int stepIndex)
        {
            if (!TryFindTrackedQuest(questId, out var trackedQuest))
                return false;

            var definition = trackedQuest.Definition;
            if (stepIndex < 0 || stepIndex >= definition.StepCount)
                return false;

            var progress = trackedQuest.Progress;
            var stepState = QuestRules.GetStepState(definition, progress.ProgressValue, progress.ClaimedStepsMask,
                progress.CompletedCycles, stepIndex);
            if (stepState != StepState.Claimable)
                return false;

            progress.MarkStepClaimed(stepIndex);

            if (QuestRules.AreAllStepsClaimed(definition, progress.ClaimedStepsMask))
            {
                progress.CompletedCycles++;

                // Repeatable quests start over until the limit is hit; then they stay Completed.
                if (definition.ClaimPolicy == ClaimPolicy.RepeatWithReset &&
                    progress.CompletedCycles < definition.RepeatLimit)
                    progress.ResetCycle();
            }

            StepClaimed?.Invoke(questId, stepIndex);
            ProgressChanged?.Invoke(questId);
            BoardChanged?.Invoke(questId.BoardId);
            return true;
        }

        #endregion


        #region Private helpers

        private OpenBoardState GetOpenBoardOrThrow(BoardId boardId)
        {
            if (!_openBoards.TryGetValue(boardId, out var openBoard))
                throw new InvalidOperationException("Board '" + boardId + "' is not open.");

            return openBoard;
        }


        private bool TryFindTrackedQuest(QuestId questId, out TrackedQuest trackedQuest)
        {
            if (_openBoards.TryGetValue(questId.BoardId, out var openBoard))
                return openBoard.QuestsByLocalId.TryGetValue(questId.LocalId, out trackedQuest);

            trackedQuest = null;
            return false;
        }


        /// <summary>
        /// Add a TrackedQuest to the list of Quests grouped by ObjectiveKind.
        /// </summary>
        private void AddToObjectiveKindIndex(TrackedQuest trackedQuest)
        {
            var objectiveKind = trackedQuest.Definition.Objective.Kind;
            if (!_questsByObjectiveKind.TryGetValue(objectiveKind, out var questsOfKind))
            {
                questsOfKind = new List<TrackedQuest>();
                _questsByObjectiveKind.Add(objectiveKind, questsOfKind);
            }

            questsOfKind.Add(trackedQuest);
        }


        /// <summary>
        /// Remove a TrackedQuest from the list of Quests grouped by ObjectiveKind.
        /// </summary>
        private void RemoveFromObjectiveKindIndex(TrackedQuest trackedQuest)
        {
            var objectiveKind = trackedQuest.Definition.Objective.Kind;
            if (!_questsByObjectiveKind.TryGetValue(objectiveKind, out var questsOfKind))
                return;

            questsOfKind.Remove(trackedQuest);
            if (questsOfKind.Count == 0)
                _questsByObjectiveKind.Remove(objectiveKind);
        }


        private static QuestView CreateView(TrackedQuest trackedQuest) =>
            new(
                trackedQuest.Id,
                trackedQuest.Definition,
                trackedQuest.Progress.ProgressValue,
                trackedQuest.Progress.ClaimedStepsMask,
                trackedQuest.Progress.CompletedCycles);

        #endregion
    }
}