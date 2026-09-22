using System;

namespace Aerisyn.Quests
{
    /// <summary>
    /// The fact a Quest listens for. <see cref="Kind"/> is an opaque integer the game defines
    /// (cast your own enum); <see cref="Param"/> narrows it (a stall id, a currency id, ...).
    /// With <see cref="MatchAnyParam"/> the Quest reacts to every report of that kind.
    /// </summary>
    public readonly struct Objective : IEquatable<Objective>
    {
        public readonly int Kind;
        public readonly int Param;
        public readonly bool MatchAnyParam;

        /// <summary>Exact match on kind and param.</summary>
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

        /// <summary>Matches every report of <paramref name="kind"/>, regardless of param.</summary>
        public static Objective Any(int kind) => new Objective(kind, 0, true);

        /// <summary>True when a report of (kind, param) should update a Quest with this Objective.</summary>
        public bool Matches(int kind, int param) => Kind == kind && (MatchAnyParam || Param == param);

        #region Equality

        public bool Equals(Objective other) =>
            Kind == other.Kind && Param == other.Param && MatchAnyParam == other.MatchAnyParam;

        public override bool Equals(object obj) => obj is Objective other && Equals(other);

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

        public override string ToString() => MatchAnyParam ? Kind + "(*)" : Kind + "(" + Param + ")";
    }
}
