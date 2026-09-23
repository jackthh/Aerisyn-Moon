using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Aerisyn.Quests.Samples.BasicBoard
{
    /// <summary>
    /// Minimal end-to-end demo of one Board on a <see cref="QuestTracker"/>.
    ///
    /// Flow:
    ///   OnEnable   -> build definitions in code, restore the temp JSON save, open the board
    ///   Report*    -> gameplay facts (use the component context menu in the Inspector)
    ///   ClaimAll   -> claims every claimable step and "grants" rewards from the game-side table
    ///   OnDisable  -> close the board
    /// Rewards and their contents live here in the game, keyed by (localId, step); the package never sees them.
    /// </summary>
    public sealed class SampleStageBoard : MonoBehaviour
    {
        #region Local ids for the sample quests

        private const int ServeTenCustomers = 1;
        private const int EarnCashStepped = 2;
        private const int StallOneToLevel25 = 3;
        private const int UnlockStallTwo = 4;
        private const int ServeThreeRepeatable = 5;

        #endregion


        #region Fields

        [SerializeField] private string boardName = "sample.stage";
        [SerializeField] private bool persistWithTempJson = true;

        private readonly QuestTracker _tracker = new();
        private readonly List<QuestView> _viewBuffer = new();
        private readonly Dictionary<(int localId, int step), string> _rewards = new();
        private BoardId _board;

        public QuestTracker Tracker => _tracker;

        #endregion


        #region Lifecycle

        private void OnEnable()
        {
            _board = new BoardId(boardName);

            _tracker.ProgressChanged += OnProgressChanged;
            _tracker.StepBecameClaimable += OnStepBecameClaimable;
            _tracker.BoardChanged += OnBoardChanged;

            var saved = persistWithTempJson ? TempJsonProgressStore.Load(_board) : null;
            _tracker.OpenBoard(_board, BuildDefinitions(), saved);

            Debug.Log("[Quests sample] Opened board '" + _board + "'" +
                      (saved != null ? " (restored " + saved.Length + " snapshot(s))" : ""));
            LogBoard();
        }


        private void OnDisable()
        {
            _tracker.ProgressChanged -= OnProgressChanged;
            _tracker.StepBecameClaimable -= OnStepBecameClaimable;
            _tracker.BoardChanged -= OnBoardChanged;
            _tracker.CloseBoard(_board);
        }

        #endregion


        #region Definitions (would normally come from CSV or QuestBoardAsset)

        private QuestDefinition[] BuildDefinitions()
        {
            _rewards.Clear();
            _rewards[(ServeTenCustomers, 0)] = "15 gold";
            _rewards[(EarnCashStepped, 0)] = "1 gem";
            _rewards[(EarnCashStepped, 1)] = "3 gems";
            _rewards[(EarnCashStepped, 2)] = "10 gems";
            _rewards[(StallOneToLevel25, 0)] = "40 gold";
            _rewards[(UnlockStallTwo, 0)] = "1 crate";
            _rewards[(ServeThreeRepeatable, 0)] = "1 ticket";

            return new[]
            {
                // Counter: any customer served counts.
                new QuestDefinition(ServeTenCustomers, Objective.Any((int)SampleObjectiveKind.ServeCustomer),
                    Accumulation.Sum, 10),

                // Stepped achievement: progress never resets, three rewards.
                new QuestDefinition(EarnCashStepped, Objective.Any((int)SampleObjectiveKind.EarnCash), Accumulation.Sum,
                    new long[] { 100, 1_000, 10_000 }, ClaimPolicy.OncePerStep, 1),

                // High-water: report the stall's level, quest completes when it reaches 25.
                new QuestDefinition(StallOneToLevel25, new Objective((int)SampleObjectiveKind.UpgradeStall, 1),
                    Accumulation.HighWater, 25),

                // Flag: unlocking stall 2 once is enough.
                new QuestDefinition(UnlockStallTwo, new Objective((int)SampleObjectiveKind.UnlockStall, 2),
                    Accumulation.Flag, 1),

                // Repeatable daily-style: claim, reset, up to 3 times.
                new QuestDefinition(ServeThreeRepeatable, Objective.Any((int)SampleObjectiveKind.ServeCustomer),
                    Accumulation.Sum,
                    new long[] { 3 }, ClaimPolicy.RepeatWithReset, 3)
            };
        }

        #endregion


        #region Gameplay reports (Inspector context menu)

        [ContextMenu("Report / Serve customer at stall 1")]
        public void ReportServeCustomer() => _tracker.Report((int)SampleObjectiveKind.ServeCustomer, 1, 1);


        [ContextMenu("Report / Earn 250 cash")]
        public void ReportEarnCash() => _tracker.Report((int)SampleObjectiveKind.EarnCash, 0, 250);


        [ContextMenu("Report / Stall 1 upgraded to level 25")]
        public void ReportStallLevel25() => _tracker.Report((int)SampleObjectiveKind.UpgradeStall, 1, 25);


        [ContextMenu("Report / Unlock stall 2")]
        public void ReportUnlockStallTwo() => _tracker.Report((int)SampleObjectiveKind.UnlockStall, 2, 1);


        [ContextMenu("Claim all claimable steps")]
        public void ClaimAll()
        {
            if (!_tracker.TryGetBoard(_board, _viewBuffer))
                return;

            for (var i = 0; i < _viewBuffer.Count; i++)
            {
                var view = _viewBuffer[i];
                for (var step = 0; step < view.StepCount; step++)
                {
                    if (view.GetStepState(step) != StepState.Claimable)
                        continue;

                    // TryClaim returning true is the signal to grant. Rewards are game data.
                    if (_tracker.TryClaim(view.Id, step))
                        Debug.Log("[Quests sample] Claimed " + view.Id + " step " + step + " -> granted " +
                                  _rewards[(view.Id.LocalId, step)]);
                }
            }

            LogBoard();
        }


        [ContextMenu("Reset temp save and reopen")]
        public void ResetTempSave()
        {
            TempJsonProgressStore.Delete(_board);
            enabled = false;
            enabled = true;
        }

        #endregion


        #region Tracker events

        private void OnProgressChanged(QuestId id)
        {
            if (_tracker.TryGetQuest(id, out var view))
                Debug.Log("[Quests sample] " + id + " = " + view.ProgressValue + " (" + view.State + ")");
        }


        private void OnStepBecameClaimable(QuestId id, int step) =>
            Debug.Log("[Quests sample] " + id + " step " + step + " is claimable");


        // Durable progress changed: export and hand to the (temporary) store.
        private void OnBoardChanged(BoardId board)
        {
            if (persistWithTempJson)
                TempJsonProgressStore.Save(board, _tracker.ExportBoard(board));
        }

        #endregion


        private void LogBoard()
        {
            if (!_tracker.TryGetBoard(_board, _viewBuffer))
                return;

            for (var i = 0; i < _viewBuffer.Count; i++)
            {
                var view = _viewBuffer[i];
                var target = view.FirstUnclaimedStepIndex >= 0 ? view.GetStepThreshold(view.FirstUnclaimedStepIndex).ToString() : "-";
                Debug.Log("[Quests sample]   quest " + view.Id.LocalId + ": " + view.ProgressValue + " / " + target + "  " +
                          view.State + "  claims=" + view.CompletedCycles);
            }
        }
    }
}