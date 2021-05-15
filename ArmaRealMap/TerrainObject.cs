using System;
using System.Globalization;
using System.IO;
using System.Numerics;
using ArmaRealMap.Geometries;
using ArmaRealMap.Libraries;

namespace ArmaRealMap
{
    internal class TerrainObject
    {
        private readonly SingleObjetInfos objectInfos;
        private readonly IBoundingShape box;
        private readonly float? absoluteElevation;

        public TerrainObject (SingleObjetInfos objectInfos, IBoundingShape box)
        {
            this.box = box;
            this.objectInfos = objectInfos;
        }
        public TerrainObject(SingleObjetInfos objectInfos, TerrainPoint point, float angle)
            : this(objectInfos, new BoundingCircle(point, objectInfos.GetPlacementRadius(), angle))
        {
        }

        public TerrainObject(SingleObjetInfos objectInfos, TerrainPoint point, float angle, float absoluteElevation)
            : this(objectInfos, new BoundingCircle(point, objectInfos.GetPlacementRadius(), angle))
        {
            this.absoluteElevation = absoluteElevation;
        }

        public Vector2 StartPoint => box.StartPoint;

        public Vector2 EndPoint => box.EndPoint;

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
            if (absoluteElevation != null)
            {
                return string.Format(CultureInfo.InvariantCulture, @"""{0}"";{1:0.000};{2:0.000};{3:0.000};0.0;0.0;1;{4:0.000};",
                                objectInfos.Name,
                                box.Center.X + objectInfos.CX + mapInfos.StartPointUTM.Easting,
                                box.Center.Y + objectInfos.CY + mapInfos.StartPointUTM.Northing,
                                -box.Angle,
                                absoluteElevation.Value
                                );
            }

            return string.Format(CultureInfo.InvariantCulture, @"""{0}"";{1:0.000};{2:0.000};{3:0.000};0.0;0.0;1;0.0;",
                            objectInfos.Name,
                            box.Center.X + objectInfos.CX + mapInfos.StartPointUTM.Easting,
                            box.Center.Y + objectInfos.CY + mapInfos.StartPointUTM.Northing,
                            -box.Angle
                            );
        }
    }
}