using System.Numerics;
using System.Text.Json.Serialization;
using ClipperLib;
using GeoJSON.Text.Geometry;
using Pmad.Geometry;

namespace GameRealisticMap.Geometries
{
    /// <summary>
    /// A 2D point in local terrain space, measured in metres from the south-west corner of the terrain area.
    /// X increases eastward, Y increases northward. Origin (0, 0) is the south-west corner.
    /// Precise to approximately 1 cm for terrain areas up to 83 km across.
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

        internal static TerrainPoint FromPmadGeometry(Vector2D point)
        {
            return new TerrainPoint((float)point.X, (float)point.Y);
        }

        public static TerrainPoint operator +(TerrainPoint left, Vector2 right)
        {
            return new TerrainPoint(left.Vector + right);
        }

        public static TerrainPoint operator -(TerrainPoint left, Vector2 right)
        {
            return new TerrainPoint(left.Vector - right);
        }

        public TerrainPoint ToIntPointPrecision()
        {
            return new TerrainPoint(ToIntPoint());
        }
    }
}
