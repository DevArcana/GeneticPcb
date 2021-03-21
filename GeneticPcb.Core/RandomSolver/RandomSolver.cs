using System;
using System.Collections.Generic;
using System.Linq;
using GeneticPcb.Core.Models;
using Serilog;
using Serilog.Core;

namespace GeneticPcb.Core.RandomSolver
{
    public class RandomSolver
    {
        private readonly Random _random;
        private readonly CircuitBoard _board;
        private readonly Logger _logger;

        public RandomSolver(Random random, CircuitBoard board)
        {
            _random = random;
            _board = board;
            
            _logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File("generations.log")
                .CreateLogger();
        }

        private CircuitBoard InitialBoard()
        {
            var board = _board.Copy();

            foreach (var route in board.Routes)
            {
                route.Path.Clear();

                while (!route.IsConnected)
                {
                    var direction = route.Path.End.GetDirection(route.End);

                    route.Path.AddSegment(direction,
                        direction is Direction.Down or Direction.Up
                            ? Math.Abs(route.Path.End.Y - route.End.Y)
                            : Math.Abs(route.Path.End.X - route.End.X));
                }
            }
            
            return board;
        }

        private CircuitBoard NextBoard(CircuitBoard parent)
        {
            var board = parent.Copy();

            var routeIndex = _random.Next(0, board.Routes.Length);
            var route = board.Routes[routeIndex];

            var segmentIndex = _random.Next(0, route.Path.Segments.Count);
            var operationType = _random.Next();

            if (operationType % 2 == 0)
            {
                route.Path.DeleteSegment(segmentIndex, true);
            }
            else
            {
                var direction = (Direction) _random.Next(0, 4);
                var length = direction switch
                {
                    Direction.Up => route.Path.End.Y,
                    Direction.Down => _board.Height - route.Path.End.Y,
                    Direction.Left => route.Path.End.X,
                    Direction.Right => _board.Width - route.Path.End.X,
                    _ => 0
                };
                
                route.Path.InsertSegment(segmentIndex, direction, length, true, _random.Next(0, route.Path.Segments.Count));
            }
            
            return board;
        }

        public CircuitBoard Solve(int tries)
        {
            _logger.Information("Starting Random Solve: {Tries} tries", tries);

            var solutions = new List<CircuitBoard>(tries);

            var original = InitialBoard();
            solutions.Add(original);

            for (var i = 0; i < tries - 1; i++)
            {
                var board = NextBoard(original);
                original = board;
                solutions.Add(board);
            }

            var worstFitness = solutions.Max(x => x.CalculateFitness());
            var bestFitness = solutions.Min(x => x.Fitness);
            var averageFitness = solutions.Average(x => x.Fitness);

            var standardDeviation = solutions.Sum(x => (x.Fitness - averageFitness) * (x.Fitness - averageFitness));
            standardDeviation /= solutions.Count;
            standardDeviation = Math.Sqrt(standardDeviation);
      
            _logger.Information("Best: {Best} Worst: {Worst} Average: {Average} Std: {Deviation}", bestFitness, worstFitness, averageFitness, standardDeviation);

            return solutions.FirstOrDefault(x => x.Fitness == bestFitness);
        }
    }
}