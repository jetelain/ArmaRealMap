using System;

namespace ArmaRealMap.Geometries
{
    /// <summary>
    /// Coordinates for Terrain Builder (but not for road.shp, or imagery)
    /// </summary>
    public class TerrainPoint : IEquatable<TerrainPoint>
    {
        public static readonly TerrainPoint Empty = default;

        public TerrainPoint(float x, float y)
        {
            X = x;
            Y = y;
        }

        public float X { get; set; }

        public float Y { get; set; }

        public bool IsEmpty => Equals(Empty);

        public static bool operator ==(TerrainPoint left, TerrainPoint right) => left.Equals(right);

        public static bool operator !=(TerrainPoint left, TerrainPoint right) => !left.Equals(right);

        public void Deconstruct(out float x, out float y)
        {
            x = X;
            y = Y;
        }

        public override int GetHashCode() => HashCode.Combine(X, Y);

        public override string ToString() => $"[ X={X}, Y={Y} ]";

        public override bool Equals(object obj) => obj is TerrainPoint && Equals((TerrainPoint)obj);

        public bool Equals(TerrainPoint other) => X.Equals(other.X) && Y.Equals(other.Y);
    }
}
