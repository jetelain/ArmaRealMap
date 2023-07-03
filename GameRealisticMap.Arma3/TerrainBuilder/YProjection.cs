using System.Numerics;
using GameRealisticMap.Geometries;

namespace GameRealisticMap.Arma3.TerrainBuilder
{
    internal sealed class YProjection : I3dProjection
    {
        internal static readonly YProjection Instance = new YProjection();

        private YProjection()
        {

        }

        public TerrainPoint Project(Vector3 p)
        {
            return new TerrainPoint(p.X, p.Z);
        }

        public Vector3 Unproject(TerrainPoint p)
        {
            return new Vector3(p.X, 0, p.Y);
        }
    }
}
