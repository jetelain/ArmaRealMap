using System.Numerics;
using GameRealisticMap.Geometries;

namespace GameRealisticMap.Arma3.TerrainBuilder
{
    internal interface I3dProjection
    {
        TerrainPoint Project(Vector3 vector3);

        Vector3 Unproject(TerrainPoint point);
    }
}
