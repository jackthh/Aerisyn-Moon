namespace Aerisyn.Quests
{
    /// <summary>
    /// Pure functions that derive Quest/Step state from a definition and progress numbers.
    /// Centralised so the Tracker, views, and snapshots never disagree.
    /// </summary>
    internal static class QuestRules
    {
        public static bool IsStepReached(QuestDefinition def, long value, int step) => value >= def.Thresholds[step];

        public static bool IsStepClaimed(ulong claimedMask, int step) => (claimedMask & (1UL << step)) != 0;

        public static bool AreAllStepsClaimed(QuestDefinition def, ulong claimedMask) =>
            (claimedMask & def.AllStepsMask) == def.AllStepsMask;

        /// <summary>Nothing left to earn on this Quest.</summary>
        public static bool IsCompleted(QuestDefinition def, ulong claimedMask, int claimCount)
        {
            if (def.ClaimPolicy == ClaimPolicy.RepeatWithReset)
                return claimCount >= def.RepeatLimit;

            return AreAllStepsClaimed(def, claimedMask);
        }

        public static StepState GetStepState(QuestDefinition def, long value, ulong claimedMask, int claimCount, int step)
        {
            if (IsStepClaimed(claimedMask, step))
                return StepState.Claimed;

            if (IsCompleted(def, claimedMask, claimCount))
                return StepState.Locked;

            return IsStepReached(def, value, step) ? StepState.Claimable : StepState.Locked;
        }

        public static QuestState GetQuestState(QuestDefinition def, long value, ulong claimedMask, int claimCount)
        {
            if (IsCompleted(def, claimedMask, claimCount))
                return QuestState.Completed;

            for (var i = 0; i < def.StepCount; i++)
            {
                if (!IsStepClaimed(claimedMask, i) && IsStepReached(def, value, i))
                    return QuestState.Claimable;
            }

            return QuestState.InProgress;
        }

        /// <summary>
        /// Progress only moves while some unclaimed Step is still ahead of the current value.
        /// A single-step Quest therefore freezes at its threshold until claimed; a stepped Quest keeps
        /// counting toward its later Steps.
        /// </summary>
        public static bool CanAccumulate(QuestDefinition def, long value, ulong claimedMask, int claimCount)
        {
            if (IsCompleted(def, claimedMask, claimCount))
                return false;

            for (var i = 0; i < def.StepCount; i++)
            {
                if (!IsStepClaimed(claimedMask, i) && !IsStepReached(def, value, i))
                    return true;
            }

            return false;
        }

        /// <summary>Apply one report to a value under the definition's accumulation mode.</summary>
        public static long Accumulate(Accumulation mode, long current, long reported)
        {
            switch (mode)
            {
                case Accumulation.HighWater:
                    return reported > current ? reported : current;
                case Accumulation.Flag:
                    return 1;
                default:
                    // Sum: allow negative reports (refunds) but never go below zero.
                    var next = current + reported;
                    return next < 0 ? 0 : next;
            }
        }
    }
}
