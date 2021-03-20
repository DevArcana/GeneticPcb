using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using DynamicData;
using GeneticPcb.Core;
using GeneticPcb.Core.GeneticSolver;
using GeneticPcb.Core.Models;
using GeneticPcb.Core.RandomSolver;
using GeneticPcb.Models;
using ReactiveUI;

namespace GeneticPcb.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private CircuitBoard _circuitBoard;

        public CircuitBoard CircuitBoard
        {
            get => _circuitBoard;
            set
            {
                this.RaiseAndSetIfChanged(ref _circuitBoard, value);

                var solderingPoints = value.Routes.SelectMany((route, i) =>
                {
                    var routeName = i.ToString();

                    return new[]
                    {
                        new SolderingPoint(route.Start.X, route.Start.Y, routeName),
                        new SolderingPoint(route.End.X, route.End.Y, routeName)
                    };
                });

                SolderingPoints.Clear();
                SolderingPoints.AddRange(solderingPoints);

                var segments = value.Routes.SelectMany(route =>
                {
                    var path = route.Path;

                    var rectangles = new List<SegmentRectangle>(path.Segments.Count);

                    var currentPoint = path.Start;
                    foreach (var (direction, length) in path.Segments)
                    {
                        BoardPoint endPoint = direction switch
                        {
                            Direction.Up => currentPoint with {Y = currentPoint.Y - length},
                            Direction.Down => currentPoint with {Y = currentPoint.Y + length},
                            Direction.Right => currentPoint with {X = currentPoint.X + length},
                            Direction.Left => currentPoint with {X = currentPoint.X - length},
                            _ => null
                        } ?? throw new InvalidOperationException("Invalid segment detected!");

                        var x = currentPoint.X;
                        var y = currentPoint.Y;

                        var width = 1;
                        var height = 1;

                        const int diff = 14;

                        switch (direction)
                        {
                            case Direction.Up:
                                y -= length;
                                height = length + 1;
                                break;
                            case Direction.Down:
                                height = length + 1;
                                break;
                            case Direction.Left:
                                x -= length;
                                width = length + 1;
                                break;
                            case Direction.Right:
                                width = length + 1;
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }

                        rectangles.Add(new SegmentRectangle(x * 32 + diff, y * 32 + diff, width * 32 - diff * 2,
                            height * 32 - diff * 2));

                        currentPoint = endPoint;
                    }

                    return rectangles;
                });

                Segments.Clear();
                Segments.AddRange(segments);
            }
        }

        public ObservableCollection<SolderingPoint> SolderingPoints { get; }

        public ObservableCollection<SegmentRectangle> Segments { get; }

        #region Genetic Parameters

        private int _generations = 100;

        public int Generations
        {
            get => _generations;
            set => this.RaiseAndSetIfChanged(ref _generations, value);
        }
        
        private int _population = 20;

        public int Population
        {
            get => _population;
            set => this.RaiseAndSetIfChanged(ref _population, value);
        }
        
        private int _mutationChance = 90;

        public int MutationChance
        {
            get => _mutationChance;
            set => this.RaiseAndSetIfChanged(ref _mutationChance, value);
        }
        
        private int _insertChance = 90;

        public int InsertChance
        {
            get => _insertChance;
            set => this.RaiseAndSetIfChanged(ref _insertChance, value);
        }
        
        private int _lengthWeight = HackyUglySingleton.LengthWeight;

        public int LengthWeight
        {
            get => _lengthWeight;
            set
            {
                HackyUglySingleton.LengthWeight = value;
                this.RaiseAndSetIfChanged(ref _lengthWeight, value);
            }
        }
        
        private int _segmentWeight = HackyUglySingleton.SegmentWeight;

        public int SegmentWeight
        {
            get => _segmentWeight;
            set
            {
                HackyUglySingleton.SegmentWeight = value;
                this.RaiseAndSetIfChanged(ref _segmentWeight, value);
            }
        }
        
        private int _intersectionWeight = HackyUglySingleton.IntersectionWeight;

        public int IntersectionWeight
        {
            get => _intersectionWeight;
            set
            {
                HackyUglySingleton.IntersectionWeight = value;
                this.RaiseAndSetIfChanged(ref _intersectionWeight, value);
            }
        }
        
        private int _outOfBoundsWeight = HackyUglySingleton.OutOfBoundsWeight;

        public int OutOfBoundsWeight
        {
            get => _outOfBoundsWeight;
            set
            {
                HackyUglySingleton.OutOfBoundsWeight = value;
                this.RaiseAndSetIfChanged(ref _outOfBoundsWeight, value);
            }
        }

        #endregion

        public MainWindowViewModel()
        {
            var routes = new Route[]
            {
                new(new BoardPoint(2, 4), new BoardPoint(3, 1))
            };

            var path = routes[0].Path;
            path.AddSegment(Direction.Up, 3);
            path.AddSegment(Direction.Right, 1);
            
            SolderingPoints = new ObservableCollection<SolderingPoint>();
            Segments = new ObservableCollection<SegmentRectangle>();
            
            CircuitBoard = new CircuitBoard(8, 8, routes);
        }

        public async Task LoadPcbSpecification()
        {
            var fileDialog = new OpenFileDialog();
            var result = await fileDialog.ShowAsync(HackyUglyStaticHelper.MainWindow);

            if (result != null && result.Any())
            {
                var filePath = result[0];

                var contents = await File.ReadAllLinesAsync(filePath);

                var dimensions = contents[0].Split(";");

                var width = int.Parse(dimensions[0]);
                var height = int.Parse(dimensions[1]);

                var routes = new List<Route>();

                for (var i = 1; i < contents.Length; i++)
                {
                    var coords = contents[i].Split(";");

                    var x1 = int.Parse(coords[0]);
                    var y1 = int.Parse(coords[1]);
                    var x2 = int.Parse(coords[2]);
                    var y2 = int.Parse(coords[3]);

                    routes.Add(new Route(new BoardPoint(x1, y1), new BoardPoint(x2, y2)));
                }

                CircuitBoard = new CircuitBoard(width, height, routes.ToArray());
            }
        }

        public void SolveGenetically()
        {
            var random = new Random();
            var solver = new GeneticSolver(random, CircuitBoard);

            CircuitBoard = solver.SolveTenFold(Generations, Population, MutationChance, InsertChance);
        }
        
        public void SolveRandomly()
        {
            var random = new Random();
            var solver = new RandomSolver(random, CircuitBoard);

            CircuitBoard = solver.Solve(Generations * Population * 10);
        }
    }
}