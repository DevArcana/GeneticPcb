using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace GeneticPcb.Core.Models
{
    public sealed class Path
    {
        private readonly List<Segment> _segments;
        public IReadOnlyCollection<Segment> Segments => new ReadOnlyCollection<Segment>(_segments);

        public BoardPoint Start { get; }
        public BoardPoint End { get; private set; }

        public Path(BoardPoint start)
        {
            Start = start;
            End = Start;

            _segments = new List<Segment>();
        }

        public void AddSegment(Direction direction, uint length)
        {
            if (length <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(length), length, "Length must be a positive value!");
            }

            switch (direction)
            {
                case Direction.DOWN:
                    End = End with { Y = End.Y + length };
                    break;
                case Direction.UP:
                    if (length > End.Y)
                    {
                        throw new ArgumentOutOfRangeException(nameof(length),
                            length,
                            "You can not have a segment of length 0 or below!");
                    }

                    End = End with { Y = End.Y - length };
                    break;
                case Direction.RIGHT:
                    End = End with { X = End.X + length };
                    break;
                case Direction.LEFT:
                    if (length > End.X)
                    {
                        throw new ArgumentOutOfRangeException(nameof(length),
                            length,
                            "The X coordinate must not be negative!");
                    }

                    End = End with { X = End.X - length };
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, "Invalid direction.");
            }

            _segments.Add(new Segment(direction, length));
        }
    }
}