using System.Numerics;
using GameRealisticMap.ElevationModel;
using GameRealisticMap.Geometries;

namespace GameRealisticMap.Arma3.Test
{
    internal class SlopeElevationGrid : IElevationGrid
    {
        private readonly float baseElevation;

        private readonly Vector2 slopeFactor;

        public SlopeElevationGrid(float baseElevation, Vector2 slopeFactor)
        {
            this.baseElevation = baseElevation;
            this.slopeFactor = slopeFactor;
        }

        public float ElevationAt(TerrainPoint terrainPoint)
        {
            var value = terrainPoint.Vector * slopeFactor;
            return baseElevation + value.X + value.Y;
        }
    }
}
