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
        /// Null or empty → <see cref="ChoreographyDefinition.Empty"/>.
        /// </summary>
        public AuthoredCueGroup[] CueGroups = Array.Empty<AuthoredCueGroup>();

        #endregion


        #region Projection

        /// <summary>Builds the Report match from kind / param / match-any fields.</summary>
        public ReportMatch ToReportMatch() =>
            MatchAnyParam ? ReportMatch.AnyParam(ReportKind) : new ReportMatch(ReportKind, ReportParam);


        /// <summary>
        /// Builds Core Choreography from authored Cue groups via public Sequential / Concurrent / Mix factories.
        /// Null / empty / blank groups are skipped (Empty when none remain).
        /// </summary>
        public ChoreographyDefinition ToChoreography()
        {
            if (CueGroups == null || CueGroups.Length == 0)
                return ChoreographyDefinition.Empty;

            // Count groups that have Cue ids so Mix gets a tight array.
            var count = 0;
            for (var i = 0; i < CueGroups.Length; i++)
            {
                if (CueGroups[i] != null && CueGroups[i].HasCueIds)
                    count++;
            }

            if (count == 0)
                return ChoreographyDefinition.Empty;

            var parts = new ChoreographyDefinition[count];
            var write = 0;
            for (var i = 0; i < CueGroups.Length; i++)
            {
                if (CueGroups[i] == null || !CueGroups[i].HasCueIds)
                    continue;

                parts[write] = ToSingleGroupChoreography(CueGroups[i]);
                write++;
            }

            return count == 1 ? parts[0] : ChoreographyDefinition.Mix(parts);
        }


        /// <summary>Projects this authored Step into an immutable Core <see cref="StepDefinition"/>.</summary>
        public StepDefinition ToStepDefinition() =>
            new StepDefinition(Id, Enforcement, ToReportMatch(), ToChoreography());

        #endregion


        #region Private helpers

        /// <summary>
        /// One authored Cue group → Sequential or Concurrent Choreography via Core public factories
        /// (Authoring never depends on Core private constructors).
        /// </summary>
        static ChoreographyDefinition ToSingleGroupChoreography(AuthoredCueGroup authored)
        {
            CueGroup group = authored.ToCueGroup();
            var ids = new string[group.CueIds.Count];
            for (var i = 0; i < group.CueIds.Count; i++)
                ids[i] = group.CueIds[i];

            return group.Kind == CueGroupKind.Sequential
                ? ChoreographyDefinition.Sequential(ids)
                : ChoreographyDefinition.Concurrent(ids);
        }

        #endregion
    }
}
