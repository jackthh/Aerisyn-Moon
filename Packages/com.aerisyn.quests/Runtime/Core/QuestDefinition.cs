using System;
using System.Collections.Generic;

namespace Aerisyn.Quests
{
    /// <summary>
    /// An authored Quest: what to listen for, how progress accumulates, the ordered Step thresholds,
    /// and how claiming works. Immutable and validated on construction. Reward contents are not
    /// part of the definition; games look them up by (QuestId, step index).
    /// </summary>
    public sealed class QuestDefinition
    {
        /// <summary>Upper bound on Steps per Quest, because claimed Steps are stored as bits in a 64-bit mask.</summary>
        public const int MaxSteps = 64;

        #region Properties

        private readonly long[] _stepThresholds;

        /// <summary>Id of this Quest inside its Board. Must be unique within the Board, may repeat across Boards.</summary>
        public int LocalId { get; }

        /// <summary>The gameplay fact this Quest listens for.</summary>
        public Objective Objective { get; }

        /// <summary>How each matching report changes the progress value (add, keep max, or set once).</summary>
        public Accumulation Accumulation { get; }

        /// <summary>What happens once Steps are reached: claim each once, or claim all then reset and repeat.</summary>
        public ClaimPolicy ClaimPolicy { get; }

        /// <summary>
        /// Maximum number of full claim cycles (all Steps claimed) before the Quest is Completed.
        /// Always 1 for <see cref="ClaimPolicy.OncePerStep"/>.
        /// </summary>
        public int RepeatLimit { get; }

        /// <summary>Progress value needed to reach each Step, indexed by step. Strictly ascending and &gt; 0.</summary>
        public IReadOnlyList<long> Thresholds => _stepThresholds;

        /// <summary>Number of claimable Steps (1..<see cref="MaxSteps"/>).</summary>
        public int StepCount => _stepThresholds.Length;

        #endregion

        #region Construction

        /// <summary>Single-step, claim-once Quest. The common case ("serve 10 customers, claim one reward").</summary>
        public QuestDefinition(int localId, Objective objective, Accumulation accumulation, long threshold)
            : this(localId, objective, accumulation, new[] { threshold }, ClaimPolicy.OncePerStep, 1)
        {
        }

        /// <summary>
        /// Full constructor. Validates, in order:
        /// 1) 1..<see cref="MaxSteps"/> thresholds, each &gt; 0 and strictly ascending;
        /// 2) Flag Quests have exactly one threshold of 1;
        /// 3) RepeatWithReset has <paramref name="repeatLimit"/> &gt;= 1.
        /// Throws <see cref="ArgumentException"/> on the first violation.
        /// </summary>
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

            // Validate while copying, so callers cannot mutate the definition afterwards.
            var validatedThresholds = new long[thresholds.Count];
            for (var stepIndex = 0; stepIndex < validatedThresholds.Length; stepIndex++)
            {
                var threshold = thresholds[stepIndex];
                if (threshold <= 0)
                    throw new ArgumentException("Step thresholds must be > 0 (step " + stepIndex + " is " + threshold + ").", nameof(thresholds));
                if (stepIndex > 0 && threshold <= validatedThresholds[stepIndex - 1])
                    throw new ArgumentException("Step thresholds must be strictly ascending (step " + stepIndex + ").", nameof(thresholds));
                validatedThresholds[stepIndex] = threshold;
            }

            // Flag only ever reaches 1, so any other threshold would be unreachable by construction.
            if (accumulation == Accumulation.Flag && (validatedThresholds.Length != 1 || validatedThresholds[0] != 1))
                throw new ArgumentException("Flag Quests must have exactly one Step with threshold 1.", nameof(thresholds));

            if (claimPolicy == ClaimPolicy.RepeatWithReset && repeatLimit < 1)
                throw new ArgumentOutOfRangeException(nameof(repeatLimit), "RepeatWithReset needs a repeat limit >= 1.");

            LocalId = localId;
            Objective = objective;
            Accumulation = accumulation;
            ClaimPolicy = claimPolicy;
            // OncePerStep has exactly one cycle by definition; ignore whatever the caller passed.
            RepeatLimit = claimPolicy == ClaimPolicy.RepeatWithReset ? repeatLimit : 1;
            _stepThresholds = validatedThresholds;
        }

        #endregion

        /// <summary>Mask with one bit set per Step (bits 0..StepCount-1). Used to clamp restored snapshots.</summary>
        /// <remarks>C# masks the shift count to 6 bits, so <c>1UL &lt;&lt; 64</c> is 1, not 0; the 64-Step case is special-cased.</remarks>
        internal ulong AllStepsMask => StepCount == MaxSteps ? ulong.MaxValue : (1UL << StepCount) - 1UL;

        /// <summary>Debug form: <c>Quest localId kind(param) xStepCount</c>.</summary>
        public override string ToString() => "Quest " + LocalId + " " + Objective + " x" + StepCount;
    }
}
