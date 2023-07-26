using System.Numerics;
using GameRealisticMap.Geometries;

namespace GameRealisticMap.Test.Geometries
{
    public class TerrainPathTest
    {
        [Fact]
        public void TerrainPath_Length()
        {
            var path = new TerrainPath(new (0, 0), new (0, 10), new (10, 10));
            Assert.Equal(20, path.Length);

            path = new TerrainPath(new(0, 0), new(10, 10));
            Assert.Equal(14.14, Math.Round(path.Length, 2));
        }

        [Fact]
        public void TerrainPath_Distance()
        {
            var path = new TerrainPath(new(0, 0), new(0, 10));
            Assert.Equal(5, path.Distance(new(5, 5)));
            Assert.Equal(5, path.Distance(new(-5, 5)));
            Assert.Equal(5, path.Distance(new(0, 15)));
            Assert.Equal(5, path.Distance(new(0, -5)));
        }

        [Fact]
        public void TerrainPath_NearestPoint()
        {
            var path = new TerrainPath(new(0, 0), new(0, 10));
            Assert.Equal(new (0, 5), path.NearestPointBoundary(new (5, 5)));
            Assert.Equal(new (0, 5), path.NearestPointBoundary(new (-5, 5)));
            Assert.Equal(new (0, 10), path.NearestPointBoundary(new (0, 15)));
            Assert.Equal(new (0, 0), path.NearestPointBoundary(new (0, -5)));
        }

        [Fact]
        public void RoadsBuilder_PreventSplines()
        {
            Assert.Equal(new List<TerrainPoint>()
            {
                new TerrainPoint(0,0),
                new TerrainPoint(8,0),
                new TerrainPoint(10,0),
                new TerrainPoint(10,2),
                new TerrainPoint(10,10)
            },
            TerrainPath.PreventSplines(new List<TerrainPoint>()
            {
                new TerrainPoint(0,0),
                new TerrainPoint(10,0),
                new TerrainPoint(10,10)
            }, 2));

            Assert.Equal(new List<TerrainPoint>()
            {
                new TerrainPoint(0,0),
                new TerrainPoint(8,0),
                new TerrainPoint(10,0),
                new TerrainPoint(10,2),
                new TerrainPoint(10,10)
            },
            TerrainPath.PreventSplines(new List<TerrainPoint>()
            {
                new TerrainPoint(0,0),
                new TerrainPoint(8,0),
                new TerrainPoint(10,0),
                new TerrainPoint(10,2),
                new TerrainPoint(10,10)
            }, 2));

            Assert.Equal(new List<TerrainPoint>()
            {
                new TerrainPoint(0,0),
                new TerrainPoint(8,0),
                new TerrainPoint(10,0),
                new TerrainPoint(10,2),
                new TerrainPoint(10,8),
                new TerrainPoint(10,10),
                new TerrainPoint(12,10),
                new TerrainPoint(18,10),
                new TerrainPoint(20,10),
                new TerrainPoint(20,12),
                new TerrainPoint(20,20)
            },
            TerrainPath.PreventSplines(new List<TerrainPoint>()
            {
                new TerrainPoint(0,0),
                new TerrainPoint(10,0),
                new TerrainPoint(10,10),
                new TerrainPoint(20,10),
                new TerrainPoint(20,20)
            }, 2));
        }

        [Fact]
        public void TerrainPath_ExtendBothEnds()
        {
            var path = new TerrainPath(new(0, 0), new(0, 10), new(10, 10));
            var pathEx = path.ExtendBothEnds(5);
            Assert.Equal(30, pathEx.Length);
            Assert.Equal(new TerrainPoint[] { new(0, -5), new(0, 10), new(15, 10) }, pathEx.Points);

            path = new TerrainPath(new(0, 5), new(0, 15));
            pathEx = path.ExtendBothEnds(5);
            Assert.Equal(20, pathEx.Length);
            Assert.Equal(new TerrainPoint[] { new(0, 0), new(0, 20) }, pathEx.Points);
        }

        [Fact]
        public void TerrainPath_GetNormalizedVectorAtIndex()
        {
            var path = new TerrainPath(new(0, 0), new(0, 10), new(10, 10), new (20,10));
            Assert.Equal(new Vector2(0, 1), path.GetNormalizedVectorAtIndex(0));
            Assert.Equal(new Vector2(0.70710677f, 0.70710677f), path.GetNormalizedVectorAtIndex(1));
            Assert.Equal(new Vector2(1, 0), path.GetNormalizedVectorAtIndex(2));

            path = new TerrainPath(new(0, 0), new(0, -10), new(-10, -10), new(-20, -10));
            Assert.Equal(new Vector2(0, -1), path.GetNormalizedVectorAtIndex(0));
            Assert.Equal(new Vector2(-0.70710677f, -0.70710677f), path.GetNormalizedVectorAtIndex(1));
            Assert.Equal(new Vector2(-1, 0), path.GetNormalizedVectorAtIndex(2));
        }
        
    }
}
