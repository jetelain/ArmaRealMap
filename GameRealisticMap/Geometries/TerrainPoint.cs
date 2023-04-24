using System.Numerics;
using System.Text.Json.Serialization;
using ClipperLib;
using GeoJSON.Text.Geometry;

namespace GameRealisticMap.Geometries
{
    /// <summary>
    /// Coordinates 
    /// 
    /// Good for up to 83 Km with 1 cm precision
    /// </summary>
    [JsonConverter(typeof(TerrainPointJsonConverter))]
    public class TerrainPoint : IEquatable<TerrainPoint>, IPosition, ITerrainEnvelope
    {
        public static readonly TerrainPoint Empty = new TerrainPoint(Vector2.Zero);

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

        double? IPosition.Altitude => null;

        double IPosition.Latitude => Math.Round(Y, 3);

        double IPosition.Longitude => Math.Round(X, 3);

        TerrainPoint ITerrainEnvelope.MinPoint => this;

        TerrainPoint ITerrainEnvelope.MaxPoint => this;

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

        public override bool Equals(object? obj) => Equals(obj as TerrainPoint);

        public bool Equals(TerrainPoint? other) => other != null && ((vector - other.vector).LengthSquared() < 0.01f);

        public static bool Equals(TerrainPoint? a, TerrainPoint? b)
        {
            return a == b || (a != null && a.Equals(b));
        }

        internal static TerrainPoint FromGeoJson(IPosition point)
        {
            return new TerrainPoint((float)point.Longitude, (float)point.Latitude);
        }

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
