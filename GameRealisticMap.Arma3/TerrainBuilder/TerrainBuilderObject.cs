﻿using System.Globalization;
using System.Numerics;
using BIS.WRP;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.ElevationModel;
using GameRealisticMap.Geometries;

namespace GameRealisticMap.Arma3.TerrainBuilder
{
    public sealed class TerrainBuilderObject
    {
        public const int XShift = 200000;

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
            : this(library.ResolveByPath(wrpObject.Model), wrpObject.Transform.Matrix, ElevationMode.Absolute)
        {

        }

        public TerrainBuilderObject(ModelInfo model, EditableWrpObject wrpObject)
            : this(model, wrpObject.Transform.Matrix, ElevationMode.Absolute)
        {

        }

        public TerrainBuilderObject(ModelInfo model, Matrix4x4 wrpMatrix, ElevationMode elevationMode)
        {
            this.model = model;
            this.elevationMode = elevationMode;
            var rotateOnly = GetRotateAndScaleOnly(wrpMatrix);
            if (Matrix4x4.Decompose(wrpMatrix, out Vector3 cscale, out Quaternion _, out Vector3 _))
            {
                scale = cscale.X; // Asssume uniform scale
                if (scale != 1 && Matrix4x4.Invert(Matrix4x4.CreateScale(cscale), out var invertScale))
                {
                    rotateOnly = rotateOnly * invertScale;
                }
            }
            else
            {
                scale = 1;
            }
            point = new TerrainPoint(wrpMatrix.M41, wrpMatrix.M43);
            yaw = MathHelper.FromRadians(-Math.Atan2(rotateOnly.M13, rotateOnly.M33));
            pitch = MathHelper.FromRadians(Math.Asin(-rotateOnly.M23));
            roll = MathHelper.FromRadians(Math.Atan2(rotateOnly.M21, rotateOnly.M22));
            elevation = wrpMatrix.M42 - GetAbsoluteElevation(model, GetRotateAndScaleOnly(wrpMatrix));
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

        public float Elevation => elevation;

        public ElevationMode ElevationMode => elevationMode;

        /// <summary>
        /// "Yaw" in Terrain Builder / Heading
        /// </summary>
        public float Yaw => yaw;

        /// <summary>
        /// "Pitch" in Terrain Builder
        /// </summary>
        public float Pitch => pitch;

        /// <summary>
        /// "Roll" in Terrain Builder
        /// </summary>
        public float Roll => roll;

        /// <summary>
        /// "Scale" in Terrain Builder
        /// </summary>
        public float Scale => scale;

        public string ToTerrainBuilderCSV()
        {
            return FormattableString.Invariant(@$"""{model.Name}"";{point.X + XShift:0.000};{point.Y:0.000};{yaw:0.000};{pitch:0.000};{roll:0.000};{scale:0.000};{elevation:0.000};");
        }

        public EditableWrpObject ToWrpObject(IElevationGrid grid)
        {
            return new EditableWrpObject()
            {
                Model = model.Path,
                Transform = new BIS.Core.Math.Matrix4P(ToWrpTransform(grid))
            };
        }

        public Matrix4x4 ToWrpTransform(IElevationGrid grid)
        {
            var matrix = Matrix4x4.CreateRotationY(MathHelper.ToRadians(yaw)) * Matrix4x4.CreateRotationX(MathHelper.ToRadians(-pitch)) * Matrix4x4.CreateRotationZ(MathHelper.ToRadians(-roll));
            if (scale != 1f)
            {
                matrix = matrix * Matrix4x4.CreateScale(scale);
            }
            matrix.M42 = GetZ(grid, matrix);
            matrix.M41 = point.X;
            matrix.M43 = point.Y;
            matrix.M44 = 1f;
            return matrix;
        }

        public Matrix4x4 ToWrpTransform()
        {
            return ToWrpTransform(FlatElevationGrid.Zero);
        }

        public static TerrainBuilderObject RelativePitchThenYaw(ModelInfo model, TerrainPoint terrainPoint, float grmYaw, float grmPicth = 0, float elevation = 0, float scale = 1)
        {
            // Pitch/Yaw according to our conventions / Not TerrainBuilder ones
            var matrix = Matrix4x4.CreateRotationX(MathHelper.ToRadians(grmPicth)) * Matrix4x4.CreateRotationY(MathHelper.ToRadians(-grmYaw));
            if (scale != 1f)
            {
                matrix = matrix * Matrix4x4.CreateScale(scale);
            }
            matrix.M42 = elevation;
            matrix.M41 = terrainPoint.X;
            matrix.M43 = terrainPoint.Y;
            matrix.M44 = 1f;
            return new TerrainBuilderObject(model, matrix, ElevationMode.Relative);
        }

        private float GetZ(IElevationGrid grid, Matrix4x4 matrix)
        {
            if (elevationMode == ElevationMode.Relative)
            {
                return GetRelativeElevation(model, grid, point, matrix) + elevation;
            }
            return GetAbsoluteElevation(model, matrix) + elevation;
        }

        internal static float GetRelativeElevation(ModelInfo model, IElevationGrid grid, TerrainPoint point, Matrix4x4 matrix)
        {
            var pointToCenter = Vector3.Transform(model.BoundingCenter, matrix);
            return grid.ElevationAt(new TerrainPoint(point.X - pointToCenter.X, point.Y - pointToCenter.Z)) + pointToCenter.Y;
        }

        internal static float GetAbsoluteElevation(ModelInfo model, Matrix4x4 matrix)
        {
            var pointToCenter = Vector3.Transform(model.BoundingCenter, matrix);
            return pointToCenter.Y;
        }

        public override string ToString()
        {
            return ToTerrainBuilderCSV();
        }
    }
}
