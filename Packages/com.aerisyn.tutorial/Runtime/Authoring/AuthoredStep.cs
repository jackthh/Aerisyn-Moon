using System;

namespace Aerisyn.Tutorial.Authoring
{
    /// <summary>
    /// Serializable Step for Inspector / SO authoring.
    /// Projects 1:1 into a Core <see cref="StepDefinition"/> (Enforcement, Report match, Choreography).
    /// List-shaped data only: no per-beat C# subclass required.
    /// </summary>
    [Serializable]
    public sealed class AuthoredStep
    {
        #region Fields

        /// <summary>Stable Step identity for analytics / Completion signals.</summary>
        public string Id = "";

        /// <summary>Soft (coach) or Hard (gate).</summary>
        public Enforcement Enforcement = Enforcement.Soft;

        /// <summary>Game-owned Report kind (cast your own enum to int).</summary>
        public int ReportKind;

        /// <summary>Report param when <see cref="MatchAnyParam"/> is false.</summary>
        public int ReportParam;

        /// <summary>When true, any param of <see cref="ReportKind"/> completes the Step.</summary>
        public bool MatchAnyParam;

        /// <summary>
        /// Ordered Cue groups (Sequential / Concurrent) for this Step's Choreography.
        /// Null or empty array → <see cref="ChoreographyDefinition.Empty"/>.
        /// Non-empty slots must be valid (blank Cue ids throw like Core).
        /// </summary>
        public AuthoredCueGroup[] CueGroups = Array.Empty<AuthoredCueGroup>();

        #endregion


        #region Projection

        /// <summary>Builds the Report match from kind / param / match-any fields.</summary>
        public ReportMatch ToReportMatch() =>
            MatchAnyParam ? ReportMatch.AnyParam(ReportKind) : new ReportMatch(ReportKind, ReportParam);


        /// <summary>
        /// Builds Core Choreography from authored Cue groups via public Sequential / Concurrent / Mix factories.
        /// Null array or zero-length → Empty. Null slots or blank Cue ids throw (1:1 with Core validation).
        /// </summary>
        public ChoreographyDefinition ToChoreography()
        {
            if (CueGroups == null || CueGroups.Length == 0)
                return ChoreographyDefinition.Empty;

            var parts = new ChoreographyDefinition[CueGroups.Length];
            for (var i = 0; i < CueGroups.Length; i++)
            {
                if (CueGroups[i] == null)
                    throw new ArgumentException("Authored Cue group at index " + i + " is null.");

                parts[i] = CueGroups[i].ToChoreography();
            }

            return CueGroups.Length == 1 ? parts[0] : ChoreographyDefinition.Mix(parts);
        }


        /// <summary>Projects this authored Step into an immutable Core <see cref="StepDefinition"/>.</summary>
        public StepDefinition ToStepDefinition() =>
            new StepDefinition(Id, Enforcement, ToReportMatch(), ToChoreography());

        #endregion
    }
}
