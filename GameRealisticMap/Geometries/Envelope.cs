using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameRealisticMap.Geometries
{
    internal class Envelope : ITerrainGeometry
    {
        public Envelope(TerrainPoint minPoint, TerrainPoint maxPoint)
        {
            MinPoint = minPoint;
            MaxPoint = maxPoint;
        }

        public TerrainPoint MinPoint { get; }

        public TerrainPoint MaxPoint { get; }
    }
}
