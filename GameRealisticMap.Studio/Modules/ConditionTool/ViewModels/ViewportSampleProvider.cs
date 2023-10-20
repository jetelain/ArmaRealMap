using System.Collections.Generic;
using System.Numerics;
using GameRealisticMap.Geometries;

namespace GameRealisticMap.Studio.Modules.ConditionTool.ViewModels
{
    internal sealed class ViewportSampleProvider : IConditionSampleProvider<TerrainPoint>, IConditionSampleProvider<TerrainPath>, IConditionSampleProvider<TerrainPolygon>
    {
        private const int NumX = 20;
        private const int NumY = 15;
        private readonly Vector2 delta;
        private readonly TerrainPoint start;

        public ViewportSampleProvider(ITerrainEnvelope envelope)
        {
            this.delta = (envelope.MaxPoint.Vector - envelope.MinPoint.Vector) / new Vector2(NumX, NumY);
            this.start = envelope.MinPoint;
        }

        IEnumerable<TerrainPoint> IConditionSampleProvider<TerrainPoint>.GetSamplePoints(IBuildContext buildContext)
        {
            var points = new List<TerrainPoint>(NumX*NumY);
            for(int x = 0; x < NumX; x++)
            {
                for (int y = 0; y < NumY; y++)
                {
                    points.Add(start + delta * (new Vector2(x, y) + new Vector2(0.5f)));
                }
            }
            return points;
        }

        IEnumerable<TerrainPath> IConditionSampleProvider<TerrainPath>.GetSamplePoints(IBuildContext buildContext)
        {
            var points = new List<TerrainPath>(NumX * NumY * 2);
            for (int x = 0; x < NumX; x++)
            {
                for (int y = 0; y < NumY; y++)
                {
                    var p1 = start + delta * (new Vector2(x, y));
                    var p2 = start + delta * (new Vector2(x, y) + new Vector2(1));
                    var p3 = start + delta * (new Vector2(x+1, y));
                    var p4 = start + delta * (new Vector2(x, y+1));
                    points.Add(new TerrainPath(p1, p2));
                    points.Add(new TerrainPath(p3, p4));
                }
            }
            return points;
        }

        IEnumerable<TerrainPolygon> IConditionSampleProvider<TerrainPolygon>.GetSamplePoints(IBuildContext buildContext)
        {
            var points = new List<TerrainPolygon>(NumX * NumY);
            for (int x = 0; x < NumX; x++)
            {
                for (int y = 0; y < NumY; y++)
                {
                    var p1 = start + delta * (new Vector2(x, y));
                    var p2 = start + delta * (new Vector2(x, y) + new Vector2(1));
                    points.Add(TerrainPolygon.FromRectangle(p1, p2));
                }
            }
            return points;
        }
    }
}
