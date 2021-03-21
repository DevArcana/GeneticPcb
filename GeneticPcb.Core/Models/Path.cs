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

                // if (previous.Direction == segment.Direction)
                // {
                //     previous = previous with {Length = previous.Length + segment.Length};
                //     _segments[i - 1] = previous;
                //     
                //     _segments.RemoveAt(i);
                //     i--;
                //     count--;
                // }
                // else
                if (previous.Direction == segment.Direction.Opposite())
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

                        previous = i == -1 ? null : _segments[i];
                    }
                    else
                    {
                        previous = segment with {Length = segment.Length - previous.Length};
                        _segments[i - 1] = previous;
                        
                        _segments.RemoveAt(i);
                        
                        i -= 2;
                        count--;
                        
                        previous = i == -1 ? null : _segments[i];
                    }
                }
                else
                {
                    previous = segment;
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

        public void DeleteSegment(int i, bool conserveEnd = false, int conserveAt = -1)
        {
            var segment = _segments[i];
            _segments.RemoveAt(i);

            if (conserveEnd)
            {
                if (conserveAt == -1)
                {
                    _segments.Add(segment);
                }
                else
                {
                    _segments.Insert(conserveAt, segment);
                }
            }
            
            Optimize();
            Recalculate();
        }
        
        public void InsertSegment(int i, Direction direction, int length, bool conserveEnd = false, int conserveAt = -1)
        {
            if (length <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(length), length, "Length must be positive!");
            }
            
            _segments.Insert(i, new Segment(direction, length));

            if (conserveEnd)
            {
                if (conserveAt == -1)
                {
                    _segments.Add(new Segment(direction.Opposite(), length));
                }
                else
                {
                    _segments.Insert(conserveAt, new Segment(direction.Opposite(), length));
                }
            }
            
            Optimize();
            Recalculate();
        }

        public void AddSegment(Direction direction, int length)
        {
            if (length <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(length), length, "Length must be positive!");
            }

            End = direction switch
            {
                Direction.Down => End with {Y = End.Y + length},
                Direction.Up => End with {Y = End.Y - length},
                Direction.Right => End with {X = End.X + length},
                Direction.Left => End with {X = End.X - length},
                _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, "Invalid direction.")
            };

            _segments.Add(new Segment(direction, length));
        }

        public void Clear()
        {
            _segments.Clear();
            Optimize();
            Recalculate();
        }
    }
}