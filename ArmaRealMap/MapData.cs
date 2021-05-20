using System;
using System.Collections.Generic;
using System.Text;
using ArmaRealMap.ElevationModel;
using ArmaRealMap.Roads;

namespace ArmaRealMap
{
    internal class MapData
    {
        public MapInfos MapInfos { get; set; }
        public ElevationGrid Elevation { get; set; }
        public List<Road> Roads { get; internal set; }
    }
}
