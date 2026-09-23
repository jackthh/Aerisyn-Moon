using System;

namespace Aerisyn.Quests
{
    /// <summary>
    /// The durable shape of one Quest's progress. The game (or a later save package) stores and
    /// restores these; this package never touches disk. Everything else about a Quest (its state,
    /// which Steps are claimable) is derived from the definition plus these four numbers.
    /// </summary>
    /// <remarks>
    /// Field names are part of the save format (JsonUtility and similar serializers key on them),
    /// so they are kept as-is even where <see cref="QuestView"/> uses clearer names.
    /// </remarks>
    [Serializable]
    public struct ProgressSnapshot
    {
        #region Fields

        /// <summary>Which Quest this is, by <see cref="QuestDefinition.LocalId"/> within its Board.</summary>
        public int LocalId;

        /// <summary>
        /// Accumulated progress value in the current cycle (count for Sum, best value for HighWater, 0/1 for Flag).
        /// Same number as <see cref="QuestView.ProgressValue"/>.
        /// </summary>
        public long Value;

        /// <summary>Bit i set means Step i is claimed in the current cycle.</summary>
        public ulong ClaimedStepsMask;

        /// <summary>
        /// Number of fully completed claim cycles (every Step claimed), not the number of individual Step claims.
        /// Only grows past 1 under <see cref="ClaimPolicy.RepeatWithReset"/>.
        /// Same number as <see cref="QuestView.CompletedCycles"/>.
        /// </summary>
        public int ClaimCount;

        #endregion

        public ProgressSnapshot(int localId, long progressValue, ulong claimedStepsMask, int completedCycles)
        {
            LocalId = localId;
            Value = progressValue;
            ClaimedStepsMask = claimedStepsMask;
            ClaimCount = completedCycles;
        }
    }
}
