using System.Diagnostics;
using GameRealisticMap.Geometries;
using Xunit.Abstractions;

namespace GameRealisticMap.Test.Geometries
{
    public class TerrainPolygonTest
    {
        private static TerrainPolygon Square100x100()
        {
            return new TerrainPolygon(new List<TerrainPoint>() { new(100, 100), new(0, 100), new(0, 0), new(100, 0), new(100, 100) });
        }

        private static TerrainPolygon TriangleA()
        {
            return new TerrainPolygon(new List<TerrainPoint>() { new(0, 0), new(0, 100), new(100, 0), new(0, 0) });
        }
        private static TerrainPolygon TriangleB()
        {
            return new TerrainPolygon(new List<TerrainPoint>() { new(101, 101), new(0, 101), new(101, 0), new(101, 101) });
        }

        private static TerrainPolygon Square100x100Far()
        {
            return new TerrainPolygon(new List<TerrainPoint>() { new(1100, 1100), new(1000, 1100), new(1000, 1000), new(1100, 1000), new(1100, 1100) });
        }

        private static TerrainPolygon Square100x100WithHole()
        {
            return new TerrainPolygon(new List<TerrainPoint>() {
                new TerrainPoint(100,100),
                new TerrainPoint(0,100),
                new TerrainPoint(0,0),
                new TerrainPoint(100,0),
                new TerrainPoint(100,100)
            }, new List<List<TerrainPoint>>() { new List<TerrainPoint>() {

                new TerrainPoint(75,75),
                new TerrainPoint(75,25),
                new TerrainPoint(25,25),
                new TerrainPoint(25,75),
                new TerrainPoint(75,75)
            } });
        }

        private static TerrainPolygon Square50x50()
        {
            return new TerrainPolygon(new() { new(75, 75), new(75, 25), new(25, 25), new(25, 75), new(75, 75) });
        }

        private static TerrainPolygon Square10x10()
        {
            return new TerrainPolygon(new() { new(55, 55), new(55, 45), new(45, 45), new(45, 55), new(55, 55) });
        }

        private static TerrainPolygon Square50x50WithHole()
        {
            return new TerrainPolygon(new() { new(75, 75), new(75, 25), new(25, 25), new(25, 75), new(75, 75) },
                new() { new() { new(55, 55), new(55, 45), new(45, 45), new(45, 55), new(55, 55) } });
        }

        private static List<TerrainPolygon> SquareBands100x100WithHole()
        {
            return new List<TerrainPolygon> {
                new (new() { new(0, 0), new(100, 0), new(100, 25), new(0, 25), new(0, 0) }),
                new (new() { new(0, 75), new(100, 75), new(100, 100), new(0, 100), new(0, 75) }),
                new (new() { new(0, 0), new(0, 100), new(25,100), new(25, 0), new(0, 0) }),
                new (new() { new(75, 0), new(75, 100), new(100, 100), new(100, 0), new(75, 0) }),
            };
        }

        [Fact]
        public void TerrainPolygon_InnerCrown_NoHole()
        {
            var poly = Square100x100();

            var innerPoly = Assert.Single(poly.InnerCrown(10));
            Assert.Equal(new TerrainPoint(0, 0), innerPoly.MinPoint);
            Assert.Equal(new TerrainPoint(100, 100), innerPoly.MaxPoint);
            Assert.Equal(new List<TerrainPoint>() {
                new TerrainPoint(100,100),
                new TerrainPoint(0,100),
                new TerrainPoint(0,0),
                new TerrainPoint(100,0),
                new TerrainPoint(100,100)
            }, innerPoly.Shell);
            Assert.Equal(new List<TerrainPoint>() {
                new TerrainPoint(10,10),
                new TerrainPoint(10,90),
                new TerrainPoint(90,90),
                new TerrainPoint(90,10),
                new TerrainPoint(10,10)
            }, Assert.Single(innerPoly.Holes));
        }

        [Fact]
        public void TerrainPolygon_InnerCrown_Hole()
        {
            var poly = Square100x100WithHole();

            var innerPolyList = poly.InnerCrown(5).ToList();

            Assert.Equal(2, innerPolyList.Count);

            var innerPoly = innerPolyList[0];
            Assert.Equal(new TerrainPoint(0, 0), innerPoly.MinPoint);
            Assert.Equal(new TerrainPoint(100, 100), innerPoly.MaxPoint);
            Assert.Equal(new List<TerrainPoint>() {
                new TerrainPoint(100,100),
                new TerrainPoint(0,100),
                new TerrainPoint(0,0),
                new TerrainPoint(100,0),
                new TerrainPoint(100,100)
            }, innerPoly.Shell);
            Assert.Equal(new List<TerrainPoint>() {
                new TerrainPoint(5,5),
                new TerrainPoint(5,95),
                new TerrainPoint(95,95),
                new TerrainPoint(95,5),
                new TerrainPoint(5,5)
            }, Assert.Single(innerPoly.Holes));

            innerPoly = innerPolyList[1];
            Assert.Equal(new TerrainPoint(20, 20), innerPoly.MinPoint);
            Assert.Equal(new TerrainPoint(80, 80), innerPoly.MaxPoint);
            Assert.Equal(9, innerPoly.Shell.Count);
            Assert.Equal(new List<TerrainPoint>() {
                new TerrainPoint(25,25),
                new TerrainPoint(25,75),
                new TerrainPoint(75,75),
                new TerrainPoint(75,25),
                new TerrainPoint(25,25)
            }, Assert.Single(innerPoly.Holes));
        }


        [Fact]
        public void TerrainPolygon_Offset_NoHole()
        {
            var poly = Square100x100();

            var offsetPoly = Assert.Single(poly.Offset(10));
            Assert.Equal(new TerrainPoint(-10, -10), offsetPoly.MinPoint);
            Assert.Equal(new TerrainPoint(110, 110), offsetPoly.MaxPoint);
            Assert.Equal(9, offsetPoly.Shell.Count);
            Assert.Empty(offsetPoly.Holes);
        }

        [Fact]
        public void TerrainPolygon_Offset_Hole()
        {
            var poly = Square100x100WithHole();

            var offsetPoly = Assert.Single(poly.Offset(10));
            Assert.Equal(new TerrainPoint(-10, -10), offsetPoly.MinPoint);
            Assert.Equal(new TerrainPoint(110, 110), offsetPoly.MaxPoint);
            Assert.Equal(9, offsetPoly.Shell.Count);
            Assert.Equal(new List<TerrainPoint>() {
                new TerrainPoint(35,35),
                new TerrainPoint(35,65),
                new TerrainPoint(65,65),
                new TerrainPoint(65,35),
                new TerrainPoint(35,35)
            }, Assert.Single(offsetPoly.Holes));

            offsetPoly = Assert.Single(poly.Offset(-10));
            Assert.Equal(new TerrainPoint(10, 10), offsetPoly.MinPoint);
            Assert.Equal(new TerrainPoint(90, 90), offsetPoly.MaxPoint);
            Assert.Equal(new List<TerrainPoint>() {
                new TerrainPoint(90,90),
                new TerrainPoint(10,90),
                new TerrainPoint(10,10),
                new TerrainPoint(90,10),
                new TerrainPoint(90,90)
            }, offsetPoly.Shell);
            Assert.Equal(9, Assert.Single(offsetPoly.Holes).Count);
        }

        [Fact]
        public void TerrainPolygon_OuterCrown_NoHole()
        {
            var poly = Square100x100();

            var outerPoly = Assert.Single(poly.OuterCrown(10));
            Assert.Equal(new TerrainPoint(-10, -10), outerPoly.MinPoint);
            Assert.Equal(new TerrainPoint(110, 110), outerPoly.MaxPoint);
            Assert.Equal(9, outerPoly.Shell.Count);
            Assert.Equal(new List<TerrainPoint>() {
                new TerrainPoint(0,0),
                new TerrainPoint(0,100),
                new TerrainPoint(100,100),
                new TerrainPoint(100,0),
                new TerrainPoint(0,0)
            }, Assert.Single(outerPoly.Holes));
        }

        [Fact]
        public void TerrainPolygon_Crown_NoHole()
        {
            var poly = Square100x100();

            var crown = Assert.Single(poly.Crown(10, 10));
            Assert.Equal(new TerrainPoint(-10, -10), crown.MinPoint);
            Assert.Equal(new TerrainPoint(110, 110), crown.MaxPoint);
            Assert.Equal(9, crown.Shell.Count);
            Assert.Equal(new List<TerrainPoint>() {
                new TerrainPoint(10,10),
                new TerrainPoint(10,90),
                new TerrainPoint(90,90),
                new TerrainPoint(90,10),
                new TerrainPoint(10,10)
            }, Assert.Single(crown.Holes));
        }

        [Fact]
        public void TerrainPolygon_NearestPoint()
        {
            var polygon = Square100x100();

            // Outside
            Assert.Equal(new(100, 50), polygon.NearestPointBoundary(new(105, 50)));
            Assert.Equal(new(0, 50), polygon.NearestPointBoundary(new(-5, 50)));

            // Inside
            Assert.Equal(new(100, 50), polygon.NearestPointBoundary(new(95, 50)));
            Assert.Equal(new(0, 50), polygon.NearestPointBoundary(new(5, 50)));

            polygon = Square100x100WithHole();

            // Outside
            Assert.Equal(new(100, 50), polygon.NearestPointBoundary(new(105, 50)));
            Assert.Equal(new(0, 50), polygon.NearestPointBoundary(new(-5, 50)));

            // Inside
            Assert.Equal(new(100, 50), polygon.NearestPointBoundary(new(95, 50)));
            Assert.Equal(new(0, 50), polygon.NearestPointBoundary(new(5, 50)));

            // In hole
            Assert.Equal(new(25, 50), polygon.NearestPointBoundary(new(30, 50)));
            Assert.Equal(new(75, 50), polygon.NearestPointBoundary(new(70, 50)));
        }

        [Fact]
        public void TerrainPolygon_Distance()
        {
            var polygon = Square100x100();

            // Outside
            Assert.Equal(5, polygon.Distance(new(105, 50)));
            Assert.Equal(5, polygon.Distance(new(-5, 50)));

            // Inside
            Assert.Equal(0, polygon.Distance(new(95, 50)));
            Assert.Equal(0, polygon.Distance(new(5, 50)));

            polygon = Square100x100WithHole();

            // Outside
            Assert.Equal(5, polygon.Distance(new(105, 50)));
            Assert.Equal(5, polygon.Distance(new(-5, 50)));

            // Inside
            Assert.Equal(0, polygon.Distance(new(95, 50)));
            Assert.Equal(0, polygon.Distance(new(5, 50)));

            // In hole
            Assert.Equal(5, polygon.Distance(new(30, 50)));
            Assert.Equal(5, polygon.Distance(new(70, 50)));
        }


        [Fact]
        public void TerrainPolygon_SubstractAll()
        {
            var result = Square100x100().SubstractAll(new[] { Square50x50() });
            var polygon = Assert.Single(result);
            Assert.Equal(new TerrainPoint(0, 0), polygon.MinPoint);
            Assert.Equal(new TerrainPoint(100, 100), polygon.MaxPoint);
            Assert.Single(polygon.Holes);
            Assert.Equal("POLYGON ((100 100, 0 100, 0 0, 100 0, 100 100), (25 25, 25 75, 75 75, 75 25, 25 25))", polygon.ToString());

            result = Square100x100().SubstractAll(SquareBands100x100WithHole());
            polygon = Assert.Single(result);
            Assert.Equal(new TerrainPoint(25, 25), polygon.MinPoint);
            Assert.Equal(new TerrainPoint(75, 75), polygon.MaxPoint);
            Assert.Equal("POLYGON ((75 75, 25 75, 25 25, 75 25, 75 75))", polygon.ToString());

            result = Square50x50().SubstractAll(new[] { Square100x100() });
            Assert.Empty(result);

            result = Square100x100().SubstractAll(new[] { Square100x100WithHole() });
            polygon = Assert.Single(result);
            Assert.Equal(new TerrainPoint(25, 25), polygon.MinPoint);
            Assert.Equal(new TerrainPoint(75, 75), polygon.MaxPoint);
            Assert.Equal("POLYGON ((75 75, 25 75, 25 25, 75 25, 75 75))", polygon.ToString());
        }

        [Fact]
        public void TerrainPolygon_MergeAll()
        {
            var result = TerrainPolygon.MergeAll(new[] { Square100x100(), Square50x50() });
            var polygon = Assert.Single(result);
            Assert.Equal(new TerrainPoint(0, 0), polygon.MinPoint);
            Assert.Equal(new TerrainPoint(100, 100), polygon.MaxPoint);
            Assert.Equal("POLYGON ((100 100, 0 100, 0 0, 100 0, 100 100))", polygon.ToString());

            result = TerrainPolygon.MergeAll(new[] { Square100x100WithHole(), Square50x50(), Square10x10() });
            polygon = Assert.Single(result);
            Assert.Equal(new TerrainPoint(0, 0), polygon.MinPoint);
            Assert.Equal(new TerrainPoint(100, 100), polygon.MaxPoint);
            Assert.Equal("POLYGON ((100 100, 0 100, 0 0, 100 0, 100 100))", polygon.ToString());

            result = TerrainPolygon.MergeAll(SquareBands100x100WithHole());
            polygon = Assert.Single(result);
            Assert.Equal(new TerrainPoint(0, 0), polygon.MinPoint);
            Assert.Equal(new TerrainPoint(100, 100), polygon.MaxPoint);
            Assert.Single(polygon.Holes);
            Assert.Equal("POLYGON ((0 100, 0 0, 100 0, 100 100, 0 100), (75 25, 25 25, 25 75, 75 75, 75 25))", polygon.ToString());

            result = TerrainPolygon.MergeAll(SquareBands100x100WithHole().Concat(new[] { Square10x10() }).ToList());
            Assert.Equal(2, result.Count);
            polygon = result[0];
            Assert.Equal(new TerrainPoint(0, 0), polygon.MinPoint);
            Assert.Equal(new TerrainPoint(100, 100), polygon.MaxPoint);
            Assert.Single(polygon.Holes);
            Assert.Equal("POLYGON ((100 100, 0 100, 0 0, 100 0, 100 100), (25 25, 25 75, 75 75, 75 25, 25 25))", polygon.ToString());
            polygon = result[1];
            Assert.Equal(new TerrainPoint(45, 45), polygon.MinPoint);
            Assert.Equal(new TerrainPoint(55, 55), polygon.MaxPoint);
            Assert.Equal("POLYGON ((55 55, 45 55, 45 45, 55 45, 55 55))", polygon.ToString());
        }

        [Fact]
        public void TerrainPolygon_IntersectionArea()
        {
            Assert.Equal(7500, Square100x100().IntersectionArea(Square100x100WithHole()));
            Assert.Equal(10000, Square100x100().IntersectionArea(Square100x100()));
            Assert.Equal(0, Square100x100().IntersectionArea(Square100x100Far()));
            Assert.Equal(0, TriangleA().IntersectionArea(TriangleB()));
            Assert.Equal(5000, TriangleA().IntersectionArea(Square100x100()));
        }

        [Fact]
        public void TerrainPolygon_Intersects()
        {
            Assert.True(Square100x100().Intersects(Square100x100WithHole()));
            Assert.True(Square100x100().Intersects(Square100x100()));
            Assert.False(Square100x100().Intersects(Square100x100Far()));
            Assert.False(TriangleA().Intersects(TriangleB()));
            Assert.True(TriangleA().Intersects(Square100x100()));
        }
    }

}
