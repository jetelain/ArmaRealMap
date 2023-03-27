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

        //(2,1,5), B=(1,2,5) and C=(2,1,3) and D=(2,1,2)

    }
}
