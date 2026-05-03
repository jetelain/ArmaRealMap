using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameRealisticMap.Geometries;

namespace GameRealisticMap.ElevationModel
{
    /// <summary>
    /// Read/write access to a 2D elevation grid.
    /// Implemented by <see cref="ElevationGrid"/> (normal RAM-backed) and
    /// <see cref="FlatElevationGrid"/> (all-zero stub for testing).
    /// </summary>
    public interface IElevationGrid
    {
        float ElevationAt(TerrainPoint terrainPoint);
    }
}
