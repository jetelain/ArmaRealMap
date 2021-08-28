using System;
using System.Globalization;
using System.Numerics;
using ArmaRealMap.Geometries;
using ArmaRealMap.Libraries;

namespace ArmaRealMap
{
    internal class TerrainObject : ITerrainGeometry
    {
        private readonly SingleObjetInfos objectInfos;
        private readonly IBoundingShape box;
        private readonly float? absoluteElevation;

        public TerrainObject (SingleObjetInfos objectInfos, IBoundingShape box, float? absoluteElevation = null)
        {
            this.box = box;
            this.objectInfos = objectInfos;
            this.absoluteElevation = absoluteElevation;
        }
        public TerrainObject(SingleObjetInfos objectInfos, TerrainPoint point, float angle)
            : this(objectInfos, new BoundingCircle(point, objectInfos.GetPlacementRadius(), angle))
        {
        }

        public TerrainObject(SingleObjetInfos objectInfos, TerrainPoint point, float angle, float absoluteElevation)
            : this(objectInfos, new BoundingCircle(point, objectInfos.GetPlacementRadius(), angle), absoluteElevation)
        {

        }

        public TerrainPoint MinPoint => box.MinPoint;

        public TerrainPoint MaxPoint => box.MaxPoint;

        public NetTopologySuite.Geometries.Polygon Poly => box.Poly;

        public TerrainPoint Center => box.Center;

        public SingleObjetInfos Object => objectInfos;

        public float DistanceTo(TerrainObject other)
        {
            return Vector2.Subtract(other.Center.Vector, Center.Vector).Length();
            //return Math.Sqrt(Math.Pow(other.Center.X - Center.X, 2) + Math.Pow(other.Center.Y - Center.Y, 2));
        }

        public string ToString(MapInfos mapInfos)
        {
            var scale = objectInfos.Scale == null ? "1" : objectInfos.Scale.Value.ToString("0.000", CultureInfo.InvariantCulture);
            var point = box.Center;
            if (objectInfos.CX != 0 || objectInfos.CY != 0)
            {
                point = point + Vector2.Transform(
                    new Vector2(objectInfos.CX, objectInfos.CY), 
                    Matrix3x2.CreateRotation(box.Angle * MathF.PI / 180f)
                    );
            }
            if (absoluteElevation != null)
            {
                return string.Format(CultureInfo.InvariantCulture, @"""{0}"";{1:0.000};{2:0.000};{3:0.000};0.0;0.0;{4};{5:0.000};",
                                objectInfos.Name,
                                point.X + 200000/*+ mapInfos.StartPointUTM.Easting*/,
                                point.Y /*+ mapInfos.StartPointUTM.Northing*/,
                                -box.Angle,
                                scale,
                                absoluteElevation.Value);
            }
            return string.Format(CultureInfo.InvariantCulture, @"""{0}"";{1:0.000};{2:0.000};{3:0.000};0.0;0.0;{4};0.0;",
                            objectInfos.Name,
                            point.X + 200000/*+ mapInfos.StartPointUTM.Easting*/,
                            point.Y /*+ mapInfos.StartPointUTM.Northing*/,
                            -box.Angle,
                            scale);
        }
    }
}