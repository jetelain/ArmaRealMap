using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameRealisticMap.Geometries;

namespace GameRealisticMap.ElevationModel
{
    public interface IElevationGrid
    {
        float ElevationAt(TerrainPoint terrainPoint);
    }
}
