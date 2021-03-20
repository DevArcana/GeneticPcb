using System;
using System.Collections.Generic;
using System.Linq;
using GeneticPcb.Core.Display;
using GeneticPcb.Core.Models;
using Serilog;
using Serilog.Core;

namespace GeneticPcb.Core.GeneticSolver
{
  public class GeneticSolver
  {
    private readonly Random _random;
    private readonly CircuitBoard _board;
    private readonly Logger _logger;

    public GeneticSolver(Random random, CircuitBoard board)
    {
      _random = random;
      _board = board;

      _logger = new LoggerConfiguration()
        .WriteTo.Console()
        .WriteTo.File("generations.log")
        .CreateLogger();
    }

    private CircuitBoard[] CreateInitialPopulation(int size)
    {
      var boards = new CircuitBoard[size];

      for (var i = 0; i < size; i++)
      {
        boards[i] = _board.Copy();
        var board = boards[i];

        foreach (var route in board.Routes)
        {
          route.Path.Clear();
        }
        
        var @break = false;
        while (!@break)
        {
          var allConnected = true;
          foreach (var route in board.Routes)
          {
            if (route.IsConnected)
            {
              continue;
            }
            else
            {
              allConnected = false;
            }
            
            route.Path.AddSegment((Direction) _random.Next(0, 4), 1);
            board.CalculateFitness();

            if (board.HasIntersections)
            {
              route.Path.DeleteSegment(route.Path.Segments.Count - 1);
              @break = true;
              break;
            }
          }

          @break = @break || allConnected;
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

        for (var i = 5; i > 1; i--)
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
        
        foreach (var j in Enumerable.Range(0, mom.Routes.Length).OrderBy(x => Guid.NewGuid()))
        {
          var dadRoute = dad.Routes[j];
          var momRoute = mom.Routes[j];

          var dadFitness = dad.CalculateFitness();
          var momFitness = mom.CalculateFitness();
          
          dad.Routes[j] = momRoute;
          mom.Routes[j] = dadRoute;

          if (dad.CalculateFitness() > dadFitness)
          {
            dad.Routes[j] = dadRoute;
          }
          
          if (mom.CalculateFitness() > momFitness)
          {
            mom.Routes[j] = momRoute;
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
          if (!route.IsConnected)
          {
            var direction = (Direction) _random.Next(0, 4);
            var length = 1;
            
            route.Path.AddSegment(direction, length);
          }
          else
          {
            var shouldMutate = _random.Next(0, 101);

            if (shouldMutate > mutationChance)
            {
              var shouldInsert = _random.Next(0, 101);
              if (shouldInsert > insertChance)
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
                route.Path.DeleteSegment(_random.Next(0, route.Path.Segments.Count), true);
              }
            }
          }
        }
      }
      
      return population;
    }

    public CircuitBoard SolveTenFold(int generations, int populationSize, int mutationChance, int insertChance)
    {
      var solutions = new List<CircuitBoard>();

      _logger.Information("Starting GA: generations {Generations} population {Population} mutation {Mutation} insert {Insert}", generations, populationSize, mutationChance, insertChance);
      
      for (var i = 0; i < 9; i++)
      {
        solutions.Add(Solve(generations, populationSize, mutationChance, insertChance));
      }
      
      solutions.Add(Solve(generations, populationSize, mutationChance, insertChance, true));

      var worstFitness = solutions.Max(x => x.Fitness);
      var bestFitness = solutions.Min(x => x.Fitness);
      var averageFitness = solutions.Average(x => x.Fitness);

      var standardDeviation = solutions.Sum(x => (x.Fitness - averageFitness) * (x.Fitness - averageFitness));
      standardDeviation /= solutions.Count;
      standardDeviation = Math.Sqrt(standardDeviation);
      
      _logger.Information("Best: {Best} Worst: {Worst} Average: {Average} Std: {Deviation}", bestFitness, worstFitness, averageFitness, standardDeviation);

      return solutions.FirstOrDefault(x => x.Fitness == bestFitness);
    }

    private CircuitBoard Solve(int generations, int populationSize, int mutationChance, int insertChance, bool recordImages = false)
    {
      if (recordImages)
      {
        PcbImageWriter.ResetSolutions();
      }
      
      var population = CreateInitialPopulation(populationSize);

      var solutions = new List<CircuitBoard>(generations);

      for (var i = 0; i < generations; i++) {
        population = Selection(population);
        population = Crossover(population);
        population = Mutation(population, mutationChance, insertChance);
        
        var bestInPopulationFitness = population.Min(x => x.CalculateFitness());
        var bestInPopulation = population.FirstOrDefault(x => x.Fitness == bestInPopulationFitness);

        if (recordImages)
        {
          PcbImageWriter.SaveSolution(bestInPopulation, i);
        }
        solutions.Add(bestInPopulation);
      }
      
      var bestFitness = solutions.Min(x => x.Fitness);
      return solutions.FirstOrDefault(x => x.Fitness == bestFitness);
    }
  }
}