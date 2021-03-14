using System;
using System.Linq;

namespace GeneticPcb.Core.Models
{
    public sealed record CircuitBoard (int Width, int Height, Route[] Routes)
    {
        public long Fitness { get; private set; }
        
        public CircuitBoard Copy()
        {
            var routes = new Route[Routes.Length];

            for (var i = 0; i < routes.Length; i++)
            {
                var route = Routes[i];
                routes[i] = new Route(route.Start, route.End);

                foreach (var segment in route.Path.Segments)
                {
                    routes[i].Path.AddSegment(segment.Direction, segment.Length);
                }
            }

            return this with {Routes = routes};
        }

        private int CountIntersections()
        {
            var matrix = new byte[Width * Height];
            var outOfBounds = 0;

            foreach (var route in Routes)
            {
                var current = route.Path.Start;
                matrix[current.X + current.Y * Width]++;
                
                foreach (var (direction, length) in route.Path.Segments)
                {
                    switch (direction)
                    {
                        case Direction.Up:
                        {
                            for (var i = 1; i <= length; i++)
                            {
                                var index = current.X + (current.Y - i) * Width;

                                if (index < 0 || index >= matrix.Length)
                                {
                                    outOfBounds++;
                                }
                                else
                                {
                                    matrix[index]++;
                                }
                            }
                            current = current with {Y = current.Y - length};

                            break;
                        }
                        case Direction.Down:
                        {
                            for (var i = 1; i <= length; i++)
                            {
                                var index = current.X + (current.Y + i) * Width;
                                
                                if (index < 0 || index >= matrix.Length)
                                {
                                    outOfBounds++;
                                }
                                else
                                {
                                    matrix[index]++;
                                }
                            }
                            current = current with {Y = current.Y + length};

                            break;
                        }
                        case Direction.Left:
                        {
                            for (var i = 1; i <= length; i++)
                            {
                                var index = current.X - i + current.Y * Width;
                                
                                if (index < 0 || index >= matrix.Length)
                                {
                                    outOfBounds++;
                                }
                                else
                                {
                                    matrix[index]++;
                                }
                            }
                            current = current with {X = current.X - length};

                            break;
                        }
                        case Direction.Right:
                        {
                            for (var i = 1; i <= length; i++)
                            {
                                var index = current.X + i + current.Y * Width;
                                
                                if (index < 0 || index >= matrix.Length)
                                {
                                    outOfBounds++;
                                }
                                else
                                {
                                    matrix[index]++;
                                }
                            }
                            current = current with {X = current.X + length};

                            break;
                        }
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }
            
            return matrix.Count(x => x > 1) + outOfBounds;
        }

        public long CalculateFitness() {
            long fitness = 0;

            foreach (var route in Routes)
            {
                var segments = route.Path.Segments.Count;
                var length = route.Path.Segments.Sum(x => x.Length);

                fitness += length;
                fitness += segments * 10;

                if (!route.IsConnected)
                {
                    var dx = (long) route.End.X - route.Path.End.X;
                    var dy = (long) route.End.Y - route.Path.End.Y;
                    fitness += 1000 * (dx*dx + dy*dy);
                }
            }

            fitness += CountIntersections() * 100;

            Fitness = fitness;
            return fitness;
        }
    }
}