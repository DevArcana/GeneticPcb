using System;
using System.Collections.Generic;
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

    private CircuitBoard[] Selection(CircuitBoard[] population)
    {
      // tournament, best out of k participants
      foreach (var circuitBoard in population)
      {
        circuitBoard.CalculateFitness();
      }

      var selected = new List<CircuitBoard>(population.Length);

      while (selected.Count < population.Length)
      {
        var winner = population[_random.Next(0, population.Length)];

        for (var i = population.Length / 8; i > 1; i--)
        {
          var candidate = population[_random.Next(0, population.Length)];
          if (candidate.Fitness < winner.Fitness)
          {
            winner = candidate;
          }
        }
        
        selected.Add(winner);
      }
      
      return selected.ToArray();
    }

    private CircuitBoard[] Crossover(CircuitBoard[] population)
    {
      return population;
    }

    private CircuitBoard[] Mutation(CircuitBoard[] population)
    {
      return population;
    }
    
    public CircuitBoard Solve(int generations, int populationSize)
    {
      var population = CreateInitialPopulation(populationSize);

      var solutions = new List<CircuitBoard>(generations);

      while (generations > 0)
      {
        generations--;
        population = Selection(population);
        population = Crossover(population);
        population = Mutation(population);
        
        var bestInPopulationFitness = population.Min(x => x.CalculateFitness());
        var bestInPopulation = population.FirstOrDefault(x => x.Fitness == bestInPopulationFitness);
        
        solutions.Add(bestInPopulation);
      }
      
      var bestFitness = solutions.Min(x => x.Fitness);
      return solutions.FirstOrDefault(x => x.Fitness == bestFitness);
    }
  }
}