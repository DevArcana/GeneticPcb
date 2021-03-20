using System;
using GeneticPcb.Core.Models;

namespace GeneticPcb.Core.RandomSolver
{
    public class RandomSolver
    {
        private readonly Random _random;
        private readonly CircuitBoard _board;

        public RandomSolver(Random random, CircuitBoard board)
        {
            _random = random;
            _board = board;
        }

        public CircuitBoard Solve(int tries)
        {
            return _board;
        }
    }
}