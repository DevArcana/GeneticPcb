using System;
using System.Collections.Generic;
using System.Linq;
using GeneticPcb.Core.Models;
using GeneticPcb.Display;

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

        foreach (var route in boards[i].Routes)
        {
          route.Path.Clear();
        }
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
        
        selected.Add(winner.Copy());
      }
      
      return selected.ToArray();
    }

    private CircuitBoard[] Crossover(CircuitBoard[] population)
    {
      for (var i = 0; i < population.Length - 1; i += 2)
      {
        var mom = population[i];
        var dad = population[i + 1];

        for (int j = 0; j < mom.Routes.Length; j++)
        {
          if (j % 2 == 0)
          {
            var route = dad.Routes[j];
            dad.Routes[j] = mom.Routes[j];
            mom.Routes[j] = route;
          }
        }
      }
      
      return population;
    }

    private CircuitBoard[] Mutation(CircuitBoard[] population, int mutationChance, int insertChance)
    {
      foreach (var genome in population)
      {
        foreach (var route in genome.Routes)
        {
          var shouldMutate = _random.Next(0, 101);

          if (shouldMutate > mutationChance) continue;
          
          var mutationType = _random.Next(0, 101);

          if (mutationType <= insertChance)
          {
            // insert segment
            if (route.IsConnected)
            {
              var direction = (Direction) _random.Next(0, 4);
              var where = _random.Next(0, route.Path.Segments.Count);

              var length = direction switch
              {
                Direction.Up => route.Path.End.Y,
                Direction.Down => _board.Height - route.Path.End.Y,
                Direction.Left => route.Path.End.X,
                Direction.Right => _board.Width - route.Path.End.X,
                _ => 0
              };

              route.Path.InsertSegment(where, direction, _random.Next(1, length), true);
            }
            else
            {
              var direction = route.Path.End.GetDirection(route.End);
              var where = _random.Next(0, route.Path.Segments.Count);

              var length = direction switch
              {
                Direction.Up => route.Path.End.Y - route.End.Y,
                Direction.Down => route.End.Y - route.Path.End.Y,
                Direction.Left => route.Path.End.X - route.End.X,
                Direction.Right => route.End.X - route.Path.End.X,
                _ => 0
              };

              route.Path.AddSegment(direction, length);
            }
          }
          else if (route.Path.Segments.Any())
          {
            // delete segment
            route.Path.DeleteSegment(_random.Next(0, route.Path.Segments.Count), route.IsConnected);
          }
        }
      }
      
      return population;
    }
    
    public CircuitBoard Solve(int generations, int populationSize, int mutationChance, int insertChance)
    {
      PcbImageWriter.ResetSolutions();
      
      var population = CreateInitialPopulation(populationSize);

      var solutions = new List<CircuitBoard>(generations);

      for (var i = 0; i < generations; i++) {
        population = Selection(population);
        population = Crossover(population);
        population = Mutation(population, mutationChance, insertChance);
        
        var bestInPopulationFitness = population.Min(x => x.CalculateFitness());
        var bestInPopulation = population.FirstOrDefault(x => x.Fitness == bestInPopulationFitness);
        
        PcbImageWriter.SaveSolution(bestInPopulation, i);
        solutions.Add(bestInPopulation);
      }
      
      var bestFitness = solutions.Min(x => x.Fitness);
      return solutions.FirstOrDefault(x => x.Fitness == bestFitness);
    }
  }
}