namespace Aerisyn.Quests
{
    /// <summary>
    /// Pure functions that derive Quest/Step state from a definition and the three progress numbers
    /// (progress value, claimed-steps mask, completed cycles). Centralised so the Tracker, views,
    /// and snapshots never disagree.
    /// </summary>
    internal static class QuestRules
    {
        #region Step checks

        /// <summary>True when the progress value has met this Step's threshold (claimed or not).</summary>
        public static bool IsStepReached(QuestDefinition definition, long progressValue, int stepIndex) =>
            progressValue >= definition.Thresholds[stepIndex];


        public static bool IsStepClaimed(ulong claimedStepsMask, int stepIndex) =>
            (claimedStepsMask & (1UL << stepIndex)) != 0;


        public static bool AreAllStepsClaimed(QuestDefinition definition, ulong claimedStepsMask) =>
            (claimedStepsMask & definition.AllStepsMask) == definition.AllStepsMask;

        #endregion


        #region Derived states

        /// <summary>
        /// Nothing left to earn on this Quest. RepeatWithReset looks at finished cycles (its mask is
        /// cleared on every reset except the last); OncePerStep looks at the claimed mask.
        /// </summary>
        public static bool IsCompleted(QuestDefinition definition, ulong claimedStepsMask, int completedCycles)
        {
            if (definition.ClaimPolicy == ClaimPolicy.RepeatWithReset)
                return completedCycles >= definition.RepeatLimit;

            return AreAllStepsClaimed(definition, claimedStepsMask);
        }


        /// <summary>Claimed wins over everything; otherwise a Step is Claimable only if reached and the Quest is not Completed.</summary>
        public static StepState GetStepState(
            QuestDefinition definition, long progressValue, ulong claimedStepsMask, int completedCycles, int stepIndex)
        {
            if (IsStepClaimed(claimedStepsMask, stepIndex))
                return StepState.Claimed;

            if (IsCompleted(definition, claimedStepsMask, completedCycles))
                return StepState.Locked;

            return IsStepReached(definition, progressValue, stepIndex) ? StepState.Claimable : StepState.Locked;
        }


        /// <summary>Completed if nothing is left to earn; Claimable if any reached Step is unclaimed; otherwise InProgress.</summary>
        public static QuestState GetQuestState(
            QuestDefinition definition, long progressValue, ulong claimedStepsMask, int completedCycles)
        {
            if (IsCompleted(definition, claimedStepsMask, completedCycles))
                return QuestState.Completed;

            for (var stepIndex = 0; stepIndex < definition.StepCount; stepIndex++)
            {
                if (!IsStepClaimed(claimedStepsMask, stepIndex) && IsStepReached(definition, progressValue, stepIndex))
                    return QuestState.Claimable;
            }

            return QuestState.InProgress;
        }

        #endregion


        #region Accumulation

        /// <summary>
        /// Whether a report is allowed to change the progress value right now.
        /// Progress only moves while some unclaimed Step is still ahead of the current value.
        /// A single-step Quest therefore freezes at its threshold until claimed; a stepped Quest keeps
        /// counting toward its later Steps.
        /// </summary>
        public static bool CanAccumulate(
            QuestDefinition definition, long progressValue, ulong claimedStepsMask, int completedCycles)
        {
            if (IsCompleted(definition, claimedStepsMask, completedCycles))
                return false;

            for (var stepIndex = 0; stepIndex < definition.StepCount; stepIndex++)
            {
                if (!IsStepClaimed(claimedStepsMask, stepIndex) && !IsStepReached(definition, progressValue, stepIndex))
                    return true;
            }

            return false;
        }


        /// <summary>Apply one reported value to the current progress value under the given accumulation mode.</summary>
        public static long Accumulate(Accumulation accumulation, long currentValue, long reportedValue)
        {
            switch (accumulation)
            {
                case Accumulation.HighWater:
                    return reportedValue > currentValue ? reportedValue : currentValue;
                case Accumulation.Flag:
                    return 1;
                default:
                    // Sum: allow negative reports (refunds) but never go below zero.
                    var summedValue = currentValue + reportedValue;
                    return summedValue < 0 ? 0 : summedValue;
            }
        }

        #endregion
    }
}
