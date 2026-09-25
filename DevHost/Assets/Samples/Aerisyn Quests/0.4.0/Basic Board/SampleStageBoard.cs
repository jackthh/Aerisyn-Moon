using System.Collections.Generic;
using UnityEngine;

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
    /// Rewards and their contents live here in the game, keyed by (localId, stepIndex); the package never sees them.
    /// </summary>
    public sealed class SampleStageBoard : MonoBehaviour
    {
        #region Local ids for the sample quests

        private const int ServeTenCustomersQuestId = 1;
        private const int EarnCashSteppedQuestId = 2;
        private const int StallOneToLevel25QuestId = 3;
        private const int UnlockStallTwoQuestId = 4;
        private const int ServeThreeRepeatableQuestId = 5;

        #endregion

        #region Fields

        [Tooltip("Name of the Board this component opens. Also names the temp save file.")]
        [SerializeField] private string _boardName = "sample.stage";

        [Tooltip("Save and restore progress through TempJsonProgressStore (sample only).")]
        [SerializeField] private bool _persistWithTempJson = true;

        private readonly QuestTracker _tracker = new QuestTracker();

        // Reused by ClaimAll and LogBoardSummary so reading the board does not allocate.
        private readonly List<QuestView> _questViewsBuffer = new List<QuestView>();

        /// <summary>Game-side reward table, keyed by (quest local id, step index). Plain labels; a real game stores item/currency data.</summary>
        private readonly Dictionary<(int localId, int stepIndex), string> _rewardLabels = new Dictionary<(int, int), string>();

        private BoardId _boardId;

        public QuestTracker Tracker => _tracker;

        #endregion

        #region Lifecycle

        private void OnEnable()
        {
            _boardId = new BoardId(_boardName);

            _tracker.ProgressChanged += OnProgressChanged;
            _tracker.StepBecameClaimable += OnStepBecameClaimable;
            _tracker.BoardChanged += OnBoardChanged;

            var savedSnapshots = _persistWithTempJson ? TempJsonProgressStore.Load(_boardId) : null;
            _tracker.OpenBoard(_boardId, BuildDefinitions(), savedSnapshots);

            Debug.Log("[Quests sample] Opened board '" + _boardId + "'" + (savedSnapshots != null ? " (restored " + savedSnapshots.Length + " snapshot(s))" : ""));
            LogBoardSummary();
        }

        private void OnDisable()
        {
            _tracker.ProgressChanged -= OnProgressChanged;
            _tracker.StepBecameClaimable -= OnStepBecameClaimable;
            _tracker.BoardChanged -= OnBoardChanged;
            _tracker.CloseBoard(_boardId);
        }

        #endregion

        #region Definitions (would normally come from CSV or QuestBoardAsset)

        /// <summary>Fill the reward table and return one definition per sample quest, covering every Accumulation and ClaimPolicy.</summary>
        private QuestDefinition[] BuildDefinitions()
        {
            _rewardLabels.Clear();
            _rewardLabels[(ServeTenCustomersQuestId, 0)] = "15 gold";
            _rewardLabels[(EarnCashSteppedQuestId, 0)] = "1 gem";
            _rewardLabels[(EarnCashSteppedQuestId, 1)] = "3 gems";
            _rewardLabels[(EarnCashSteppedQuestId, 2)] = "10 gems";
            _rewardLabels[(StallOneToLevel25QuestId, 0)] = "40 gold";
            _rewardLabels[(UnlockStallTwoQuestId, 0)] = "1 crate";
            _rewardLabels[(ServeThreeRepeatableQuestId, 0)] = "1 ticket";

            return new[]
            {
                // Counter: any customer served counts.
                new QuestDefinition(ServeTenCustomersQuestId, Objective.AnyParam((int)SampleObjectiveKind.ServeCustomer), Accumulation.Sum, 10),

                // Stepped achievement: progress never resets, three rewards.
                new QuestDefinition(EarnCashSteppedQuestId, Objective.AnyParam((int)SampleObjectiveKind.EarnCash), Accumulation.Sum,
                    new long[] { 100, 1_000, 10_000 }, ClaimPolicy.OncePerStep, 1),

                // High-water: report the stall's level, quest completes when it reaches 25.
                new QuestDefinition(StallOneToLevel25QuestId, new Objective((int)SampleObjectiveKind.UpgradeStall, 1), Accumulation.HighWater, 25),

                // Flag: unlocking stall 2 once is enough.
                new QuestDefinition(UnlockStallTwoQuestId, new Objective((int)SampleObjectiveKind.UnlockStall, 2), Accumulation.Flag, 1),

                // Repeatable daily-style: claim, reset, up to 3 times.
                new QuestDefinition(ServeThreeRepeatableQuestId, Objective.AnyParam((int)SampleObjectiveKind.ServeCustomer), Accumulation.Sum,
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

        /// <summary>Try to claim every Claimable Step on the board and log the reward granted for each success.</summary>
        [ContextMenu("Claim all claimable steps")]
        public void ClaimAll()
        {
            if (!_tracker.TryGetBoard(_boardId, _questViewsBuffer))
                return;

            for (var questIndex = 0; questIndex < _questViewsBuffer.Count; questIndex++)
            {
                var questView = _questViewsBuffer[questIndex];
                for (var stepIndex = 0; stepIndex < questView.StepCount; stepIndex++)
                {
                    if (questView.GetStepState(stepIndex) != StepState.Claimable)
                        continue;

                    // TryClaim returning true is the signal to grant. Rewards are game data.
                    if (_tracker.TryClaim(questView.Id, stepIndex))
                        Debug.Log("[Quests sample] Claimed " + questView.Id + " step " + stepIndex + " -> granted " + _rewardLabels[(questView.Id.LocalId, stepIndex)]);
                }
            }

            LogBoardSummary();
        }

        /// <summary>Delete the temp save, then toggle the component so OnDisable/OnEnable reopen a fresh board.</summary>
        [ContextMenu("Reset temp save and reopen")]
        public void ResetTempSave()
        {
            TempJsonProgressStore.Delete(_boardId);
            enabled = false;
            enabled = true;
        }

        #endregion

        #region Tracker events

        private void OnProgressChanged(QuestId questId)
        {
            if (_tracker.TryGetQuest(questId, out var questView))
                Debug.Log("[Quests sample] " + questId + " = " + questView.ProgressValue + " (" + questView.State + ")");
        }

        private void OnStepBecameClaimable(QuestId questId, int stepIndex) =>
            Debug.Log("[Quests sample] " + questId + " step " + stepIndex + " is claimable");

        // Durable progress changed: export and hand to the (temporary) store.
        private void OnBoardChanged(BoardId boardId)
        {
            if (_persistWithTempJson)
                TempJsonProgressStore.Save(boardId, _tracker.ExportBoard(boardId));
        }

        #endregion

        #region Logging

        /// <summary>One line per quest: progress / current target, state, completed cycles.</summary>
        private void LogBoardSummary()
        {
            if (!_tracker.TryGetBoard(_boardId, _questViewsBuffer))
                return;

            for (var questIndex = 0; questIndex < _questViewsBuffer.Count; questIndex++)
            {
                var questView = _questViewsBuffer[questIndex];
                var targetStepIndex = questView.FirstUnclaimedStepIndex;
                // "-" once every Step is claimed and there is no next target.
                var targetLabel = targetStepIndex >= 0 ? questView.GetStepThreshold(targetStepIndex).ToString() : "-";
                Debug.Log("[Quests sample]   quest " + questView.Id.LocalId + ": " + questView.ProgressValue + " / " + targetLabel
                          + "  " + questView.State + "  cycles=" + questView.CompletedCycles);
            }
        }

        #endregion
    }
}
