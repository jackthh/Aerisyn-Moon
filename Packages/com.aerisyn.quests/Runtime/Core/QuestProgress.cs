namespace Aerisyn.Quests
{
    /// <summary>Live, mutable progress for one Quest. Owned by the Tracker; exposed read-only via <see cref="QuestView"/>.</summary>
    internal sealed class QuestProgress
    {
        #region State

        /// <summary>Accumulated value in the current cycle. Stored as <see cref="ProgressSnapshot.Value"/>.</summary>
        public long ProgressValue;

        /// <summary>Bit i set means Step i is claimed in the current cycle.</summary>
        public ulong ClaimedStepsMask;

        /// <summary>Full cycles finished (every Step claimed). Stored as <see cref="ProgressSnapshot.ClaimCount"/>.</summary>
        public int CompletedCycles;

        #endregion

        #region Mutation

        public bool IsStepClaimed(int stepIndex) => QuestRules.IsStepClaimed(ClaimedStepsMask, stepIndex);

        public void MarkStepClaimed(int stepIndex) => ClaimedStepsMask |= 1UL << stepIndex;

        /// <summary>Start a new claim cycle (RepeatWithReset). <see cref="CompletedCycles"/> is kept.</summary>
        public void ResetCycle()
        {
            ProgressValue = 0;
            ClaimedStepsMask = 0;
        }

        #endregion

        #region Snapshots

        public ProgressSnapshot ToSnapshot(int localId) =>
            new ProgressSnapshot(localId, ProgressValue, ClaimedStepsMask, CompletedCycles);

        /// <summary>
        /// Restore from a snapshot, clamping anything the definition cannot represent:
        /// negative numbers become 0, and claimed bits beyond the Quest's Step count are dropped
        /// (e.g. a save made before a designer removed a Step).
        /// </summary>
        public void Restore(in ProgressSnapshot snapshot, QuestDefinition definition)
        {
            ProgressValue = snapshot.Value < 0 ? 0 : snapshot.Value;
            ClaimedStepsMask = snapshot.ClaimedStepsMask & definition.AllStepsMask;
            CompletedCycles = snapshot.ClaimCount < 0 ? 0 : snapshot.ClaimCount;
        }

        #endregion
    }
}
