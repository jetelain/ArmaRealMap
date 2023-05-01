using System.Globalization;
using System.Numerics;
using BIS.WRP;
using GameRealisticMap.ElevationModel;
using GameRealisticMap.Geometries;

namespace GameRealisticMap.Arma3.TerrainBuilder
{
    public sealed class TerrainBuilderObject
    {
        private const int XShift = 200000;

        private readonly TerrainPoint point;
        private readonly float elevation;
        private readonly float yaw;
        private readonly float pitch;
        private readonly float roll;
        private readonly float scale;
        private readonly ElevationMode elevationMode;
        private readonly ModelInfo model;

        public TerrainBuilderObject(ModelInfo model, TerrainPoint point, float elevation = 0f, ElevationMode elevationMode = ElevationMode.Relative, float yaw = 0f, float pitch = 0f, float roll = 0f, float scale = 1f)
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

        public TerrainBuilderObject(ElevationMode elevationMode, string terrainBuilderCsv, IModelInfoLibrary library)
            : this(elevationMode, terrainBuilderCsv.Split(';'), library)
        {

        }

        public TerrainBuilderObject(ElevationMode elevationMode, string[] terrainBuilderCsv, IModelInfoLibrary library)
        {
            model = library.ResolveByName(terrainBuilderCsv[0].Trim('"'));
            point = new TerrainPoint(
                        (float)(double.Parse(terrainBuilderCsv[1], CultureInfo.InvariantCulture) - XShift),
                        (float)(double.Parse(terrainBuilderCsv[2], CultureInfo.InvariantCulture)));
            yaw = float.Parse(terrainBuilderCsv[3], CultureInfo.InvariantCulture);
            pitch = float.Parse(terrainBuilderCsv[4], CultureInfo.InvariantCulture);
            roll = float.Parse(terrainBuilderCsv[5], CultureInfo.InvariantCulture);
            scale = float.Parse(terrainBuilderCsv[6], CultureInfo.InvariantCulture);
            elevation = float.Parse(terrainBuilderCsv[7], CultureInfo.InvariantCulture);
            this.elevationMode = elevationMode;
        }

        public TerrainBuilderObject(EditableWrpObject wrpObject, IModelInfoLibrary library)
            : this(library.ResolveByPath(wrpObject.Model), wrpObject)
        {

        }

        public TerrainBuilderObject(ModelInfo model, EditableWrpObject wrpObject)
        {
            this.model = model;
            point = new TerrainPoint(wrpObject.Transform.Matrix.M41, wrpObject.Transform.Matrix.M43);
            yaw = FromRadians(-Math.Atan2(wrpObject.Transform.Matrix.M13, wrpObject.Transform.Matrix.M33));
            pitch = FromRadians(Math.Asin(-wrpObject.Transform.Matrix.M23));
            roll = FromRadians(Math.Atan2(wrpObject.Transform.Matrix.M21, wrpObject.Transform.Matrix.M22));
            elevation = wrpObject.Transform.Matrix.M42 - GetAbsoluteElevation(model, GetRotateAndScaleOnly(wrpObject.Transform.Matrix));
            elevationMode = ElevationMode.Absolute;
            if (Matrix4x4.Decompose(wrpObject.Transform.Matrix, out Vector3 cscale, out Quaternion _, out Vector3 _))
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
            return FormattableString.Invariant(@$"""{model.Name}"";{point.X + XShift:0.000};{point.Y:0.000};{yaw:0.000};{pitch:0.000};{roll:0.000};{scale:0.000};{elevation:0.000};");
        }

        public EditableWrpObject ToWrpObject(int id, ElevationGrid grid)
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
                return GetRelativeElevation(model, grid, point, matrix) + elevation;
            }
            return GetAbsoluteElevation(model, matrix) + elevation;
        }

        internal static float GetRelativeElevation(ModelInfo model, ElevationGrid grid, TerrainPoint point, Matrix4x4 matrix)
        {
            var pointToCenter = Vector3.Transform(model.BoundingCenter, matrix);
            return grid.ElevationAt(new TerrainPoint(point.X - pointToCenter.X, point.Y - pointToCenter.Z)) + pointToCenter.Y;
        }

        internal static float GetAbsoluteElevation(ModelInfo model, Matrix4x4 matrix)
        {
            var pointToCenter = Vector3.Transform(model.BoundingCenter, matrix);
            return pointToCenter.Y;
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
