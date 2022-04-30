using System;
using System.Numerics;
using ArmaRealMap.ElevationModel;
using ArmaRealMap.Geometries;

namespace ArmaRealMap
{
    public class ModelInfo
    {
        public string Name { get; set; }

        public string Path { get; set; }

        public string Bundle { get; set; }

        public Vector3 BoundingCenter { get; set; }  

        internal float GetRelativeElevation(ElevationGrid grid, TerrainPoint point, Matrix4x4 matrix)
        {
            var pointToCenter = Vector3.Transform(BoundingCenter, matrix);
            return grid.ElevationAt(new TerrainPoint(point.X - pointToCenter.X, point.Y - pointToCenter.Z)) + pointToCenter.Y;
        }
        internal float GetAbsoluteElevation(Matrix4x4 matrix)
        {
            var pointToCenter = Vector3.Transform(BoundingCenter, matrix);
            return pointToCenter.Y;
        }
    }
}