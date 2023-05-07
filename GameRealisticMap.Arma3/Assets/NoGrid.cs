using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameRealisticMap.ElevationModel;
using GameRealisticMap.Geometries;

namespace GameRealisticMap.Arma3.Assets
{
    internal class NoGrid : IElevationGrid
    {
        internal static readonly NoGrid Zero = new NoGrid();

        public float ElevationAt(TerrainPoint terrainPoint)
        {
            return 0;
        }
    }
}
