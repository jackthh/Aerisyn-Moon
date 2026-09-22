using System;

namespace Aerisyn.Quests
{
    /// <summary>
    /// Identity of a Quest: the Board it belongs to plus a local id unique within that Board.
    /// Two Boards may reuse the same local id without conflict.
    /// </summary>
    public readonly struct QuestId : IEquatable<QuestId>
    {
        public readonly BoardId Board;
        public readonly int LocalId;

        public QuestId(BoardId board, int localId)
        {
            Board = board;
            LocalId = localId;
        }

        #region Equality

        public bool Equals(QuestId other) => Board.Equals(other.Board) && LocalId == other.LocalId;

        public override bool Equals(object obj) => obj is QuestId other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                return (Board.GetHashCode() * 397) ^ LocalId;
            }
        }

        public static bool operator ==(QuestId left, QuestId right) => left.Equals(right);

        public static bool operator !=(QuestId left, QuestId right) => !left.Equals(right);

        #endregion

        public override string ToString() => Board + "/" + LocalId;
    }
}
