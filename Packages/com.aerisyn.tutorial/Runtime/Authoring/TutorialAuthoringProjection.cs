using System;
using System.Collections.Generic;

namespace Aerisyn.Tutorial.Authoring
{
    /// <summary>
    /// One-way Authoring → Core projection: ordered authored Steps become a <see cref="TutorialDefinition"/>.
    /// Used by <see cref="TutorialAsset"/> and by fixtures that cannot CreateInstance a ScriptableObject.
    /// </summary>
    public static class TutorialAuthoringProjection
    {
        #region Public API

        /// <summary>
        /// Projects <paramref name="tutorialId"/> + <paramref name="steps"/> into an immutable Core definition.
        /// Throws when the id is blank, steps are null/empty, a slot is null, or a Step fails Core validation.
        /// </summary>
        public static TutorialDefinition Project(string tutorialId, IReadOnlyList<AuthoredStep> steps)
        {
            if (string.IsNullOrEmpty(tutorialId))
                throw new ArgumentException("Tutorial id must be a non-empty string.", nameof(tutorialId));
            if (steps == null || steps.Count == 0)
                throw new ArgumentException("A Tutorial needs at least one authored Step.", nameof(steps));

            var definitions = new StepDefinition[steps.Count];
            for (var i = 0; i < steps.Count; i++)
            {
                if (steps[i] == null)
                    throw new ArgumentException("Authored Step at index " + i + " is null.", nameof(steps));

                definitions[i] = steps[i].ToStepDefinition();
            }

            return new TutorialDefinition(new TutorialId(tutorialId), definitions);
        }

        #endregion
    }
}
