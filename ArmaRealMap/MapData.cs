using System;
using System.Collections.Generic;
using System.Text;
using ArmaRealMap.ElevationModel;

namespace ArmaRealMap
{
    internal class MapData
    {
        public MapInfos MapInfos { get; set; }
        public ElevationGrid Elevation { get; set; }
    }
}
