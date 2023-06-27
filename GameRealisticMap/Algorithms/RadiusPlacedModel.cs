using System.Numerics;
using GameRealisticMap.Geometries;

namespace GameRealisticMap.Algorithms
{
    public class RadiusPlacedModel<TModelInfo> : ITerrainEnvelope, IModelPosition
    {
        private readonly BoundingCircle boundingCircle;

        public RadiusPlacedModel(BoundingCircle boundingCircle, float elevation, float scale, TModelInfo modelInfo, float radius)
        {
            this.boundingCircle = boundingCircle;
            this.RelativeElevation = elevation;
            this.Model = modelInfo;
            this.Scale = scale;
            this.Radius = radius;

            var boxRadius = MathF.Max(boundingCircle.Radius, radius);
            MinPoint = Center - new Vector2(boxRadius, boxRadius);
            MaxPoint = Center + new Vector2(boxRadius, boxRadius);
        }

        public TerrainPoint MinPoint { get; }

        public TerrainPoint MaxPoint { get; }

        public float Angle => boundingCircle.Angle;

        public float Radius { get; }

        public float FitRadius => boundingCircle.Radius;

        public TerrainPoint Center => boundingCircle.Center;

        public float RelativeElevation { get; }

        public TModelInfo Model { get; }

        public float Scale { get; }
    }
}