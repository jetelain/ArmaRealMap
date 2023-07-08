using System.Numerics;
using GameRealisticMap.Geometries;

namespace GameRealisticMap.Arma3.TerrainBuilder
{
    internal sealed class ZProjection : I3dProjection
    {
        internal static readonly ZProjection Instance = new ZProjection();

        private ZProjection() { }

        public TerrainPoint Project(Vector3 p)
        {
            return new TerrainPoint(p.X, p.Y);
        }

        public Vector3 Unproject(TerrainPoint p)
        {
            return new Vector3(p.X, p.Y, 0);
        }
    }
}
