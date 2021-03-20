using System;
using System.Drawing;
using System.IO;
using GeneticPcb.Core.Models;

namespace GeneticPcb.Core.Display
{
    public static class PcbImageWriter
    {
        private static void DrawRectangle(this Bitmap bitmap, int x, int y, int w, int h, Color color)
        {
            if (x < 0 || y < 0 || x > bitmap.Width || y > bitmap.Height || x + w > bitmap.Width || y + h > bitmap.Height)
            {
                color = Color.Red;
            }

            for (var i = 0; i < w; i++)
            {
                for (var j = 0; j < h; j++)
                {
                    var px = x + i;
                    var py = y + j;
                    
                    if (px >= 0 && px < bitmap.Width && py >= 0 && py < bitmap.Height)
                    {
                        bitmap.SetPixel(px, py, color);
                    }
                }
            }
        }

        public static void ResetSolutions()
        {
            if (Directory.Exists("generations"))
            {
                Directory.Delete("generations", true);
            }

            Directory.CreateDirectory("generations");
        }

        public static void SaveSolution(CircuitBoard board, int index)
        {
            const int scale = 32;

            var image = new Bitmap(board.Width * scale, board.Height * scale);

            image.DrawRectangle(0, 0, board.Width * 32, board.Height * 32, Color.DarkGreen);

            for (var x = 0; x < board.Width; x++)
            {
                for (var y = 0; y < board.Height; y++)
                {
                    image.DrawRectangle(x * scale + 4, y * scale + 4, 24, 24, Color.ForestGreen);
                }
            }

            foreach (var route in board.Routes)
            {
                var path = route.Path;

                var currentPoint = path.Start;
                foreach (var (direction, length) in path.Segments)
                {
                    var endPoint = direction switch
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

                    image.DrawRectangle(x * 32 + diff, y * 32 + diff, width * 32 - diff * 2, height * 32 - diff * 2,
                        Color.Black);

                    currentPoint = endPoint;
                }

                image.DrawRectangle(route.Start.X * scale + 6, route.Start.Y * scale + 6, 20, 20, Color.Chocolate);
                image.DrawRectangle(route.End.X * scale + 6, route.End.Y * scale + 6, 20, 20, Color.Chocolate);
            }

            image.Save($"generations/{index:000000}_{board.Fitness}.bmp");
        }
    }
}