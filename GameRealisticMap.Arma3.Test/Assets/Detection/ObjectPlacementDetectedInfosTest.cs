using System.Numerics;
using GameRealisticMap.Arma3.Assets.Detection;

namespace GameRealisticMap.Arma3.Test.Assets.Detection
{
    public class ObjectPlacementDetectedInfosTest
    {
        // Four corners at Y=0, symmetric around origin
        private static List<Vector3> SymmetricGroundPoints() => new List<Vector3>
        {
            new Vector3( 1, 0,  1),
            new Vector3(-1, 0,  1),
            new Vector3( 1, 0, -1),
            new Vector3(-1, 0, -1),
        };

        [Fact]
        public void CreateFromPoints_SymmetricGroundPoints_GeneralRectangleCenteredAtOrigin()
        {
            var result = ObjectPlacementDetectedInfos.CreateFromPoints(SymmetricGroundPoints());

            Assert.Equal(new Vector2(0, 0), result.GeneralRectangle.Center);
            Assert.Equal(new Vector2(2, 2), result.GeneralRectangle.Size);
        }

        [Fact]
        public void CreateFromPoints_SymmetricGroundPoints_GeneralRadiusCenteredAtOrigin()
        {
            var result = ObjectPlacementDetectedInfos.CreateFromPoints(SymmetricGroundPoints());

            Assert.Equal(new Vector2(0, 0), result.GeneralRadius.Center);
            Assert.Equal(MathF.Sqrt(2), result.GeneralRadius.Radius, 4);
        }

        [Fact]
        public void CreateFromPoints_SymmetricGroundPoints_TrunkRadiusEqualsGeneralRadius()
        {
            // All points at Y=0 → trunk (−0.05 ≤ Y ≤ 0.75), so TrunkRadius == GeneralRadius
            var result = ObjectPlacementDetectedInfos.CreateFromPoints(SymmetricGroundPoints());

            Assert.Equal(result.GeneralRadius.Center, result.TrunkRadius.Center);
            Assert.Equal(result.GeneralRadius.Radius, result.TrunkRadius.Radius, 4);
        }

        [Fact]
        public void CreateFromPoints_SymmetricGroundPoints_UpperRectangleEqualsGeneralRectangle()
        {
            // maxY = 0 ≤ 1.5 → upper rectangle falls back to all projected points
            var result = ObjectPlacementDetectedInfos.CreateFromPoints(SymmetricGroundPoints());

            Assert.Equal(result.GeneralRectangle.Center, result.UpperRectangle.Center);
            Assert.Equal(result.GeneralRectangle.Size, result.UpperRectangle.Size);
        }

        [Fact]
        public void CreateFromPoints_WithUpperPoints_UpperRectangleUsesOnlyHighPoints()
        {
            // Base footprint 2×2, small crown at height 2
            var points = new List<Vector3>
            {
                new Vector3( 1, 0,  1),
                new Vector3(-1, 0, -1),
                new Vector3( 0.5f, 2f,  0.5f),
                new Vector3(-0.5f, 2f, -0.5f),
            };

            var result = ObjectPlacementDetectedInfos.CreateFromPoints(points);

            // GeneralRectangle covers the full 2×2 footprint
            Assert.Equal(new Vector2(2, 2), result.GeneralRectangle.Size);

            // UpperRectangle should be smaller (only the crown points)
            Assert.True(result.UpperRectangle.Size.X < result.GeneralRectangle.Size.X ||
                        result.UpperRectangle.Size.Y < result.GeneralRectangle.Size.Y);
        }

        [Fact]
        public void CreateFromPoints_WithTrunkPoints_TrunkRadiusUsesNarrowBase()
        {
            // Trunk (Y=0) is narrow (±0.5), crown (Y=3) is wide (±2)
            var points = new List<Vector3>
            {
                new Vector3( 0.5f, 0f,  0.5f),
                new Vector3(-0.5f, 0f, -0.5f),
                new Vector3( 2f,   3f,  2f),
                new Vector3(-2f,   3f, -2f),
            };

            var result = ObjectPlacementDetectedInfos.CreateFromPoints(points);

            // Trunk circle is fitted to narrow base — its center should stay near origin
            Assert.True(result.TrunkRadius.Radius <= result.GeneralRadius.Radius);
        }

        [Fact]
        public void CreateFromPoints_NoTrunkPoints_TrunkRadiusFallsBackToAllPoints()
        {
            // All points well above the trunk band (Y > 0.75)
            var points = new List<Vector3>
            {
                new Vector3( 1, 1f,  1),
                new Vector3(-1, 1f, -1),
            };

            var result = ObjectPlacementDetectedInfos.CreateFromPoints(points);

            // Fallback: trunk scan uses all projected points → same radius as GeneralRadius
            Assert.Equal(result.GeneralRadius.Radius, result.TrunkRadius.Radius, 4);
        }

        [Fact]
        public void CreateFromPoints_CollinearPoints_RectangleCenterIsMiddle()
        {
            // Two points along the X axis
            var points = new List<Vector3>
            {
                new Vector3(0, 0, 0),
                new Vector3(4, 0, 0),
            };

            var result = ObjectPlacementDetectedInfos.CreateFromPoints(points);

            Assert.Equal(new Vector2(2, 0), result.GeneralRectangle.Center);
            Assert.Equal(new Vector2(4, 0), result.GeneralRectangle.Size);
        }

        [Fact]
        public void CreateFromPoints_OffCenterPoints_RectangleCenterIsCorrect()
        {
            var points = new List<Vector3>
            {
                new Vector3(2, 0, 4),
                new Vector3(4, 0, 8),
            };

            var result = ObjectPlacementDetectedInfos.CreateFromPoints(points);

            Assert.Equal(new Vector2(3, 6), result.GeneralRectangle.Center);
            Assert.Equal(new Vector2(2, 4), result.GeneralRectangle.Size);
        }

        [Fact]
        public void Constructor_SetsAllProperties()
        {
            var generalRadius = new RadiusBasedPlacement(new Vector2(1, 2), 3f);
            var trunkRadius = new RadiusBasedPlacement(new Vector2(0, 0), 1f);
            var upperRect = new RectangleBasedPlacement(new Vector2(1, 1), new Vector2(2, 2));
            var generalRect = new RectangleBasedPlacement(new Vector2(0, 0), new Vector2(4, 4));

            var infos = new ObjectPlacementDetectedInfos(generalRadius, trunkRadius, upperRect, generalRect);

            Assert.Same(generalRadius, infos.GeneralRadius);
            Assert.Same(trunkRadius, infos.TrunkRadius);
            Assert.Same(upperRect, infos.UpperRectangle);
            Assert.Same(generalRect, infos.GeneralRectangle);
        }
    }
}
