using GeneticPcb.Core.Models;
using NUnit.Framework;

namespace GeneticPcb.Tests
{
    [TestFixture]
    public class RouteTests
    {
        [Test]
        public void EnsureRouteConnects()
        {
            var route = new Route(new BoardPoint(2, 4), new BoardPoint(3, 1));
            
            var path = route.Path;
            Assert.That(route.IsConnected, Is.False);
            
            path.AddSegment(Direction.UP, 3);
            Assert.That(route.IsConnected, Is.False);
            
            path.AddSegment(Direction.RIGHT, 1);
            Assert.That(route.IsConnected, Is.True);
        }
    }
}