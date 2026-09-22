namespace Aerisyn.Quests
{
    /// <summary>Live, mutable progress for one Quest. Owned by the Tracker; exposed read-only via <see cref="QuestView"/>.</summary>
    internal sealed class QuestProgress
    {
        public long Value;
        public ulong ClaimedStepsMask;
        public int ClaimCount;

        public bool IsStepClaimed(int step) => (ClaimedStepsMask & (1UL << step)) != 0;

        public void MarkStepClaimed(int step) => ClaimedStepsMask |= 1UL << step;

        /// <summary>Start a new claim cycle (RepeatWithReset).</summary>
        public void ResetCycle()
        {
            Value = 0;
            ClaimedStepsMask = 0;
        }

        public ProgressSnapshot ToSnapshot(int localId) => new ProgressSnapshot(localId, Value, ClaimedStepsMask, ClaimCount);

        /// <summary>Restore from a snapshot, clamping anything the definition cannot represent.</summary>
        public void Restore(in ProgressSnapshot snapshot, QuestDefinition definition)
        {
            Value = snapshot.Value < 0 ? 0 : snapshot.Value;
            ClaimedStepsMask = snapshot.ClaimedStepsMask & definition.AllStepsMask;
            ClaimCount = snapshot.ClaimCount < 0 ? 0 : snapshot.ClaimCount;
        }
    }
}
