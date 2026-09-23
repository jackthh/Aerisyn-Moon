using System;

namespace Aerisyn.Quests
{
    /// <summary>
    /// Identity of a Quest: the Board it belongs to plus a local id unique within that Board.
    /// Two Boards may reuse the same local id without conflict ("stage.1/3" and "daily/3" are different Quests).
    /// </summary>
    public readonly struct QuestId : IEquatable<QuestId>
    {
        #region Fields

        /// <summary>The Board that owns this Quest.</summary>
        public readonly BoardId BoardId;

        /// <summary>Id of the Quest inside its Board (<see cref="QuestDefinition.LocalId"/>). Not unique across Boards.</summary>
        public readonly int LocalId;

        #endregion

        public QuestId(BoardId boardId, int localId)
        {
            BoardId = boardId;
            LocalId = localId;
        }

        #region Equality

        public bool Equals(QuestId other) => BoardId.Equals(other.BoardId) && LocalId == other.LocalId;

        public override bool Equals(object obj) => obj is QuestId other && Equals(other);

        public override int GetHashCode()
        {
            // Standard prime-multiply combine; overflow is expected and harmless.
            unchecked
            {
                return (BoardId.GetHashCode() * 397) ^ LocalId;
            }
        }

        public static bool operator ==(QuestId left, QuestId right) => left.Equals(right);

        public static bool operator !=(QuestId left, QuestId right) => !left.Equals(right);

        #endregion

        #region Obsolete aliases (removed in 0.2.0)

        [Obsolete("Renamed to QuestId.BoardId for clarity. This alias is removed in 0.2.0.")]
        public BoardId Board => BoardId;

        #endregion

        /// <summary>Debug form: <c>board/localId</c>.</summary>
        public override string ToString() => BoardId + "/" + LocalId;
    }
}
