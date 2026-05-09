using System.Numerics;
using GameRealisticMap.Geometries;

namespace GameRealisticMap.Test.Geometries
{
    public class CircleTest
    {
        [Fact]
        public void Contains_PointAtCenter_ReturnsTrue()
        {
            var circle = new Circle(new Vector2(5f, 5f), 3f);
            Assert.True(circle.Contains(new Vector2(5f, 5f)));
        }

        [Fact]
        public void Contains_PointOnEdge_ReturnsTrue()
        {
            var circle = new Circle(new Vector2(0f, 0f), 3f);
            Assert.True(circle.Contains(new Vector2(3f, 0f)));
        }

        [Fact]
        public void Contains_PointOutside_ReturnsFalse()
        {
            var circle = new Circle(new Vector2(0f, 0f), 3f);
            Assert.False(circle.Contains(new Vector2(4f, 0f)));
        }

        [Fact]
        public void FromTwoPoints_CenterIsMiddle()
        {
            var a = new Vector2(0f, 0f);
            var b = new Vector2(4f, 0f);
            var circle = Circle.FromTwoPoints(a, b);

            Assert.Equal(new Vector2(2f, 0f), circle.Center);
            Assert.Equal(2f, circle.Radius, 5);
        }

        [Fact]
        public void FromTwoPoints_ContainsBothPoints()
        {
            var a = new Vector2(1f, 2f);
            var b = new Vector2(5f, 6f);
            var circle = Circle.FromTwoPoints(a, b);

            Assert.True(circle.Contains(a));
            Assert.True(circle.Contains(b));
        }

        [Fact]
        public void FromThreePoints_ContainsAllPoints()
        {
            var a = new Vector2(0f, 0f);
            var b = new Vector2(4f, 0f);
            var c = new Vector2(2f, 3f);
            var circle = Circle.FromThreePoints(a, b, c);

            Assert.True(circle.Contains(a));
            Assert.True(circle.Contains(b));
            Assert.True(circle.Contains(c));
        }

        [Fact]
        public void CreateFromWelzlStable_Empty_ReturnsZeroCircle()
        {
            var circle = Circle.CreateFromWelzlStable(Array.Empty<Vector2>());
            Assert.Equal(Vector2.Zero, circle.Center);
            Assert.Equal(0f, circle.Radius);
        }

        [Fact]
        public void CreateFromWelzlStable_SinglePoint_ReturnsZeroRadiusAtPoint()
        {
            var pt = new Vector2(3f, 4f);
            var circle = Circle.CreateFromWelzlStable(new[] { pt });
            Assert.Equal(pt, circle.Center);
            Assert.Equal(0f, circle.Radius);
        }

        [Fact]
        public void CreateFromWelzlStable_ContainsAllPoints()
        {
            var points = new[]
            {
                new Vector2(0f, 0f),
                new Vector2(10f, 0f),
                new Vector2(5f, 8f),
                new Vector2(3f, 5f),
            };

            var circle = Circle.CreateFromWelzlStable(points);

            foreach (var p in points)
            {
                Assert.True(circle.Contains(p), $"Circle should contain point {p}");
            }
        }
    }
}
