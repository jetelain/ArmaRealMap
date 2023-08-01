using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameRealisticMap.Geometries;
using Xunit;

namespace GameRealisticMap.Test.Geometries
{
    public class FollowPathTest
    {

        [Fact]
        public void FollowPath_Move()
        {
            var follow = new FollowPath(
                new TerrainPoint(0, 0),
                new TerrainPoint(0, 4),
                new TerrainPoint(0, 8),
                new TerrainPoint(0, 12));

            Assert.Equal(new TerrainPoint(0, 0), follow.Current);
            Assert.Equal(1, follow.Index);
            Assert.True(follow.Move(3f));
            Assert.Equal(new TerrainPoint(0, 3), follow.Current);
            Assert.Equal(1, follow.Index);
            Assert.True(follow.Move(3f));
            Assert.Equal(new TerrainPoint(0, 6), follow.Current);
            Assert.Equal(2, follow.Index);
            Assert.True(follow.Move(3f));
            Assert.Equal(new TerrainPoint(0, 9), follow.Current);
            Assert.Equal(3, follow.Index);
            Assert.True(follow.Move(3f));
            Assert.Equal(new TerrainPoint(0, 12), follow.Current);
            Assert.Equal(3, follow.Index);
            Assert.False(follow.Move(3f));

            follow = new FollowPath(
                new TerrainPoint(0, 0),
                new TerrainPoint(4, 0),
                new TerrainPoint(8, 0),
                new TerrainPoint(12, 0));

            Assert.Equal(new TerrainPoint(0, 0), follow.Current);
            Assert.Equal(1, follow.Index);
            Assert.True(follow.Move(2f));
            Assert.Equal(new TerrainPoint(2, 0), follow.Current);
            Assert.Equal(1, follow.Index);
            Assert.True(follow.Move(2f));
            Assert.Equal(new TerrainPoint(4, 0), follow.Current);
            Assert.Equal(1, follow.Index);
            Assert.True(follow.Move(2f));
            Assert.Equal(new TerrainPoint(6, 0), follow.Current);
            Assert.Equal(2, follow.Index);
            Assert.True(follow.Move(2f));
            Assert.Equal(new TerrainPoint(8, 0), follow.Current);
            Assert.Equal(2, follow.Index);
            Assert.True(follow.Move(2f));
            Assert.Equal(new TerrainPoint(10, 0), follow.Current);
            Assert.Equal(3, follow.Index);
            Assert.True(follow.Move(2f));
            Assert.Equal(new TerrainPoint(12, 0), follow.Current);
            Assert.Equal(3, follow.Index);
            Assert.False(follow.Move(2f));

            follow = new FollowPath(
                new TerrainPoint(0, 0),
                new TerrainPoint(4, 0),
                new TerrainPoint(4, 4),
                new TerrainPoint(0, 4));

            Assert.Equal(new TerrainPoint(0, 0), follow.Current);
            Assert.True(follow.Move(2f));
            Assert.Equal(new TerrainPoint(2, 0), follow.Current);
            Assert.True(follow.Move(2f));
            Assert.Equal(new TerrainPoint(4, 0), follow.Current);
            Assert.True(follow.Move(2f));
            Assert.Equal(new TerrainPoint(4, 2), follow.Current);
            Assert.True(follow.Move(2f));
            Assert.Equal(new TerrainPoint(4, 4), follow.Current);
            Assert.True(follow.Move(2f));
            Assert.Equal(new TerrainPoint(2, 4), follow.Current);
            Assert.True(follow.Move(2f));
            Assert.Equal(new TerrainPoint(0, 4), follow.Current);
            Assert.False(follow.Move(2f));
        }

        [Fact]
        public void FollowPath_KeepRightAngles()
        {
            var follow = new FollowPath(
                new TerrainPoint(0, 0),
                new TerrainPoint(0, 4),
                new TerrainPoint(4, 4),
                new TerrainPoint(4, 0));
            follow.KeepRightAngles = true;

            Assert.Null(follow.Previous);
            Assert.Equal(new TerrainPoint(0, 0), follow.Current);
            Assert.False(follow.IsAfterRightAngle);
            Assert.False(follow.IsLast);
            Assert.Equal(1, follow.Index);

            Assert.True(follow.Move(3f));
            Assert.Equal(new TerrainPoint(0, 0), follow.Previous);
            Assert.Equal(new TerrainPoint(0, 3), follow.Current);
            Assert.False(follow.IsAfterRightAngle);
            Assert.False(follow.IsLast);
            Assert.Equal(1, follow.Index);

            Assert.True(follow.Move(3f));
            Assert.Equal(new TerrainPoint(0, 3), follow.Previous);
            Assert.Equal(new TerrainPoint(0, 4), follow.Current);
            Assert.True(follow.IsAfterRightAngle);
            Assert.False(follow.IsLast);
            Assert.Equal(1, follow.Index);

            Assert.True(follow.Move(3f));
            Assert.Equal(new TerrainPoint(0, 4), follow.Previous);
            Assert.Equal(new TerrainPoint(3, 4), follow.Current);
            Assert.False(follow.IsAfterRightAngle);
            Assert.False(follow.IsLast);
            Assert.Equal(2, follow.Index);

            Assert.True(follow.Move(3f));
            Assert.Equal(new TerrainPoint(3, 4), follow.Previous);
            Assert.Equal(new TerrainPoint(4, 4), follow.Current);
            Assert.True(follow.IsAfterRightAngle);
            Assert.False(follow.IsLast);
            Assert.Equal(2, follow.Index);

            Assert.True(follow.Move(3f));
            Assert.Equal(new TerrainPoint(4, 4), follow.Previous);
            Assert.Equal(new TerrainPoint(4, 1), follow.Current);
            Assert.False(follow.IsAfterRightAngle);
            Assert.False(follow.IsLast);
            Assert.Equal(3, follow.Index);

            Assert.True(follow.Move(3f));
            Assert.Equal(new TerrainPoint(4, 1), follow.Previous);
            Assert.Equal(new TerrainPoint(4, 0), follow.Current);
            Assert.False(follow.IsAfterRightAngle);
            Assert.True(follow.IsLast);
            Assert.Equal(3, follow.Index);

            Assert.False(follow.Move(3f));
        }


        [Fact]
        public void FollowPath_KeepRightAngles_ExactMatch()
        {
            var follow = new FollowPath(
                new TerrainPoint(0, 0),
                new TerrainPoint(0, 10),
                new TerrainPoint(10, 10));
            follow.KeepRightAngles = true;

            Assert.Null(follow.Previous);
            Assert.Equal(new TerrainPoint(0, 0), follow.Current);
            Assert.False(follow.IsAfterRightAngle);
            Assert.False(follow.IsLast);
            Assert.Equal(1, follow.Index);

            Assert.True(follow.Move(5f));
            Assert.Equal(new TerrainPoint(0, 0), follow.Previous);
            Assert.Equal(new TerrainPoint(0, 5), follow.Current);
            Assert.False(follow.IsAfterRightAngle);
            Assert.False(follow.IsLast);
            Assert.Equal(1, follow.Index);

            Assert.True(follow.Move(5f));
            Assert.Equal(new TerrainPoint(0, 5), follow.Previous);
            Assert.Equal(new TerrainPoint(0, 10), follow.Current);
            Assert.False(follow.IsAfterRightAngle);
            Assert.False(follow.IsLast);
            Assert.Equal(1, follow.Index);

            Assert.True(follow.Move(5f));
            Assert.Equal(new TerrainPoint(0, 10), follow.Previous);
            Assert.Equal(new TerrainPoint(5, 10), follow.Current);
            Assert.False(follow.IsAfterRightAngle);
            Assert.False(follow.IsLast);
            Assert.Equal(2, follow.Index);
            Assert.True(follow.Move(5f));

            Assert.Equal(new TerrainPoint(5, 10), follow.Previous);
            Assert.Equal(new TerrainPoint(10, 10), follow.Current);
            Assert.False(follow.IsAfterRightAngle);
            Assert.False(follow.IsLast);
            Assert.Equal(2, follow.Index);
        }
    }
}
