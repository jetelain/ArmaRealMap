using System.Numerics;
using ClipperLib;
using GameRealisticMap.Geometries;
using Pmad.Geometry;

namespace GameRealisticMap.Test.Geometries
{
    public class TerrainPointTest
    {
        [Fact]
        public void TerrainPoint_ToIntPointPrecision()
        {
            var p = new TerrainPoint(1.23456789f, 9.87654321f);
            Assert.Equal(1.23456789f, p.X);
            Assert.Equal(9.87654321f, p.Y);

            p = p.ToIntPointPrecision();
            Assert.Equal(1.234f, p.X);
            Assert.Equal(9.876f, p.Y);
        }

        [Fact]
        public void TerrainPoint_Constructor_WithCoordinates()
        {
            var p = new TerrainPoint(1.23456789f, 9.87654321f);
            Assert.Equal(1.23456789f, p.X);
            Assert.Equal(9.87654321f, p.Y);
        }

        [Fact]
        public void TerrainPoint_Constructor_WithVector()
        {
            var vector = new Vector2(1.23456789f, 9.87654321f);
            var p = new TerrainPoint(vector);
            Assert.Equal(vector, p.Vector);
        }

        [Fact]
        public void TerrainPoint_Constructor_WithIntPoint()
        {
            var intPoint = new IntPoint(123456789, 987654321);
            var p = new TerrainPoint(intPoint);
            Assert.Equal(123456.789f, p.X);
            Assert.Equal(987654.321f, p.Y);
        }

        [Fact]
        public void TerrainPoint_IsEmpty()
        {
            var p = TerrainPoint.Empty;
            Assert.True(p.IsEmpty);
        }

        [Fact]
        public void TerrainPoint_ToIntPoint()
        {
            var p = new TerrainPoint(1.23456789f, 9.87654321f);
            var intPoint = p.ToIntPoint();
            Assert.Equal(1234, intPoint.X);
            Assert.Equal(9876, intPoint.Y);
        }

        [Fact]
        public void TerrainPoint_Equals()
        {
            var p1 = new TerrainPoint(1.23456789f, 9.87654321f);
            var p2 = new TerrainPoint(1.23456789f, 9.87654321f);
            Assert.True(p1.Equals(p2));
        }

        [Fact]
        public void TerrainPoint_FromPmadGeometry()
        {
            var vector2D = new Vector2D(1.23456789, 9.87654321);
            var p = TerrainPoint.FromPmadGeometry(vector2D);
            Assert.Equal(1.23456789f, p.X);
            Assert.Equal(9.87654321f, p.Y);
        }

        [Fact]
        public void TerrainPoint_AdditionOperator()
        {
            var p = new TerrainPoint(1.23456789f, 9.87654321f);
            var vector = new Vector2(1.0f, 1.0f);
            var result = p + vector;
            Assert.Equal(2.23456789f, result.X);
            Assert.Equal(10.87654321f, result.Y);
        }

        [Fact]
        public void TerrainPoint_SubtractionOperator()
        {
            var p = new TerrainPoint(1.23456789f, 9.87654321f);
            var vector = new Vector2(1.0f, 1.0f);
            var result = p - vector;
            Assert.Equal(0.23456789f, result.X, 6);
            Assert.Equal(8.87654321f, result.Y, 6);
        }
    }
}
