using GameRealisticMap.Geometries;

namespace GameRealisticMap.Algorithms
{
    public class RadiusPlacedModel<TModelInfo> : ITerrainEnvelope, IModelPosition
    {
        private readonly BoundingCircle boundingCircle;

        public RadiusPlacedModel(BoundingCircle boundingCircle, float elevation, float scale, TModelInfo modelInfo)
        {
            this.boundingCircle = boundingCircle;
            this.RelativeElevation = elevation;
            this.Model = modelInfo;
            this.Scale = scale;
        }

        public TerrainPoint MinPoint => boundingCircle.MinPoint;

        public TerrainPoint MaxPoint => boundingCircle.MaxPoint;

        public TerrainPolygon Polygon => boundingCircle.Polygon;

        public BoundingCircle Box => boundingCircle;

        public float Angle => boundingCircle.Angle;

        public float Radius => boundingCircle.Radius;

        public TerrainPoint Center => boundingCircle.Center;

        public float RelativeElevation { get; }

        public TModelInfo Model { get; }

        public float Scale { get; }
    }
}