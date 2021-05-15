using System;
using System.Numerics;

namespace ArmaRealMap.Geometries
{
    /// <summary>
    /// Coordinates for Terrain Builder (but not for road.shp, or imagery)
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

        public override int GetHashCode() => vector.GetHashCode();

        public override string ToString() => $"[ X={X}, Y={Y} ]";

        public override bool Equals(object obj) => obj is TerrainPoint && Equals((TerrainPoint)obj);

        public bool Equals(TerrainPoint other) => vector.Equals(other.vector);

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
