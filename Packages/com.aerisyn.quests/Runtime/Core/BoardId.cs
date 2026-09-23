using System;

namespace Aerisyn.Quests
{
    /// <summary>
    /// Identity of a Board: a game-chosen name for a set of Quests that open and close together
    /// (e.g. "stage.1.3", "daily", "event.boat-breaker"). Compared with ordinal (case-sensitive) string equality.
    /// </summary>
    public readonly struct BoardId : IEquatable<BoardId>
    {
        /// <summary>The Board name. Keep it stable: saved progress is keyed by it.</summary>
        public readonly string Value;

        /// <summary>Wraps a non-empty Board name. Throws on null or empty.</summary>
        public BoardId(string value)
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentException("BoardId must be a non-empty string.", nameof(value));

            Value = value;
        }

        /// <summary>False for <c>default(BoardId)</c>, i.e. a field that was never assigned.</summary>
        public bool IsValid => !string.IsNullOrEmpty(Value);

        #region Equality

        public bool Equals(BoardId other) => string.Equals(Value, other.Value, StringComparison.Ordinal);

        public override bool Equals(object obj) => obj is BoardId other && Equals(other);

        public override int GetHashCode() => Value == null ? 0 : StringComparer.Ordinal.GetHashCode(Value);

        public static bool operator ==(BoardId left, BoardId right) => left.Equals(right);

        public static bool operator !=(BoardId left, BoardId right) => !left.Equals(right);

        #endregion

        public override string ToString() => Value ?? "<none>";
    }
}
