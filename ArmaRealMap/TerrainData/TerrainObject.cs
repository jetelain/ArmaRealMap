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
        private readonly float? elevation;

        public TerrainObject (SingleObjetInfos objectInfos, IBoundingShape box, float? elevation = null)
        {
            this.box = box;
            this.objectInfos = objectInfos;
            if (elevation.HasValue && Math.Abs(elevation.Value) > 0.001f)
            {
                this.elevation = elevation;
            }
        }

        public TerrainObject(SingleObjetInfos objectInfos, TerrainPoint point, float angle, float? elevation = null)
            : this(objectInfos, new BoundingCircle(point, objectInfos.GetReservedRadius(), angle), elevation)
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
        }

        public string ToTerrainBuilderCSV()
        {
            var point = Transform(objectInfos, box.Center, box.Angle);
            if (elevation != null)
            {
                return string.Format(CultureInfo.InvariantCulture, @"""{0}"";{1:0.000};{2:0.000};{3:0.000};0.0;0.0;1;{4:0.000};",
                                objectInfos.Name,
                                point.X + 200000,
                                point.Y,
                                -box.Angle,
                                elevation.Value + objectInfos.CZ);
            }
            return string.Format(CultureInfo.InvariantCulture, @"""{0}"";{1:0.000};{2:0.000};{3:0.000};0.0;0.0;1;{4:0.000};",
                            objectInfos.Name,
                            point.X + 200000,
                            point.Y,
                            -box.Angle,
                            objectInfos.CZ);
        }

        internal static TerrainPoint Transform(SingleObjetInfos obj, TerrainPoint point, float angle)
        {
            if (obj.CX != 0 || obj.CY != 0)
            {
                point = point + FixCenter(obj, angle);
            }
            return point;
        }
        internal static TerrainPoint TransformBack(SingleObjetInfos obj, TerrainPoint point, float angle)
        {
            if (obj.CX != 0 || obj.CY != 0)
            {
                point = point - FixCenter(obj, angle);
            }
            return point;
        }

        private static Vector2 FixCenter(SingleObjetInfos obj, float angle)
        {
            return Vector2.Transform(
                    new Vector2(obj.CX, obj.CY),
                    Matrix3x2.CreateRotation(angle * MathF.PI / 180f)
                );
        }
    }
}