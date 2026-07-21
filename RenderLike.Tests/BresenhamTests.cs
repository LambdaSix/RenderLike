using System.Collections.Generic;
using NUnit.Framework;
using RenderLike;

namespace RenderLike.Tests
{
    [TestFixture]
    public class BresenhamTests
    {
        // Points are encoded as "x,y" strings to stay within C# 6 (net452 has no ValueTuple).
        private static List<string> Plot(int x0, int y0, int x1, int y1)
        {
            var points = new List<string>();
            Bresenham.Line(x0, y0, x1, y1, (x, y) =>
            {
                points.Add(x + "," + y);
                return true;
            });
            return points;
        }

        [Test]
        public void HorizontalLine_PlotsEveryCellOnce()
        {
            Assert.That(Plot(0, 2, 4, 2), Is.EqualTo(new[] { "0,2", "1,2", "2,2", "3,2", "4,2" }));
        }

        [Test]
        public void VerticalLine_PlotsEveryCellOnce()
        {
            Assert.That(Plot(3, 0, 3, 4), Is.EqualTo(new[] { "3,0", "3,1", "3,2", "3,3", "3,4" }));
        }

        [Test]
        public void Diagonal_PlotsPerfectSteps()
        {
            Assert.That(Plot(0, 0, 3, 3), Is.EqualTo(new[] { "0,0", "1,1", "2,2", "3,3" }));
        }

        [Test]
        public void SinglePoint_PlotsExactlyOnce()
        {
            Assert.That(Plot(2, 2, 2, 2), Is.EqualTo(new[] { "2,2" }));
        }

        [Test]
        public void Endpoints_AreAlwaysIncluded()
        {
            var points = Plot(1, 1, 6, 4);
            Assert.That(points, Contains.Item("1,1"));
            Assert.That(points, Contains.Item("6,4"));
        }

        [Test]
        public void Reversed_ProducesTheSameSetOfCells()
        {
            var forward = Plot(0, 0, 6, 3);
            var backward = Plot(6, 3, 0, 0);
            Assert.That(backward, Is.EquivalentTo(forward));
        }

        [Test]
        public void SteepLine_IncludesBothEndpointsAndOneCellPerRow()
        {
            var points = Plot(0, 0, 2, 6);
            Assert.That(points, Contains.Item("0,0"));
            Assert.That(points, Contains.Item("2,6"));
            Assert.That(points.Count, Is.EqualTo(7)); // one cell per row from y=0..6
        }

        [Test]
        public void PlotReturningFalse_StopsIterationImmediately()
        {
            int calls = 0;
            Bresenham.Line(0, 0, 100, 0, (x, y) =>
            {
                calls++;
                return false; // request stop immediately
            });

            Assert.That(calls, Is.EqualTo(1));
        }

        [Test]
        public void PlotReturningFalse_MidwayStopsAtThatCount()
        {
            int calls = 0;
            Bresenham.Line(0, 0, 100, 0, (x, y) =>
            {
                calls++;
                return calls < 5; // stop after the 5th cell
            });

            Assert.That(calls, Is.EqualTo(5));
        }
    }
}
