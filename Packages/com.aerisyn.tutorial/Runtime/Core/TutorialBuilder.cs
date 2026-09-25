using System;
using System.Collections.Generic;

namespace Aerisyn.Tutorial
{
    /// <summary>
    /// Code-first builder for Soft-only Tutorial definitions (ticket 01).
    /// Later tickets can extend Hard / Cue authoring without changing the definition shape.
    /// </summary>
    public sealed class TutorialBuilder
    {
        #region Fields

        private readonly TutorialId _id;
        private readonly List<StepDefinition> _steps = new List<StepDefinition>();

        #endregion


        #region Construction

        /// <summary>Starts a builder for Tutorial <paramref name="tutorialId"/>.</summary>
        public TutorialBuilder(string tutorialId)
        {
            _id = new TutorialId(tutorialId);
        }

        #endregion


        #region Soft Steps

        /// <summary>Appends a Soft Step that succeeds when <paramref name="reportMatch"/> is Reported.</summary>
        public TutorialBuilder SoftStep(string stepId, ReportMatch reportMatch)
        {
            _steps.Add(new StepDefinition(stepId, Enforcement.Soft, reportMatch));
            return this;
        }

        #endregion


        #region Build

        /// <summary>Produces an immutable <see cref="TutorialDefinition"/>. Throws if no Steps were added.</summary>
        public TutorialDefinition Build() => new TutorialDefinition(_id, _steps);

        #endregion
    }
}
