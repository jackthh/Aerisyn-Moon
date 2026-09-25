using System;

namespace Aerisyn.Tutorial
{
    /// <summary>
    /// Identity of a Tutorial. Game-chosen stable name (e.g. "onboarding.sword").
    /// Compared with ordinal (case-sensitive) string equality.
    /// </summary>
    public readonly struct TutorialId : IEquatable<TutorialId>
    {
        #region Fields

        /// <summary>The Tutorial name. Keep it stable: Progress Snapshots key on it.</summary>
        public readonly string Value;

        #endregion


        #region Construction

        /// <summary>Wraps a non-empty Tutorial name. Throws on null or empty.</summary>
        public TutorialId(string value)
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentException("TutorialId must be a non-empty string.", nameof(value));

            Value = value;
        }

        #endregion


        #region Properties

        /// <summary>False for <c>default(TutorialId)</c>, i.e. a field that was never assigned.</summary>
        public bool IsValid => !string.IsNullOrEmpty(Value);

        #endregion


        #region Equality

        public bool Equals(TutorialId other) =>
            string.Equals(Value, other.Value, StringComparison.Ordinal);


        public override bool Equals(object obj) => obj is TutorialId other && Equals(other);


        public override int GetHashCode() =>
            Value == null ? 0 : StringComparer.Ordinal.GetHashCode(Value);


        public static bool operator ==(TutorialId left, TutorialId right) => left.Equals(right);


        public static bool operator !=(TutorialId left, TutorialId right) => !left.Equals(right);

        #endregion


        public override string ToString() => Value ?? "<none>";
    }
}
