using System;

namespace Aerisyn.Tutorial
{
    /// <summary>
    /// One authored beat in a Tutorial: Enforcement, Report match, optional Choreography, stable Step id.
    /// Immutable plain data for the Runner (and later SO projection).
    /// </summary>
    public sealed class StepDefinition
    {
        #region Properties

        /// <summary>Stable Step identity for analytics / UI. Unique within its Tutorial is recommended.</summary>
        public string Id { get; }

        /// <summary>Soft (coach) or Hard (gate).</summary>
        public Enforcement Enforcement { get; }

        /// <summary>Which Report completes this Step.</summary>
        public ReportMatch ReportMatch { get; }

        /// <summary>Optional sequential Cue Choreography. Never null (use <see cref="ChoreographyDefinition.Empty"/>).</summary>
        public ChoreographyDefinition Choreography { get; }

        #endregion


        #region Construction

        /// <summary>
        /// Builds one Step. Throws when <paramref name="id"/> is null or empty.
        /// Null <paramref name="choreography"/> becomes <see cref="ChoreographyDefinition.Empty"/>.
        /// </summary>
        public StepDefinition(
            string id,
            Enforcement enforcement,
            ReportMatch reportMatch,
            ChoreographyDefinition choreography = null)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentException("Step id must be a non-empty string.", nameof(id));

            Id = id;
            Enforcement = enforcement;
            ReportMatch = reportMatch;
            Choreography = choreography ?? ChoreographyDefinition.Empty;
        }

        #endregion


        public override string ToString() => "Step " + Id + " " + Enforcement + " " + ReportMatch;
    }
}
