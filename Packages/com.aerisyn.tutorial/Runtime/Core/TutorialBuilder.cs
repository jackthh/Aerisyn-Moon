using System.Collections.Generic;

namespace Aerisyn.Tutorial
{
    /// <summary>
    /// Code-first builder for Soft and Hard Tutorial definitions with optional Cue Choreography.
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
        public TutorialBuilder SoftStep(
            string stepId,
            ReportMatch reportMatch,
            ChoreographyDefinition choreography = null)
        {
            _steps.Add(new StepDefinition(stepId, Enforcement.Soft, reportMatch, choreography));
            return this;
        }

        #endregion


        #region Hard Steps

        /// <summary>
        /// Appends a Hard Step that succeeds when <paramref name="reportMatch"/> is Reported.
        /// The Runner emits Gate Started/Ended while this Step is active.
        /// </summary>
        public TutorialBuilder HardStep(
            string stepId,
            ReportMatch reportMatch,
            ChoreographyDefinition choreography = null)
        {
            _steps.Add(new StepDefinition(stepId, Enforcement.Hard, reportMatch, choreography));
            return this;
        }

        #endregion


        #region Build

        /// <summary>Produces an immutable <see cref="TutorialDefinition"/>. Throws if no Steps were added.</summary>
        public TutorialDefinition Build() => new TutorialDefinition(_id, _steps);

        #endregion
    }
}
