using System;
using System.Collections.Generic;

namespace Aerisyn.Tutorial
{
    /// <summary>
    /// An authored Tutorial: ordered Soft/Hard Steps consumed by the Runner.
    /// Immutable plain data so later SO authoring can project into the same shape.
    /// </summary>
    public sealed class TutorialDefinition
    {
        #region Properties

        private readonly StepDefinition[] _steps;

        /// <summary>Stable Tutorial identity used in Completions and Progress Snapshots.</summary>
        public TutorialId Id { get; }

        /// <summary>Ordered Steps. Index matches Completion / Progress Snapshot StepIndex.</summary>
        public IReadOnlyList<StepDefinition> Steps => _steps;

        /// <summary>Number of Steps (at least 1).</summary>
        public int StepCount => _steps.Length;

        #endregion


        #region Construction

        /// <summary>
        /// Validates: id present, at least one Step, no null Steps.
        /// Copies the list so callers cannot mutate the definition afterwards.
        /// </summary>
        public TutorialDefinition(TutorialId id, IReadOnlyList<StepDefinition> steps)
        {
            if (!id.IsValid)
                throw new ArgumentException("TutorialId is not valid.", nameof(id));
            if (steps == null || steps.Count == 0)
                throw new ArgumentException("A Tutorial needs at least one Step.", nameof(steps));

            var copy = new StepDefinition[steps.Count];
            for (var i = 0; i < steps.Count; i++)
            {
                if (steps[i] == null)
                    throw new ArgumentException("Step at index " + i + " is null.", nameof(steps));
                copy[i] = steps[i];
            }

            Id = id;
            _steps = copy;
        }

        #endregion


        public override string ToString() => "Tutorial " + Id + " x" + StepCount;
    }
}
