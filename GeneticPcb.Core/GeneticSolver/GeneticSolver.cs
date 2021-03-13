using System;
using System.Linq;
using GeneticPcb.Core.Models;

namespace GeneticPcb.Core.GeneticSolver
{
  public class GeneticSolver
  {
    private readonly Random _random;
    private readonly CircuitBoard _board;

    public GeneticSolver(Random random, CircuitBoard board)
    {
      _random = random;
      _board = board;
    }

    private CircuitBoard[] CreateInitialPopulation(int size)
    {
      var boards = new CircuitBoard[size];

      for (var i = 0; i < size; i++)
      {
        boards[i] = _board.Copy();
      }
      
      return boards;
    }

    public CircuitBoard Solve(int generations, int populationSize)
    {
      var population = CreateInitialPopulation(populationSize);

      while (generations > 0)
      {
        generations--;
        // TODO: Test performance
        population = CreateInitialPopulation(populationSize);
      }

      var maxFitness = population.Max(x => x.CalculateFitness());
      return population.FirstOrDefault(x => x.Fitness == maxFitness);
    }
  }
}