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

        private void Optimize()
        {
            var count = _segments.Count;

            Segment previous = null;
            
            for (var i = 0; i < count; i++)
            {
                var segment = _segments[i];

                if (previous == null)
                {
                    previous = segment;
                    continue;
                }

                if (previous.Direction == segment.Direction)
                {
                    previous = previous with {Length = previous.Length + segment.Length};
                    _segments[i - 1] = previous;
                    
                    _segments.RemoveAt(i);
                    i--;
                    count--;
                }
                else if (previous.Direction == segment.Direction.Opposite())
                {
                    if (previous.Length > segment.Length)
                    {
                        previous = previous with {Length = previous.Length - segment.Length};
                        _segments[i - 1] = previous;
                    
                        _segments.RemoveAt(i);
                        i--;
                        count--;
                    }
                    else if (previous.Length == segment.Length)
                    {
                        _segments.RemoveAt(i);
                        _segments.RemoveAt(i - 1);
                        i -= 2;
                        count -= 2;
                    }
                    else
                    {
                        previous = segment with {Length = segment.Length - previous.Length};
                        _segments[i - 1] = previous;
                        
                        _segments.RemoveAt(i);
                        i--;
                        count--;
                    }
                }
            }
        }

        private void Recalculate()
        {
            End = Start;
            
            foreach (var (direction, length) in _segments)
            {
                End = direction switch
                {
                    Direction.Up => End with {Y = End.Y - length},
                    Direction.Down => End with {Y = End.Y + length},
                    Direction.Left => End with {X = End.X - length},
                    Direction.Right => End with {X = End.X + length},
                    _ => throw new ArgumentOutOfRangeException()
                };
            }
        }

        public void DeleteSegment(int i, bool conserveEnd = false)
        {
            var segment = _segments[i];
            _segments.RemoveAt(i);

            if (conserveEnd)
            {
                _segments.Add(segment);
            }
            
            Optimize();
            Recalculate();
        }
        
        public void InsertSegment(int i, Direction direction, uint length, bool conserveEnd = false)
        {
            _segments.Insert(i, new Segment(direction, length));

            if (conserveEnd)
            {
                _segments.Add(new Segment(direction.Opposite(), length));
            }
            
            Optimize();
            Recalculate();
        }

        public void AddSegment(Direction direction, uint length)
        {
            if (length <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(length), length, "Length must be a positive value!");
            }

            switch (direction)
            {
                case Direction.Down:
                    End = End with { Y = End.Y + length };
                    break;
                case Direction.Up:
                    if (length > End.Y)
                    {
                        throw new ArgumentOutOfRangeException(nameof(length),
                            length,
                            "You can not have a segment of length 0 or below!");
                    }

                    End = End with { Y = End.Y - length };
                    break;
                case Direction.Right:
                    End = End with { X = End.X + length };
                    break;
                case Direction.Left:
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