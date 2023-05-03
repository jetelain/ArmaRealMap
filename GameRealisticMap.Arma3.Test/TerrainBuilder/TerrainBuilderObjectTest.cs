using System.Numerics;
using BIS.Core.Streams;
using BIS.WRP;
using GameRealisticMap.Arma3.TerrainBuilder;
using GameRealisticMap.ElevationModel;
using GameRealisticMap.Geometries;

namespace GameRealisticMap.Arma3.Test.TerrainBuilder
{
    public class TerrainBuilderObjectTest
    {
        private static readonly ModelInfo SignArrowDirectionF = new ModelInfo("Sign_Arrow_Direction_F", "a3\\misc_f\\Helpers\\Sign_Arrow_Direction_F.p3d", "a3_misc_f", new Vector3(0, 0.058088847f, 0.029880643f));

        [Fact]
        public void TerrainBuilderObject_ToWrpObject_Angles()
        {
            var grid = GenerateGrid01();
            var srcObjects = GenerateObjects();

            var wrpObjects = GetWrp(01).Objects.Where(o => !string.IsNullOrEmpty(o.Model)).OrderBy(o => o.Transform.TranslateX).ToList();

            for (var i = 0; i < srcObjects.Count; ++i)
            {
                var expected = wrpObjects[i];
                var actual = srcObjects[i].ToWrpObject(i, grid);
                Assert.Equal(expected.Model, actual.Model, true);
                Assert.Equal(Round(expected.Transform.Matrix), Round(actual.Transform.Matrix));
            }
        }

        [Fact]
        public void TerrainBuilderObject_ToWrpObject_Scale()
        {
            var grid = GenerateGrid01();
            var srcObjects = GenerateObjects(2);

            var wrpObjects = GetWrp(02).Objects.Where(o => !string.IsNullOrEmpty(o.Model)).OrderBy(o => o.Transform.TranslateX).ToList();

            for (var i = 0; i < srcObjects.Count; ++i)
            {
                var expected = wrpObjects[i];
                var actual = srcObjects[i].ToWrpObject(i, grid);
                Assert.Equal(expected.Model, actual.Model, true);
                Assert.Equal(Round(expected.Transform.Matrix), Round(actual.Transform.Matrix));
            }
        }

        [Fact]
        public void TerrainBuilderObject_FromWrpObject_Angles()
        {
            var grid = GenerateGrid01();
            var srcObjects = GenerateObjects();

            var wrpObjects = GetWrp(01).Objects.Where(o => !string.IsNullOrEmpty(o.Model)).OrderBy(o => o.Transform.TranslateX).ToList();

            for (var i = 0; i < srcObjects.Count; ++i)
            {
                var expected = srcObjects[i];
                var actual = new TerrainBuilderObject(SignArrowDirectionF, wrpObjects[i]);

                Assert.Equal(expected.Point.X, actual.Point.X, 4);
                Assert.Equal(expected.Point.Y, actual.Point.Y, 4);
                Assert.Equal(expected.Elevation + 20, actual.Elevation, 4);
                Assert.Equal(expected.Scale, actual.Scale, 4);

                if (i < 4)
                {
                    // There is multiple solutions : Matrix -> (Yaw/Pitch/Roll) does not map to something unique
                    // works only for few
                    Assert.Equal(expected.Yaw, actual.Yaw, 4);
                    Assert.Equal(expected.Pitch, actual.Pitch, 4);
                    Assert.Equal(expected.Roll, actual.Roll, 4);
                }
                
                var tranformBack = actual.ToWrpObject(i, grid);
                Assert.Equal(Round(wrpObjects[i].Transform.Matrix), Round(tranformBack.Transform.Matrix));
            }
        }

        [Fact]
        public void TerrainBuilderObject_FromWrpObject_Scale()
        {
            var grid = GenerateGrid01();
            var srcObjects = GenerateObjects(2);

            var wrpObjects = GetWrp(02).Objects.Where(o => !string.IsNullOrEmpty(o.Model)).OrderBy(o => o.Transform.TranslateX).ToList();

            for (var i = 0; i < srcObjects.Count; ++i)
            {
                var expected = srcObjects[i];
                var actual = new TerrainBuilderObject(SignArrowDirectionF, wrpObjects[i]);

                Assert.Equal(expected.Point.X, actual.Point.X, 4);
                Assert.Equal(expected.Point.Y, actual.Point.Y, 4);
                Assert.Equal(expected.Elevation + 20, actual.Elevation, 4);
                Assert.Equal(expected.Scale, actual.Scale, 4);

                if (i < 4)
                {
                    // There is multiple solutions : Matrix -> (Yaw/Pitch/Roll) does not map to something unique
                    // works only for few
                    Assert.Equal(expected.Yaw, actual.Yaw, 4);
                    Assert.Equal(expected.Pitch, actual.Pitch, 4);
                    Assert.Equal(expected.Roll, actual.Roll, 4);
                }

                var tranformBack = actual.ToWrpObject(i, grid);
                Assert.Equal(Round(wrpObjects[i].Transform.Matrix), Round(tranformBack.Transform.Matrix));
            }
        }

        private static EditableWrp GetWrp(int num)
        {
            return StreamHelper.Read<EditableWrp>(typeof(TerrainBuilderObjectTest).Assembly.GetManifestResourceStream($"GameRealisticMap.Arma3.Test.TerrainBuilder.world{num:00}.wrp"));
        }

        private static List<TerrainBuilderObject> GenerateObjects(float scale = 1)
        {
            var objects = new List<TerrainBuilderObject>();
            var anglesToTest = new float[] { 0, 45, 90, 135, 180, 225, 270, 315 };
            var pos = 1;
            foreach (var yaw in anglesToTest)
            {
                foreach (var pitch in anglesToTest)
                {
                    foreach (var roll in anglesToTest)
                    {
                        objects.Add(new TerrainBuilderObject(SignArrowDirectionF, new TerrainPoint(pos, 1), 0, ElevationMode.Relative, yaw, pitch, roll, scale));
                        pos++;
                    }
                }
            }

            return objects;
        }

        private static ElevationGrid GenerateGrid01()
        {
            var config = new TestMapConfig();
            var grid = new ElevationGrid(config.GridSize, config.GridCellSize);
            for (var ex = 0; ex < grid.Size; ++ex)
            {
                for (var ey = 0; ey < grid.Size; ++ey)
                {
                    grid[ex, ey] = 20;
                }
            }
            return grid;
        }

        private static Matrix4x4 Round(Matrix4x4 matrix4x4)
        {
            return new Matrix4x4(
                MathF.Round(matrix4x4.M11, 4),
                MathF.Round(matrix4x4.M12, 4),
                MathF.Round(matrix4x4.M13, 4),
                MathF.Round(matrix4x4.M14, 4),
                MathF.Round(matrix4x4.M21, 4),
                MathF.Round(matrix4x4.M22, 4),
                MathF.Round(matrix4x4.M23, 4),
                MathF.Round(matrix4x4.M24, 4),
                MathF.Round(matrix4x4.M31, 4),
                MathF.Round(matrix4x4.M32, 4),
                MathF.Round(matrix4x4.M33, 4),
                MathF.Round(matrix4x4.M34, 4),
                MathF.Round(matrix4x4.M41, 4),
                MathF.Round(matrix4x4.M42, 4),
                MathF.Round(matrix4x4.M43, 4),
                MathF.Round(matrix4x4.M44, 4)
                );
        }
    }
}
