using System;

namespace Aerisyn.Quests
{
    /// <summary>
    /// Identity of a Board: a game-chosen name for a set of Quests that open and close together
    /// (e.g. "stage.1.3", "daily", "event.boat-breaker").
    /// </summary>
    public readonly struct BoardId : IEquatable<BoardId>
    {
        public readonly string Value;

        public BoardId(string value)
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentException("BoardId must be a non-empty string.", nameof(value));

            Value = value;
        }

        /// <summary>False for the default struct value; useful when a field was never assigned.</summary>
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
