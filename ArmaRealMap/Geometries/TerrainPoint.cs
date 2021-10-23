using System;
using System.Numerics;
using ClipperLib;

namespace ArmaRealMap.Geometries
{
    /// <summary>
    /// Coordinates for Terrain Builder (but not for road.shp, or imagery)
    /// 
    /// Good for up to 83 Km with 1 cm precision
    /// </summary>
    public class TerrainPoint : IEquatable<TerrainPoint>
    {
        public static readonly TerrainPoint Empty = default;

        private Vector2 vector;

        public TerrainPoint(float x, float y)
        {
            vector = new Vector2(x, y);
        }

        public TerrainPoint(Vector2 vector)
        {
            this.vector = vector ;
        }
        internal TerrainPoint(IntPoint point) : this(
                  (float)(point.X  / GeometryHelper.ScaleForClipper), 
                  (float)(point.Y / GeometryHelper.ScaleForClipper))
        {

        }

        public float X => vector.X;

        public float Y => vector.Y;

        public Vector2 Vector => vector;

        public bool IsEmpty => Equals(Empty);

        public static bool operator ==(TerrainPoint left, TerrainPoint right) => left.Equals(right);

        public static bool operator !=(TerrainPoint left, TerrainPoint right) => !left.Equals(right);

        public void Deconstruct(out float x, out float y)
        {
            x = X;
            y = Y;
        }

        public IntPoint ToIntPoint()
        {
            return new IntPoint(X * GeometryHelper.ScaleForClipper, Y * GeometryHelper.ScaleForClipper);
        }

        public override int GetHashCode() => vector.GetHashCode();

        public override string ToString() => Vector.ToString();

        public override bool Equals(object obj) => Equals(obj as TerrainPoint);

        public bool Equals(TerrainPoint other) => !ReferenceEquals(null, other) && ((vector - other.vector).LengthSquared() < 0.01f);

        public static TerrainPoint operator +(TerrainPoint left, Vector2 right)
        {
            return new TerrainPoint(left.Vector + right);
        }

        public static TerrainPoint operator -(TerrainPoint left, Vector2 right)
        {
            return new TerrainPoint(left.Vector - right);
        }
    }
}
