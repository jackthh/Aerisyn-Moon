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
    ///   OpenBoard / CloseBoard  -> register or drop a Board's Quests
    ///   Report                  -> update every matching open Quest
    ///   TryClaim                -> mark a Step claimed; caller grants the reward when it returns true
    ///   Export / TryGet         -> read progress for saving or display
    /// Pure C#: no UnityEngine dependency.
    /// </summary>
    public sealed class QuestTracker
    {
        #region Nested types

        private sealed class Entry
        {
            public QuestId Id;
            public QuestDefinition Definition;
            public QuestProgress Progress;
        }

        private sealed class Board
        {
            public BoardId Id;
            public readonly List<Entry> Ordered = new List<Entry>();
            public readonly Dictionary<int, Entry> ByLocalId = new Dictionary<int, Entry>();
        }

        private struct PendingChange
        {
            public Entry Entry;
            public ulong NewlyClaimableMask;
        }

        #endregion

        #region Fields

        private readonly Dictionary<BoardId, Board> _boards = new Dictionary<BoardId, Board>();
        private readonly Dictionary<int, List<Entry>> _byKind = new Dictionary<int, List<Entry>>();

        // Reused per Report so steady-state reporting does not allocate.
        private readonly List<PendingChange> _pending = new List<PendingChange>();
        private readonly HashSet<BoardId> _dirtyBoards = new HashSet<BoardId>();

        #endregion

        #region Events

        /// <summary>A Quest's value or claimed Steps changed. Refresh its UI.</summary>
        public event Action<QuestId> ProgressChanged;

        /// <summary>A Step just crossed its threshold and can be claimed. Show the claim button.</summary>
        public event Action<QuestId, int> StepBecameClaimable;

        /// <summary>A Step was claimed via <see cref="TryClaim"/>. Rewards are granted by the caller of TryClaim, not here.</summary>
        public event Action<QuestId, int> StepClaimed;

        /// <summary>Durable progress on this Board changed. Export and hand it to your save pipeline.</summary>
        public event Action<BoardId> BoardChanged;

        #endregion

        #region Boards

        public bool IsBoardOpen(BoardId board) => _boards.ContainsKey(board);

        /// <summary>
        /// Register a Board's Quests. Local ids must be unique within the Board; duplicates throw.
        /// Snapshots are matched by local id; unknown ids are ignored so stale saves load safely.
        /// </summary>
        public void OpenBoard(BoardId board, IReadOnlyList<QuestDefinition> quests, IReadOnlyList<ProgressSnapshot> saved = null)
        {
            if (!board.IsValid)
                throw new ArgumentException("BoardId is not valid.", nameof(board));
            if (quests == null)
                throw new ArgumentNullException(nameof(quests));
            if (_boards.ContainsKey(board))
                throw new InvalidOperationException("Board '" + board + "' is already open. Close it before reopening.");

            var newBoard = new Board { Id = board };

            for (var i = 0; i < quests.Count; i++)
            {
                var def = quests[i];
                if (def == null)
                    throw new ArgumentException("Quest definition at index " + i + " is null.", nameof(quests));
                if (newBoard.ByLocalId.ContainsKey(def.LocalId))
                    throw new ArgumentException("Board '" + board + "' has duplicate quest local id " + def.LocalId + ".", nameof(quests));

                var entry = new Entry
                {
                    Id = new QuestId(board, def.LocalId),
                    Definition = def,
                    Progress = new QuestProgress()
                };
                newBoard.Ordered.Add(entry);
                newBoard.ByLocalId.Add(def.LocalId, entry);
            }

            if (saved != null)
            {
                for (var i = 0; i < saved.Count; i++)
                {
                    var snapshot = saved[i];
                    if (newBoard.ByLocalId.TryGetValue(snapshot.LocalId, out var entry))
                        entry.Progress.Restore(in snapshot, entry.Definition);
                }
            }

            // Only commit to the indexes once validation passed, so a failed open leaves no partial state.
            _boards.Add(board, newBoard);
            for (var i = 0; i < newBoard.Ordered.Count; i++)
                IndexEntry(newBoard.Ordered[i]);
        }

        /// <summary>Drop a Board from the Tracker. Progress in memory is discarded; export first if you need it.</summary>
        public bool CloseBoard(BoardId board)
        {
            if (!_boards.TryGetValue(board, out var existing))
                return false;

            for (var i = 0; i < existing.Ordered.Count; i++)
                UnindexEntry(existing.Ordered[i]);

            _boards.Remove(board);
            return true;
        }

        /// <summary>Snapshot every Quest on the Board, in definition order. Throws if the Board is not open.</summary>
        public ProgressSnapshot[] Export(BoardId board)
        {
            var existing = GetOpenBoard(board);
            var result = new ProgressSnapshot[existing.Ordered.Count];
            for (var i = 0; i < result.Length; i++)
            {
                var entry = existing.Ordered[i];
                result[i] = entry.Progress.ToSnapshot(entry.Id.LocalId);
            }

            return result;
        }

        #endregion

        #region Reading

        public bool TryGet(QuestId id, out QuestView view)
        {
            if (_boards.TryGetValue(id.Board, out var board) && board.ByLocalId.TryGetValue(id.LocalId, out var entry))
            {
                view = ToView(entry);
                return true;
            }

            view = default;
            return false;
        }

        /// <summary>Fill <paramref name="results"/> with a view per Quest on the Board, in definition order.</summary>
        public bool TryGetBoard(BoardId board, List<QuestView> results)
        {
            if (results == null)
                throw new ArgumentNullException(nameof(results));

            results.Clear();
            if (!_boards.TryGetValue(board, out var existing))
                return false;

            for (var i = 0; i < existing.Ordered.Count; i++)
                results.Add(ToView(existing.Ordered[i]));

            return true;
        }

        #endregion

        #region Reporting and claiming

        /// <summary>
        /// Gameplay reports a fact: "kind happened, about param, worth value". Every open Quest whose
        /// Objective matches is updated under its own Accumulation. Events fire after all updates are
        /// applied, so handlers observe a consistent state.
        /// </summary>
        public void Report(int kind, int param, long value)
        {
            if (!_byKind.TryGetValue(kind, out var candidates) || candidates.Count == 0)
                return;

            _pending.Clear();
            _dirtyBoards.Clear();

            for (var i = 0; i < candidates.Count; i++)
            {
                var entry = candidates[i];
                var def = entry.Definition;
                var progress = entry.Progress;

                if (!def.Objective.Matches(kind, param))
                    continue;
                if (!QuestRules.CanAccumulate(def, progress.Value, progress.ClaimedStepsMask, progress.ClaimCount))
                    continue;

                var before = progress.Value;
                var after = QuestRules.Accumulate(def.Accumulation, before, value);
                if (after == before)
                    continue;

                progress.Value = after;

                // Steps that were locked before this report and are reached now.
                ulong newlyClaimable = 0;
                for (var step = 0; step < def.StepCount; step++)
                {
                    if (progress.IsStepClaimed(step))
                        continue;
                    if (!QuestRules.IsStepReached(def, before, step) && QuestRules.IsStepReached(def, after, step))
                        newlyClaimable |= 1UL << step;
                }

                _pending.Add(new PendingChange { Entry = entry, NewlyClaimableMask = newlyClaimable });
                _dirtyBoards.Add(entry.Id.Board);
            }

            if (_pending.Count == 0)
                return;

            // Copy out before raising events: a handler may Report again and reuse the buffers.
            var changes = _pending.ToArray();
            var dirty = new BoardId[_dirtyBoards.Count];
            _dirtyBoards.CopyTo(dirty);

            for (var i = 0; i < changes.Length; i++)
            {
                var change = changes[i];
                ProgressChanged?.Invoke(change.Entry.Id);

                if (change.NewlyClaimableMask == 0)
                    continue;

                for (var step = 0; step < change.Entry.Definition.StepCount; step++)
                {
                    if ((change.NewlyClaimableMask & (1UL << step)) != 0)
                        StepBecameClaimable?.Invoke(change.Entry.Id, step);
                }
            }

            for (var i = 0; i < dirty.Length; i++)
                BoardChanged?.Invoke(dirty[i]);
        }

        /// <summary>
        /// Claim a reached, unclaimed Step. Returns true when the claim was accepted; grant the reward
        /// at that call site. Under <see cref="ClaimPolicy.RepeatWithReset"/>, claiming the last Step
        /// starts a new cycle (value back to 0) unless the repeat limit is exhausted.
        /// </summary>
        public bool TryClaim(QuestId id, int step)
        {
            if (!_boards.TryGetValue(id.Board, out var board) || !board.ByLocalId.TryGetValue(id.LocalId, out var entry))
                return false;

            var def = entry.Definition;
            if (step < 0 || step >= def.StepCount)
                return false;

            var progress = entry.Progress;
            var state = QuestRules.GetStepState(def, progress.Value, progress.ClaimedStepsMask, progress.ClaimCount, step);
            if (state != StepState.Claimable)
                return false;

            progress.MarkStepClaimed(step);

            if (QuestRules.AreAllStepsClaimed(def, progress.ClaimedStepsMask))
            {
                progress.ClaimCount++;

                // Repeatable quests start over until the limit is hit; then they stay Completed.
                if (def.ClaimPolicy == ClaimPolicy.RepeatWithReset && progress.ClaimCount < def.RepeatLimit)
                    progress.ResetCycle();
            }

            StepClaimed?.Invoke(id, step);
            ProgressChanged?.Invoke(id);
            BoardChanged?.Invoke(id.Board);
            return true;
        }

        #endregion

        #region Private helpers

        private Board GetOpenBoard(BoardId board)
        {
            if (!_boards.TryGetValue(board, out var existing))
                throw new InvalidOperationException("Board '" + board + "' is not open.");

            return existing;
        }

        private void IndexEntry(Entry entry)
        {
            var kind = entry.Definition.Objective.Kind;
            if (!_byKind.TryGetValue(kind, out var list))
            {
                list = new List<Entry>();
                _byKind.Add(kind, list);
            }

            list.Add(entry);
        }

        private void UnindexEntry(Entry entry)
        {
            var kind = entry.Definition.Objective.Kind;
            if (!_byKind.TryGetValue(kind, out var list))
                return;

            list.Remove(entry);
            if (list.Count == 0)
                _byKind.Remove(kind);
        }

        private static QuestView ToView(Entry entry) =>
            new QuestView(entry.Id, entry.Definition, entry.Progress.Value, entry.Progress.ClaimedStepsMask, entry.Progress.ClaimCount);

        #endregion
    }
}
