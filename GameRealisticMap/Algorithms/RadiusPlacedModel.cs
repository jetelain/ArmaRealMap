using GameRealisticMap.Geometries;

namespace GameRealisticMap.Algorithms
{
    public class RadiusPlacedModel<TModelInfo> : ITerrainEnvelope
    {
        private readonly BoundingCircle boundingCircle;

        public RadiusPlacedModel(BoundingCircle boundingCircle, float? elevation, TModelInfo modelInfo)
        {
            this.boundingCircle = boundingCircle;
            this.Elevation = elevation;
            this.Model = modelInfo;
        }

        public TerrainPoint MinPoint => boundingCircle.MinPoint;

        public TerrainPoint MaxPoint => boundingCircle.MaxPoint;

        public TerrainPolygon Polygon => boundingCircle.Polygon;

        public BoundingCircle Box => boundingCircle;

        public float Angle => boundingCircle.Angle;

        public float Radius => boundingCircle.Radius;

        public TerrainPoint Center => boundingCircle.Center;

        public float? Elevation { get; }

        public TModelInfo Model { get; }
    }
}