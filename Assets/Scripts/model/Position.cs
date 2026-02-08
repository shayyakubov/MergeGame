using System;

namespace Game.Model
{
    public readonly struct Position : IEquatable<Position>
    {
        public int Column { get; }
        public int Row { get; }

        public Position(int column, int row)
        {
            Column = column;
            Row = row;
        }

        public bool Equals(Position other)
        {
            return Column == other.Column && Row == other.Row;
        }

        public override bool Equals(object obj)
        {
            if (obj is Position other)
            {
                return Equals(other);
            }

            return false;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Column * 397) ^ Row;
            }
        }

        public static bool operator ==(Position left, Position right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Position left, Position right)
        {
            return !left.Equals(right);
        }
    }
}
