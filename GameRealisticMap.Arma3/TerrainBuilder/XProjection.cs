using System.Numerics;
using GameRealisticMap.Geometries;

namespace GameRealisticMap.Arma3.TerrainBuilder
{
    internal sealed class XProjection : I3dProjection
    {
        internal static readonly XProjection Instance = new XProjection();

        private XProjection() { }

        public TerrainPoint Project(Vector3 p)
        {
            return new TerrainPoint(p.Z, p.Y);
        }

        public Vector3 Unproject(TerrainPoint p)
        {
            return new Vector3(0, p.Y, p.X);
        }
    }
}
