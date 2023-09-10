using System.Collections.Generic;
using System.Numerics;
using GameRealisticMap.Geometries;

namespace GameRealisticMap.Studio.Modules.ConditionTool.ViewModels
{
    internal class ViewportPointProvider : ISamplePointProvider
    {
        private const int NumX = 20;
        private const int NumY = 15;
        private readonly ITerrainEnvelope envelope;

        public ViewportPointProvider(ITerrainEnvelope envelope)
        {
            this.envelope = envelope;
        }

        public IEnumerable<TerrainPoint> GetSamplePoints(IBuildContext buildContext)
        {
            var delta = (envelope.MaxPoint.Vector - envelope.MinPoint.Vector) / new Vector2(NumX, NumY);
            var points = new List<TerrainPoint>(NumX*NumY);
            for(int x = 0; x < NumX; x++)
            {
                for (int y = 0; y < NumY; y++)
                {
                    points.Add(envelope.MinPoint + delta * (new Vector2(x, y) + new Vector2(0.5f, 0.5f)));
                }
            }
            return points;
        }
    }
}
