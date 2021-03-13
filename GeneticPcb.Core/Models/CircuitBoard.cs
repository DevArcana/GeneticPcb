using System;
using System.Linq;

namespace GeneticPcb.Core.Models
{
    public sealed record CircuitBoard (uint Width, uint Height, Route[] Routes)
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

            foreach (var route in Routes)
            {
                var current = route.Path.Start;
                matrix[current.X + current.Y * Width]++;
                
                foreach (var segment in route.Path.Segments)
                {
                    switch (segment.Direction)
                    {
                        case Direction.UP:
                        {
                            for (var i = 1; i <= segment.Length; i++)
                            {
                                matrix[current.X + (current.Y - i) * Width]++;
                            }
                            current = current with {Y = current.Y - segment.Length};

                            break;
                        }
                        case Direction.DOWN:
                        {
                            for (var i = 1; i <= segment.Length; i++)
                            {
                                matrix[current.X + (current.Y + i) * Width]++;
                            }
                            current = current with {Y = current.Y + segment.Length};

                            break;
                        }
                        case Direction.LEFT:
                        {
                            for (var i = 1; i <= segment.Length; i++)
                            {
                                matrix[current.X - i + current.Y * Width]++;
                            }
                            current = current with {X = current.X - segment.Length};

                            break;
                        }
                        case Direction.RIGHT:
                        {
                            for (var i = 1; i <= segment.Length; i++)
                            {
                                matrix[current.X + i + current.Y * Width]++;
                            }
                            current = current with {X = current.X + segment.Length};

                            break;
                        }
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }
            
            return matrix.Count(x => x > 1);
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
                    fitness += 1000;
                }
            }

            fitness += CountIntersections() * 100;

            Fitness = fitness;
            return fitness;
        }
    }
}