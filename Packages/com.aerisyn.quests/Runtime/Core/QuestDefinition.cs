using System;
using System.Collections.Generic;

namespace Aerisyn.Quests
{
    /// <summary>
    /// An authored Quest: what to listen for, how progress accumulates, the ordered Step thresholds,
    /// and how claiming works. Immutable and validated on construction. Reward contents are not
    /// part of the definition; games look them up by (QuestId, step).
    /// </summary>
    public sealed class QuestDefinition
    {
        /// <summary>Steps are tracked in a 64-bit claimed mask.</summary>
        public const int MaxSteps = 64;

        #region Fields

        private readonly long[] _thresholds;

        /// <summary>Unique within the owning Board.</summary>
        public int LocalId { get; }

        public Objective Objective { get; }

        public Accumulation Accumulation { get; }

        public ClaimPolicy ClaimPolicy { get; }

        /// <summary>How many full claim cycles are allowed. Always 1 for <see cref="ClaimPolicy.OncePerStep"/>.</summary>
        public int RepeatLimit { get; }

        /// <summary>Ascending, strictly positive thresholds; one per Step.</summary>
        public IReadOnlyList<long> Thresholds => _thresholds;

        public int StepCount => _thresholds.Length;

        #endregion

        #region Construction

        /// <summary>Single-step, claim-once Quest. The common case.</summary>
        public QuestDefinition(int localId, Objective objective, Accumulation accumulation, long threshold)
            : this(localId, objective, accumulation, new[] { threshold }, ClaimPolicy.OncePerStep, 1)
        {
        }

        public QuestDefinition(
            int localId,
            Objective objective,
            Accumulation accumulation,
            IReadOnlyList<long> thresholds,
            ClaimPolicy claimPolicy,
            int repeatLimit)
        {
            if (thresholds == null || thresholds.Count == 0)
                throw new ArgumentException("A Quest needs at least one Step threshold.", nameof(thresholds));
            if (thresholds.Count > MaxSteps)
                throw new ArgumentException("A Quest supports at most " + MaxSteps + " Steps.", nameof(thresholds));

            // Copy so callers cannot mutate the definition afterwards.
            var copy = new long[thresholds.Count];
            for (var i = 0; i < copy.Length; i++)
            {
                var t = thresholds[i];
                if (t <= 0)
                    throw new ArgumentException("Step thresholds must be > 0 (step " + i + " is " + t + ").", nameof(thresholds));
                if (i > 0 && t <= copy[i - 1])
                    throw new ArgumentException("Step thresholds must be strictly ascending (step " + i + ").", nameof(thresholds));
                copy[i] = t;
            }

            // Flag only ever reaches 1, so any other threshold would be unreachable by construction.
            if (accumulation == Accumulation.Flag && (copy.Length != 1 || copy[0] != 1))
                throw new ArgumentException("Flag Quests must have exactly one Step with threshold 1.", nameof(thresholds));

            if (claimPolicy == ClaimPolicy.RepeatWithReset && repeatLimit < 1)
                throw new ArgumentOutOfRangeException(nameof(repeatLimit), "RepeatWithReset needs a repeat limit >= 1.");

            LocalId = localId;
            Objective = objective;
            Accumulation = accumulation;
            ClaimPolicy = claimPolicy;
            RepeatLimit = claimPolicy == ClaimPolicy.RepeatWithReset ? repeatLimit : 1;
            _thresholds = copy;
        }

        #endregion

        /// <summary>Bit mask with one bit set per Step; used to clamp restored snapshots.</summary>
        internal ulong AllStepsMask => StepCount == MaxSteps ? ulong.MaxValue : (1UL << StepCount) - 1UL;

        public override string ToString() => "Quest " + LocalId + " " + Objective + " x" + StepCount;
    }
}
