using GeneticPcb.Core.Models;
using NUnit.Framework;

namespace GeneticPcb.Tests
{
    [TestFixture]
    public class PathTests
    {
        [Test]
        public void EndCoordinatesUpdateCorrectly()
        {
            var start = new BoardPoint(2, 3);
            var path = new Path(start);
            
            path.AddSegment(Direction.Down, 4);
            path.AddSegment(Direction.Right, 3);
            path.AddSegment(Direction.Left, 1);
            path.AddSegment(Direction.Up, 2);
            
            Assert.That(path.End, Is.EqualTo(new BoardPoint(4, 5)));
        }
    }
}