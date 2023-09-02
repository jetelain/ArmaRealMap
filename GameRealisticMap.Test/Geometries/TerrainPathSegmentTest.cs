using GameRealisticMap.Geometries;

namespace GameRealisticMap.Test.Geometries
{
    public class TerrainPathSegmentTest
    {
        [Fact]
        public void FromPath_BasicSquare()
        {
            var path = new TerrainPath(new(0,0), new(10,0), new (10,10), new(0,10), new (0,0));
            Assert.True(path.IsClosed);
            Assert.True(path.IsCounterClockWise);
            var segments = TerrainPathSegment.FromPath(path);
            Assert.Equal(4, segments.Count);

            var segment = segments[0];
            Assert.Equal(new TerrainPoint[] { new(0, 0), new (10, 0) }, segment.Points);
            Assert.Equal(10, segment.Length);
            Assert.True(segment.HasNext);
            Assert.Equal(90, segment.AngleWithNext);
            Assert.False(segment.IsClosed);

            segment = segments[1];
            Assert.Equal(new TerrainPoint[] { new(10, 0), new(10, 10) }, segment.Points);
            Assert.Equal(10, segment.Length);
            Assert.True(segment.HasNext);
            Assert.Equal(90, segment.AngleWithNext);
            Assert.False(segment.IsClosed);

            segment = segments[2];
            Assert.Equal(new TerrainPoint[] { new(10, 10), new(0, 10) }, segment.Points);
            Assert.Equal(10, segment.Length);
            Assert.True(segment.HasNext);
            Assert.Equal(90, segment.AngleWithNext);
            Assert.False(segment.IsClosed);

            segment = segments[3];
            Assert.Equal(new TerrainPoint[] { new(0, 10), new(0, 0) }, segment.Points);
            Assert.Equal(10, segment.Length);
            Assert.True(segment.HasNext);
            Assert.Equal(90, segment.AngleWithNext);
            Assert.False(segment.IsClosed);
        }

        [Fact]
        public void FromPath_BasicSquare_Clockwise()
        {
            var path = new TerrainPath(new(0, 0), new(0, 10), new(10, 10), new(10, 0), new(0, 0));
            Assert.True(path.IsClosed);
            Assert.False(path.IsCounterClockWise);
            var segments = TerrainPathSegment.FromPath(path);
            Assert.Equal(4, segments.Count);

            var segment = segments[0];
            Assert.Equal(new TerrainPoint[] { new(0, 0), new(0, 10) }, segment.Points);
            Assert.Equal(10, segment.Length);
            Assert.True(segment.HasNext);
            Assert.Equal(-90, segment.AngleWithNext);
            Assert.False(segment.IsClosed);

            segment = segments[1];
            Assert.Equal(new TerrainPoint[] { new(0, 10), new(10, 10) }, segment.Points);
            Assert.Equal(10, segment.Length);
            Assert.True(segment.HasNext);
            Assert.Equal(-90, segment.AngleWithNext);
            Assert.False(segment.IsClosed);

            segment = segments[2];
            Assert.Equal(new TerrainPoint[] { new(10, 10), new(10, 0) }, segment.Points);
            Assert.Equal(10, segment.Length);
            Assert.True(segment.HasNext);
            Assert.Equal(-90, segment.AngleWithNext);
            Assert.False(segment.IsClosed);

            segment = segments[3];
            Assert.Equal(new TerrainPoint[] { new(10, 0), new(0, 0) }, segment.Points);
            Assert.Equal(10, segment.Length);
            Assert.True(segment.HasNext);
            Assert.Equal(-90, segment.AngleWithNext);
            Assert.False(segment.IsClosed);
        }


        [Fact]
        public void FromPath_BasicLine()
        {
            var path = new TerrainPath(new(0, 0), new(0, 10), new(0, 20), new(0, 30), new(0, 40));
            Assert.False(path.IsClosed);
            Assert.Equal(40, path.Length);
            var segment = Assert.Single(TerrainPathSegment.FromPath(path));

            Assert.Equal(new TerrainPoint[] { new(0, 0), new(0, 10), new(0, 20), new(0, 30), new(0, 40) }, segment.Points);
            Assert.Equal(40, segment.Length);
            Assert.False(segment.HasNext);
            Assert.False(segment.IsClosed);

            path = new TerrainPath(new(0, 0), new(0, 10));
            Assert.False(path.IsClosed);
            Assert.Equal(10, path.Length);
            segment = Assert.Single(TerrainPathSegment.FromPath(path));

            Assert.Equal(new TerrainPoint[] { new(0, 0), new(0, 10) }, segment.Points);
            Assert.Equal(10, segment.Length);
            Assert.False(segment.HasNext);
            Assert.False(segment.IsClosed);
        }

        [Fact]
        public void FromPath_OpenSquare()
        {
            var path = new TerrainPath(new(0, 0), new(5, 0), new(10, 0), new(10, 10), new(5, 10), new(0, 10));
            Assert.False(path.IsClosed);
            var segments = TerrainPathSegment.FromPath(path);
            Assert.Equal(3, segments.Count);

            var segment = segments[0];
            Assert.Equal(new TerrainPoint[] { new(0, 0), new(5, 0), new(10, 0) }, segment.Points);
            Assert.Equal(10, segment.Length);
            Assert.True(segment.HasNext);
            Assert.Equal(90, segment.AngleWithNext);
            Assert.False(segment.IsClosed);

            segment = segments[1];
            Assert.Equal(new TerrainPoint[] { new(10, 0), new(10, 10) }, segment.Points);
            Assert.Equal(10, segment.Length);
            Assert.True(segment.HasNext);
            Assert.Equal(90, segment.AngleWithNext);
            Assert.False(segment.IsClosed);

            segment = segments[2];
            Assert.Equal(new TerrainPoint[] { new(10, 10), new(5, 10), new(0, 10) }, segment.Points);
            Assert.Equal(10, segment.Length);
            Assert.False(segment.HasNext);
            Assert.False(segment.IsClosed);
        }

        [Fact]
        public void FromPath_ShiftSquare()
        {
            var path = new TerrainPath(new(5, 0), new(10, 0), new(10, 10), new(0, 10), new(0, 5), new(0, 0), new(5, 0));
            Assert.True(path.IsClosed);
            var segments = TerrainPathSegment.FromPath(path);
            Assert.Equal(4, segments.Count);

            var segment = segments[0];
            Assert.Equal(new TerrainPoint[] { new(0, 0), new(5, 0), new(10, 0) }, segment.Points);
            Assert.Equal(10, segment.Length);
            Assert.True(segment.HasNext);
            Assert.Equal(90, segment.AngleWithNext);
            Assert.False(segment.IsClosed);

            segment = segments[1];
            Assert.Equal(new TerrainPoint[] { new(10, 0), new(10, 10) }, segment.Points);
            Assert.Equal(10, segment.Length);
            Assert.True(segment.HasNext);
            Assert.Equal(90, segment.AngleWithNext);
            Assert.False(segment.IsClosed);

            segment = segments[2];
            Assert.Equal(new TerrainPoint[] { new(10, 10), new(0, 10) }, segment.Points);
            Assert.Equal(10, segment.Length);
            Assert.True(segment.HasNext);
            Assert.Equal(90, segment.AngleWithNext);
            Assert.False(segment.IsClosed);

            segment = segments[3];
            Assert.Equal(new TerrainPoint[] { new(0, 10), new(0, 5), new(0, 0) }, segment.Points);
            Assert.Equal(10, segment.Length);
            Assert.True(segment.HasNext);
            Assert.Equal(90, segment.AngleWithNext);
            Assert.False(segment.IsClosed);
        }
    }
}
