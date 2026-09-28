using System;

namespace Aerisyn.Tutorial
{
    /// <summary>
    /// The gameplay fact a Soft (or Hard) Step listens for to succeed.
    /// Kind/Param are opaque ints the game owns (cast your own enum); the package never interprets them.
    /// </summary>
    public readonly struct ReportMatch : IEquatable<ReportMatch>
    {
        #region Fields

        /// <summary>Game-defined verb, e.g. <c>(int)MyKind.UpgradeSword</c>.</summary>
        public readonly int Kind;

        /// <summary>
        /// Game-defined target that narrows <see cref="Kind"/>.
        /// Meaningless when <see cref="MatchAnyParam"/> is true.
        /// </summary>
        public readonly int Param;

        /// <summary>True when reports of <see cref="Kind"/> match regardless of their param.</summary>
        public readonly bool MatchAnyParam;

        #endregion


        #region Construction

        /// <summary>Matches only reports with exactly this kind and this param.</summary>
        public ReportMatch(int kind, int param)
        {
            Kind = kind;
            Param = param;
            MatchAnyParam = false;
        }


        private ReportMatch(int kind, int param, bool matchAnyParam)
        {
            Kind = kind;
            Param = param;
            MatchAnyParam = matchAnyParam;
        }


        /// <summary>Matches every report of <paramref name="kind"/>, whatever its param.</summary>
        public static ReportMatch AnyParam(int kind) => new ReportMatch(kind, 0, true);

        #endregion


        #region Matching

        /// <summary>
        /// True when a Report of (<paramref name="reportedKind"/>, <paramref name="reportedParam"/>)
        /// should complete a Step with this match.
        /// </summary>
        public bool Matches(int reportedKind, int reportedParam) =>
            Kind == reportedKind && (MatchAnyParam || Param == reportedParam);

        #endregion


        #region Equality

        public bool Equals(ReportMatch other) =>
            Kind == other.Kind && Param == other.Param && MatchAnyParam == other.MatchAnyParam;


        public override bool Equals(object obj) => obj is ReportMatch other && Equals(other);


        public override int GetHashCode()
        {
            unchecked
            {
                var hash = Kind;
                hash = (hash * 397) ^ Param;
                hash = (hash * 397) ^ (MatchAnyParam ? 1 : 0);
                return hash;
            }
        }

        #endregion


        /// <summary>Debug form: <c>kind(param)</c>, or <c>kind(*)</c> when any param matches.</summary>
        public override string ToString() => MatchAnyParam ? Kind + "(*)" : Kind + "(" + Param + ")";
    }
}
