using System;

namespace Aerisyn.Quests
{
    /// <summary>
    /// Read-only picture of one Quest at a point in time: definition plus progress plus derived states.
    /// Cheap to copy; take a fresh one after each event instead of caching.
    /// </summary>
    public readonly struct QuestView
    {
        #region Fields

        /// <summary>Board + local id of the viewed Quest.</summary>
        public readonly QuestId Id;

        /// <summary>The authored rules (Objective, Accumulation, thresholds, ClaimPolicy).</summary>
        public readonly QuestDefinition Definition;

        /// <summary>Accumulated value in the current cycle; compare against <see cref="GetStepThreshold"/>.</summary>
        public readonly long ProgressValue;

        /// <summary>Bit i set means Step i is claimed in the current cycle. Prefer <see cref="GetStepState"/> for UI.</summary>
        public readonly ulong ClaimedStepsMask;

        /// <summary>
        /// Full claim cycles finished (every Step claimed). Not the number of individual Step claims.
        /// Under RepeatWithReset the Quest is Completed once this reaches <see cref="QuestDefinition.RepeatLimit"/>.
        /// </summary>
        public readonly int CompletedCycles;

        #endregion


        internal QuestView(QuestId id, QuestDefinition definition, long progressValue, ulong claimedStepsMask,
            int completedCycles)
        {
            Id = id;
            Definition = definition;
            ProgressValue = progressValue;
            ClaimedStepsMask = claimedStepsMask;
            CompletedCycles = completedCycles;
        }


        #region Derived state

        public int StepCount => Definition.StepCount;

        /// <summary>Summary of the whole Quest: InProgress, Claimable, or Completed.</summary>
        public QuestState State =>
            QuestRules.GetQuestState(Definition, ProgressValue, ClaimedStepsMask, CompletedCycles);


        /// <summary>Locked, Claimable, or Claimed for the Step at <paramref name="stepIndex"/> (0-based).</summary>
        public StepState GetStepState(int stepIndex) =>
            QuestRules.GetStepState(Definition, ProgressValue, ClaimedStepsMask, CompletedCycles, stepIndex);


        /// <summary>Progress value needed to reach the Step at <paramref name="stepIndex"/> (0-based).</summary>
        public long GetStepThreshold(int stepIndex) => Definition.Thresholds[stepIndex];


        /// <summary>
        /// Index of the first Step that is not yet claimed, or -1 when every Step is claimed.
        /// Handy for "current target" UI (show 7 / 10, then 10 / 50).
        /// </summary>
        public int FirstUnclaimedStepIndex
        {
            get
            {
                for (var stepIndex = 0; stepIndex < StepCount; stepIndex++)
                {
                    if (!QuestRules.IsStepClaimed(ClaimedStepsMask, stepIndex))
                        return stepIndex;
                }

                return -1;
            }
        }

        #endregion


        public ProgressSnapshot ToSnapshot() => new(Id.LocalId, ProgressValue, ClaimedStepsMask, CompletedCycles);
    }
}