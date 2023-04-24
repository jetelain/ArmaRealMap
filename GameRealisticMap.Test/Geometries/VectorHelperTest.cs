using System.Numerics;
using GameRealisticMap.Geometries;
using Xunit;

namespace GameRealisticMap.Test.Geometries
{
    public class VectorHelperTest
    {
        [Fact]
        public void VectorHelper_HasIntersection_Vector2()
        {
            Vector2 result;

            Assert.True(VectorHelper.HasIntersection(new Vector2(0, 0), new Vector2(1, 1), new Vector2(0, 1), new Vector2(1, 0), out result));
            Assert.Equal(new Vector2(0.5f, 0.5f), result);

            Assert.True(VectorHelper.HasIntersection(new Vector2(1, 1), new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 0), out result));
            Assert.Equal(new Vector2(0.5f, 0.5f), result);

            Assert.True(VectorHelper.HasIntersection(new Vector2(0, 0), new Vector2(1, 1), new Vector2(1, 0), new Vector2(0, 1), out result));
            Assert.Equal(new Vector2(0.5f, 0.5f), result);

            Assert.False(VectorHelper.HasIntersection(new Vector2(0, 0), new Vector2(1, 1), new Vector2(5, 1), new Vector2(6, 0), out _));
        }

        [Fact]
        public void VectorHelper_HasIntersection_Vector3()
        {
            Vector3 result;

            Assert.True(VectorHelper.HasIntersection(new Vector3(0, 0, 0), new Vector3(1, 1, 0), new Vector3(0, 1, 0), new Vector3(1, 0, 0), out result));
            Assert.Equal(new Vector3(0.5f, 0.5f, 0f), result);

            Assert.True(VectorHelper.HasIntersection(new Vector3(0, 0, 0), new Vector3(1, 1, 1), new Vector3(0, 1, 1), new Vector3(1, 0, 0), out result));
            Assert.Equal(new Vector3(0.5f, 0.5f, 0.5f), result);

            Assert.False(VectorHelper.HasIntersection(new Vector3(0, 0, 0), new Vector3(1, 1, 0), new Vector3(0, 1, 1), new Vector3(1, 0, 1), out _));

            Assert.False(VectorHelper.HasIntersection(new Vector3(2, 1, 5), new Vector3(1, 2, 5), new Vector3(2, 1, 3), new Vector3(2, 1, 2), out _));
        }

        [Fact]
        public void VectorHelper_GetAngleFromXAxisInDegrees()
        {
            Assert.Equal(0, VectorHelper.GetAngleFromXAxisInDegrees(new Vector2(10, 0)));
            Assert.Equal(45, VectorHelper.GetAngleFromXAxisInDegrees(new Vector2(10, 10)));
            Assert.Equal(90, VectorHelper.GetAngleFromXAxisInDegrees(new Vector2(0, 10)));
            Assert.Equal(135, VectorHelper.GetAngleFromXAxisInDegrees(new Vector2(-10, 10)));
            Assert.Equal(180, VectorHelper.GetAngleFromXAxisInDegrees(new Vector2(-10, 0)));
            Assert.Equal(-135, VectorHelper.GetAngleFromXAxisInDegrees(new Vector2(-10, -10)));
            Assert.Equal(-90, VectorHelper.GetAngleFromXAxisInDegrees(new Vector2(0, -10)));
            Assert.Equal(-45, VectorHelper.GetAngleFromXAxisInDegrees(new Vector2(10, -10)));
        }


        [Fact]
        public void VectorHelper_GetAngleFromYAxisInDegrees()
        {
            Assert.Equal(-90, VectorHelper.GetAngleFromYAxisInDegrees(new Vector2(10, 0)));
            Assert.Equal(-45, VectorHelper.GetAngleFromYAxisInDegrees(new Vector2(10, 10)));
            Assert.Equal(0, VectorHelper.GetAngleFromYAxisInDegrees(new Vector2(0, 10)));
            Assert.Equal(45, VectorHelper.GetAngleFromYAxisInDegrees(new Vector2(-10, 10)));
            Assert.Equal(90, VectorHelper.GetAngleFromYAxisInDegrees(new Vector2(-10, 0)));
            Assert.Equal(135, VectorHelper.GetAngleFromYAxisInDegrees(new Vector2(-10, -10)));
            Assert.Equal(-180, VectorHelper.GetAngleFromYAxisInDegrees(new Vector2(0, -10)));
            Assert.Equal(-135, VectorHelper.GetAngleFromYAxisInDegrees(new Vector2(10, -10)));
        }
    }
}
