using System;
using System.Globalization;
using System.Numerics;
using ArmaRealMap.ElevationModel;
using ArmaRealMap.Geometries;
using ArmaRealMap.Libraries;
using BIS.WRP;

namespace ArmaRealMap
{
    public sealed class PlacedTerrainObject
    {
        private readonly TerrainPoint point;
        private readonly float elevation;
        private readonly float yaw;
        private readonly float pitch;
        private readonly float roll;
        private readonly float scale;
        private readonly ElevationMode elevationMode;
        private readonly ModelInfo model;

        public PlacedTerrainObject(ModelInfo model, TerrainPoint point, float elevation = 0f, ElevationMode elevationMode = ElevationMode.Relative, float yaw = 0f, float pitch = 0f, float roll = 0f, float scale = 1f)
        {
            this.point = point;
            this.elevation = elevation;
            this.yaw = yaw;
            this.pitch = pitch;
            this.roll = roll;
            this.scale = scale;
            this.elevationMode = elevationMode;
            this.model = model;
        }

        public PlacedTerrainObject(ElevationMode elevationMode, string terrainBuilderCsv, ModelInfoLibrary library)
            : this(elevationMode, terrainBuilderCsv.Split(';'), library)
        {

        }

        public PlacedTerrainObject(ElevationMode elevationMode, string[] terrainBuilderCsv, ModelInfoLibrary library)
        {
            model = library.ResolveByName(terrainBuilderCsv[0].Trim('"'));
            point = new TerrainPoint(
                        (float)(double.Parse(terrainBuilderCsv[1], CultureInfo.InvariantCulture) - 200000),
                        (float)(double.Parse(terrainBuilderCsv[2], CultureInfo.InvariantCulture)));
            yaw = float.Parse(terrainBuilderCsv[3], CultureInfo.InvariantCulture);
            pitch = float.Parse(terrainBuilderCsv[4], CultureInfo.InvariantCulture);
            roll = float.Parse(terrainBuilderCsv[5], CultureInfo.InvariantCulture);
            scale = float.Parse(terrainBuilderCsv[6], CultureInfo.InvariantCulture);
            elevation = float.Parse(terrainBuilderCsv[7], CultureInfo.InvariantCulture);
            this.elevationMode = elevationMode;
        }

        public PlacedTerrainObject(ElevationMode elevationMode, TerrainObject terrainObject, ModelInfoLibrary library)
        {
            model = library.ResolveByName(terrainObject.Object.Name);
            point = TerrainObject.Transform(terrainObject.Object, terrainObject.Center, terrainObject.Angle);
            yaw = -terrainObject.Angle;
            pitch = terrainObject.Pitch ?? 0f;
            roll = terrainObject.Roll ?? 0f;
            scale = 1f;
            elevation = (terrainObject.Elevation ?? 0f) + terrainObject.Object.CZ;
            this.elevationMode = elevationMode;
        }

        public PlacedTerrainObject(EditableWrpObject wrpObject, ModelInfoLibrary library)
        {
            model = library.ResolveByPath(wrpObject.Model);
            point = new TerrainPoint(wrpObject.Transform.Matrix.M41, wrpObject.Transform.Matrix.M43);
            yaw = FromRadians(-Math.Atan2(wrpObject.Transform.Matrix.M13, wrpObject.Transform.Matrix.M33));
            pitch = FromRadians(Math.Asin(-wrpObject.Transform.Matrix.M23));
            roll = FromRadians(Math.Atan2(wrpObject.Transform.Matrix.M21, wrpObject.Transform.Matrix.M22));
            elevation = wrpObject.Transform.Matrix.M42 - model.GetAbsoluteElevation(GetRotateAndScaleOnly(wrpObject.Transform.Matrix));
            elevationMode = ElevationMode.Absolute;
            if (Matrix4x4.Decompose(wrpObject.Transform.Matrix, out Vector3 cscale, out Quaternion q, out Vector3 _))
            {
                scale = cscale.X;
            }
            else
            {
                scale = 1;
            }
        }

        private static Matrix4x4 GetRotateAndScaleOnly(Matrix4x4 rotateMatrix)
        {
            rotateMatrix.M41 = 0;
            rotateMatrix.M42 = 0;
            rotateMatrix.M43 = 0;
            return rotateMatrix;
        }

        public ModelInfo Model => model;

        public TerrainPoint Point => point;

        public bool IsValid => !float.IsNaN(yaw);

        public string ToTerrainBuilderCSV()
        {
            return FormattableString.Invariant(@$"""{model.Name}"";{point.X + 200000:0.000};{point.Y:0.000};{yaw:0.000};{pitch:0.000};{roll:0.000};{scale:0.000};{elevation:0.000};");
        }

        public TerrainObject ToTerrainObject(ObjectLibraries library)
        {
            var obj = library.GetObject(model.Name);
            return new TerrainObject(obj, TerrainObject.TransformBack(obj, point, yaw), yaw, elevation - obj.CZ, pitch, roll);
        }

        public EditableWrpObject ToWrpObject(int id, ElevationGrid grid = null)
        {
            var matrix = Matrix4x4.CreateRotationY(ToRadians(yaw)) * Matrix4x4.CreateRotationX(ToRadians(-pitch)) * Matrix4x4.CreateRotationZ(ToRadians(-roll));
            if (scale != 1f)
            {
                matrix = matrix * Matrix4x4.CreateScale(scale);
            }
            matrix.M42 = GetZ(grid, matrix);
            matrix.M41 = point.X;
            matrix.M43 = point.Y;
            matrix.M44 = 1f;
            return new EditableWrpObject()
            {
                ObjectID = id,
                Model = model.Path,
                Transform = new BIS.Core.Math.Matrix4P(matrix)
            };
        }

        private float GetZ(ElevationGrid grid, Matrix4x4 matrix)
        {
            if (elevationMode == ElevationMode.Relative)
            {
                return model.GetRelativeElevation(grid, point, matrix) + elevation;
            }
            return model.GetAbsoluteElevation(matrix) + elevation;
        }

        private static float ToRadians(float angle)
        {
            if (angle == 0)
            {
                return 0;
            }
            return angle * MathF.PI / 180f;
        }

        private static float FromRadians(double angle)
        {
            if (angle == 0)
            {
                return 0;
            }
            return (float)(angle * 180.0 / Math.PI);
        }
    }
}
