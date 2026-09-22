namespace Aerisyn.Quests
{
    /// <summary>
    /// Read-only picture of one Quest at a point in time: definition plus progress plus derived states.
    /// Cheap to copy; take a fresh one after each event instead of caching.
    /// </summary>
    public readonly struct QuestView
    {
        public readonly QuestId Id;
        public readonly QuestDefinition Definition;
        public readonly long Value;
        public readonly ulong ClaimedStepsMask;
        public readonly int ClaimCount;

        internal QuestView(QuestId id, QuestDefinition definition, long value, ulong claimedStepsMask, int claimCount)
        {
            Id = id;
            Definition = definition;
            Value = value;
            ClaimedStepsMask = claimedStepsMask;
            ClaimCount = claimCount;
        }

        public int StepCount => Definition.StepCount;

        public QuestState State => QuestRules.GetQuestState(Definition, Value, ClaimedStepsMask, ClaimCount);

        public StepState GetStepState(int step) =>
            QuestRules.GetStepState(Definition, Value, ClaimedStepsMask, ClaimCount, step);

        public long GetThreshold(int step) => Definition.Thresholds[step];

        /// <summary>
        /// Index of the first Step that is not yet claimed, or -1 when every Step is claimed.
        /// Handy for "current target" UI (show 7 / 10, then 10 / 50).
        /// </summary>
        public int CurrentStep
        {
            get
            {
                for (var i = 0; i < StepCount; i++)
                {
                    if (!QuestRules.IsStepClaimed(ClaimedStepsMask, i))
                        return i;
                }

                return -1;
            }
        }

        public ProgressSnapshot ToSnapshot() => new ProgressSnapshot(Id.LocalId, Value, ClaimedStepsMask, ClaimCount);
    }
}
