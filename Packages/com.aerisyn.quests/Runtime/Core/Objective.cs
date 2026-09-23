using System;

namespace Aerisyn.Quests
{
    /// <summary>
    /// The gameplay fact a Quest listens for, e.g. "a customer was served at stall 2".
    /// <list type="bullet">
    /// <item><see cref="Kind"/>: what happened. An opaque int the game defines (cast your own enum).</item>
    /// <item><see cref="Param"/>: which thing it happened to (a stall id, a currency id, ...).</item>
    /// <item><see cref="MatchAnyParam"/>: when true, <see cref="Param"/> is ignored and every report of the kind matches.</item>
    /// </list>
    /// </summary>
    public readonly struct Objective : IEquatable<Objective>
    {
        #region Fields

        /// <summary>Game-defined verb, e.g. <c>(int)MyKind.ServeCustomer</c>. The package never interprets it.</summary>
        public readonly int Kind;

        /// <summary>
        /// Game-defined target that narrows <see cref="Kind"/> (stall 2, currency "gems", ...).
        /// Meaningless when <see cref="MatchAnyParam"/> is true.
        /// </summary>
        public readonly int Param;

        /// <summary>True when reports of <see cref="Kind"/> match regardless of their param.</summary>
        public readonly bool MatchAnyParam;

        #endregion

        #region Construction

        /// <summary>Matches only reports with exactly this kind and this param.</summary>
        public Objective(int kind, int param)
        {
            Kind = kind;
            Param = param;
            MatchAnyParam = false;
        }

        private Objective(int kind, int param, bool matchAnyParam)
        {
            Kind = kind;
            Param = param;
            MatchAnyParam = matchAnyParam;
        }

        /// <summary>Matches every report of <paramref name="kind"/>, whatever its param ("serve a customer at any stall").</summary>
        /// <remarks>Param is stored as 0 but never compared, so 0 stays a valid real param for exact Objectives.</remarks>
        public static Objective AnyParam(int kind) => new Objective(kind, 0, true);

        #endregion

        #region Matching

        /// <summary>True when a report of (<paramref name="reportedKind"/>, <paramref name="reportedParam"/>) should update a Quest with this Objective.</summary>
        public bool Matches(int reportedKind, int reportedParam) =>
            Kind == reportedKind && (MatchAnyParam || Param == reportedParam);

        #endregion

        #region Equality

        public bool Equals(Objective other) =>
            Kind == other.Kind && Param == other.Param && MatchAnyParam == other.MatchAnyParam;

        public override bool Equals(object obj) => obj is Objective other && Equals(other);

        public override int GetHashCode()
        {
            // Standard prime-multiply combine; overflow is expected and harmless.
            unchecked
            {
                var hash = Kind;
                hash = (hash * 397) ^ Param;
                hash = (hash * 397) ^ (MatchAnyParam ? 1 : 0);
                return hash;
            }
        }

        #endregion

        #region Obsolete aliases (removed in 0.2.0)

        [Obsolete("Renamed to Objective.AnyParam for clarity. This alias is removed in 0.2.0.")]
        public static Objective Any(int kind) => AnyParam(kind);

        #endregion

        /// <summary>Debug form: <c>kind(param)</c>, or <c>kind(*)</c> when any param matches.</summary>
        public override string ToString() => MatchAnyParam ? Kind + "(*)" : Kind + "(" + Param + ")";
    }
}
