using System.Numerics;

namespace GameRealisticMap.Geometries
{
    public static class TerrainEnvelopeExtensions
    {
        public static bool EnveloppeContains(this ITerrainEnvelope scope, ITerrainEnvelope item)
        {
            return
                item.MinPoint.X >= scope.MinPoint.X &&
                item.MinPoint.Y >= scope.MinPoint.Y &&
                item.MaxPoint.X <= scope.MaxPoint.X &&
                item.MaxPoint.Y <= scope.MaxPoint.Y;
        }

        public static ITerrainEnvelope WithMargin(this ITerrainEnvelope geometry, float margin)
        {
            return WithMargin(geometry, new Vector2(margin));
        }

        public static ITerrainEnvelope WithMargin(this ITerrainEnvelope geometry, Vector2 margin)
        {
            return new Envelope(geometry.MinPoint - margin, geometry.MaxPoint + margin);
        }

        public static IReadOnlyList<ITerrainEnvelope> SplitQuad(this ITerrainEnvelope scope)
        {
            var midPoint = (scope.MaxPoint.Vector + scope.MinPoint.Vector) / 2;
            return new[]
            {
                new Envelope(new TerrainPoint(scope.MinPoint.X, scope.MinPoint.Y), new TerrainPoint(midPoint.X, midPoint.Y)),
                new Envelope(new TerrainPoint(scope.MinPoint.X, midPoint.Y), new TerrainPoint(midPoint.X, scope.MaxPoint.Y)),
                new Envelope(new TerrainPoint(midPoint.X, scope.MinPoint.Y), new TerrainPoint(scope.MaxPoint.X, midPoint.Y)),
                new Envelope(new TerrainPoint(midPoint.X, midPoint.Y), new TerrainPoint(scope.MaxPoint.X, scope.MaxPoint.Y))
            };
        }
    }
}
