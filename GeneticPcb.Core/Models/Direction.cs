using System;

namespace GeneticPcb.Core.Models
{
    public enum Direction
    {
        Up,
        Down,
        Left,
        Right
    }

    public static class DirectionExtensions
    {
        public static Direction Opposite(this Direction direction)
        {
            return direction switch
            {
                Direction.Up => Direction.Down,
                Direction.Down => Direction.Up,
                Direction.Left => Direction.Right,
                Direction.Right => Direction.Left,
                _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
            };
        }

        public static Direction GetDirection(BoardPoint from, BoardPoint to)
        {
            var dx = to.X - from.X;
            var dy = to.Y - from.Y;

            if (Math.Abs(dx) > Math.Abs(dy))
            {
                return dx > 0 ? Direction.Right : Direction.Left;
            }

            return dy > 0 ? Direction.Down : Direction.Up;
        }
    }
}