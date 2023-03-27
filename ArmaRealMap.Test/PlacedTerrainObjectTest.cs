using System;
using System.Numerics;
using GameRealisticMap.Geometries;
using Xunit;

namespace ArmaRealMap.Test
{
    public class PlacedTerrainObjectTest
    {
        [Fact]
        public void PlacedTerrainObject_ToWrpObject()
        {
            var lib = new ModelInfoLibrary();
            var model = new ModelInfo() { Path = "test.p3d", Name = "test" };
            lib.Models.Add(model);

            var objInitial = new PlacedTerrainObject(model, new TerrainPoint(256, 512), 128, ElevationMode.Absolute);
            Assert.Equal(@"""test"";200256.000;512.000;0.000;0.000;0.000;1.000;128.000;", objInitial.ToTerrainBuilderCSV());
            var wrp = objInitial.ToWrpObject(1);
            // TODO: Assert Matrix
            var objBack = new PlacedTerrainObject(wrp, lib);
            Assert.Equal(@"""test"";200256.000;512.000;0.000;0.000;0.000;1.000;128.000;", objBack.ToTerrainBuilderCSV());
            
            //

            objInitial = new PlacedTerrainObject(model, new TerrainPoint(256, 512), 128, ElevationMode.Absolute, 45);
            Assert.Equal(@"""test"";200256.000;512.000;45.000;0.000;0.000;1.000;128.000;", objInitial.ToTerrainBuilderCSV());
            wrp = objInitial.ToWrpObject(1);
            // TODO: Assert Matrix
            objBack = new PlacedTerrainObject(wrp, lib);
            Assert.Equal(@"""test"";200256.000;512.000;45.000;0.000;0.000;1.000;128.000;", objBack.ToTerrainBuilderCSV());

            //

            objInitial = new PlacedTerrainObject(model, new TerrainPoint(256, 512), 128, ElevationMode.Absolute, 0, 45, 0);
            Assert.Equal(@"""test"";200256.000;512.000;0.000;45.000;0.000;1.000;128.000;", objInitial.ToTerrainBuilderCSV());
            wrp = objInitial.ToWrpObject(1);
            // TODO: Assert Matrix
            objBack = new PlacedTerrainObject(wrp, lib);
            Assert.Equal(@"""test"";200256.000;512.000;0.000;45.000;0.000;1.000;128.000;", objBack.ToTerrainBuilderCSV());

            //

            objInitial = new PlacedTerrainObject(model, new TerrainPoint(256, 512), 128, ElevationMode.Absolute, 90, 65, 35);
            Assert.Equal(@"""test"";200256.000;512.000;90.000;65.000;35.000;1.000;128.000;", objInitial.ToTerrainBuilderCSV());
            wrp = objInitial.ToWrpObject(1);
            // TODO: Assert Matrix
            objBack = new PlacedTerrainObject(wrp, lib);
            Assert.Equal(@"""test"";200256.000;512.000;90.000;65.000;35.000;1.000;128.000;", objBack.ToTerrainBuilderCSV());
            
            //

            objInitial = new PlacedTerrainObject(model, new TerrainPoint(17929, 43049.27f), 292.99362f, ElevationMode.Absolute, -160.467f, 4.071f, 0f);
            Assert.Equal(@"""test"";217929.000;43049.270;-160.467;4.071;0.000;1.000;292.994;", objInitial.ToTerrainBuilderCSV());
            wrp = objInitial.ToWrpObject(1);
            Assert.Equal(Round(new Matrix4x4(-0.94244903f, 0.023736361f, 0.33350623f, 0, 0, 0.9974768f, -0.07099259f, 0, -0.33434984f, -0.0669069f, -0.94007105f, 0, 17929, 292.99362f, 43049.27f, 1)), Round(wrp.Transform.Matrix));
            objBack = new PlacedTerrainObject(wrp, lib);
            Assert.Equal(@"""test"";217929.000;43049.270;-160.467;4.071;0.000;1.000;292.994;", objBack.ToTerrainBuilderCSV());
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
