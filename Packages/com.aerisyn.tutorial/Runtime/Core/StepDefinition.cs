using System;

namespace Aerisyn.Tutorial
{
    /// <summary>
    /// One authored beat in a Tutorial: Enforcement, Report match for success, and a stable Step id.
    /// Immutable. Choreography / Cues are out of ticket 01.
    /// </summary>
    public sealed class StepDefinition
    {
        #region Properties

        /// <summary>Stable Step identity for analytics / UI. Unique within its Tutorial is recommended.</summary>
        public string Id { get; }

        /// <summary>Soft (coach) or Hard (gate). Ticket 01 builders only produce Soft.</summary>
        public Enforcement Enforcement { get; }

        /// <summary>Which Report completes this Step.</summary>
        public ReportMatch ReportMatch { get; }

        #endregion


        #region Construction

        /// <summary>Builds one Step. Throws when <paramref name="id"/> is null or empty.</summary>
        public StepDefinition(string id, Enforcement enforcement, ReportMatch reportMatch)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentException("Step id must be a non-empty string.", nameof(id));

            Id = id;
            Enforcement = enforcement;
            ReportMatch = reportMatch;
        }

        #endregion


        public override string ToString() => "Step " + Id + " " + Enforcement + " " + ReportMatch;
    }
}
