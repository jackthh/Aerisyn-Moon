using System;

namespace Aerisyn.Quests
{
    /// <summary>
    /// The durable shape of one Quest's progress. The game (or a later save package) stores and
    /// restores these; this package never touches disk. Everything else about a Quest is derived
    /// from the definition plus these four numbers.
    /// </summary>
    [Serializable]
    public struct ProgressSnapshot
    {
        /// <summary>Quest local id within its Board.</summary>
        public int LocalId;

        /// <summary>Current accumulated value.</summary>
        public long Value;

        /// <summary>Bit i set means Step i is claimed in the current cycle.</summary>
        public ulong ClaimedStepsMask;

        /// <summary>Completed claim cycles (relevant for <see cref="ClaimPolicy.RepeatWithReset"/>).</summary>
        public int ClaimCount;

        public ProgressSnapshot(int localId, long value, ulong claimedStepsMask, int claimCount)
        {
            LocalId = localId;
            Value = value;
            ClaimedStepsMask = claimedStepsMask;
            ClaimCount = claimCount;
        }
    }
}
