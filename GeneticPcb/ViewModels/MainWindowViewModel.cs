using System;
using System.Collections.Generic;
using System.Linq;
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
            set => this.RaiseAndSetIfChanged(ref _circuitBoard, value);
        }

        public IEnumerable<BoardPoint> SolderingPoints => _circuitBoard.Routes.SelectMany(route => new []{route.Start, route.End});
        public IEnumerable<DisplaySegment> Segments => _circuitBoard.Routes.SelectMany(route =>
        {
            var path = route.Path;

            var segments = new List<DisplaySegment>(path.Segments.Count);

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

                segments.Add(new DisplaySegment(currentPoint, endPoint));
                currentPoint = endPoint;
            }
            
            return segments;
        });

        public MainWindowViewModel()
        {
            var routes = new Route[]
            {
                new(new BoardPoint(2,4), new BoardPoint(3, 1))
            };

            var path = routes[0].Path;
            path.AddSegment(Direction.UP, 3);
            path.AddSegment(Direction.RIGHT, 1);
            
            _circuitBoard = new CircuitBoard(8, 8, routes);
        }
    }
}