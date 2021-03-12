using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using DynamicData;
using GeneticPcb.Core.Models;
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

                    var segments = new List<SegmentRectangle>(path.Segments.Count);

                    var currentPoint = path.Start;
                    foreach (var (direction, length) in path.Segments)
                    {
                        BoardPoint endPoint = direction switch
                        {
                            Direction.UP => currentPoint with {Y = currentPoint.Y - length},
                            Direction.DOWN => currentPoint with {Y = currentPoint.Y + length},
                            Direction.RIGHT => currentPoint with {X = currentPoint.X + length},
                            Direction.LEFT => currentPoint with {X = currentPoint.X - length},
                            _ => null
                        } ?? throw new InvalidOperationException("Invalid segment detected!");

                        var x = currentPoint.X;
                        var y = currentPoint.Y;

                        uint width = 1;
                        uint height = 1;

                        const uint diff = 14;

                        if (direction == Direction.UP)
                        {
                            y -= length;
                            height = length + 1;
                        }
                        else if (direction == Direction.DOWN)
                        {
                            height = length + 1;
                        }
                        else if (direction == Direction.LEFT)
                        {
                            x -= length;
                            width = length + 1;
                        }
                        else if (direction == Direction.RIGHT)
                        {
                            width = length + 1;
                        }

                        segments.Add(new SegmentRectangle(x * 32 + diff, y * 32 + diff, width * 32 - diff * 2,
                            height * 32 - diff * 2));

                        currentPoint = endPoint;
                    }

                    return segments;
                });

                Segments.Clear();
                Segments.AddRange(segments);
            }
        }

        public ObservableCollection<SolderingPoint> SolderingPoints { get; }

        public ObservableCollection<SegmentRectangle> Segments { get; }

        public MainWindowViewModel()
        {
            var routes = new Route[]
            {
                new(new BoardPoint(2, 4), new BoardPoint(3, 1))
            };

            var path = routes[0].Path;
            path.AddSegment(Direction.UP, 3);
            path.AddSegment(Direction.RIGHT, 1);
            
            SolderingPoints = new ObservableCollection<SolderingPoint>();
            Segments = new ObservableCollection<SegmentRectangle>();
            
            CircuitBoard = new CircuitBoard(8, 8, routes);
        }

        public async Task LoadPcbSpecification()
        {
            var fileDialog = new OpenFileDialog();
            var result = await fileDialog.ShowAsync(HackyUglySingleton.MainWindow);

            if (result != null && result.Any())
            {
                var filePath = result[0];

                var contents = await File.ReadAllLinesAsync(filePath);

                var dimensions = contents[0].Split(";");

                var width = uint.Parse(dimensions[0]);
                var height = uint.Parse(dimensions[1]);

                var routes = new List<Route>();

                for (int i = 1; i < contents.Length; i++)
                {
                    var coords = contents[i].Split(";");

                    var x1 = uint.Parse(coords[0]);
                    var y1 = uint.Parse(coords[1]);
                    var x2 = uint.Parse(coords[2]);
                    var y2 = uint.Parse(coords[3]);

                    routes.Add(new Route(new BoardPoint(x1, y1), new BoardPoint(x2, y2)));
                }

                CircuitBoard = new CircuitBoard(width, height, routes.ToArray());
            }
        }
    }
}