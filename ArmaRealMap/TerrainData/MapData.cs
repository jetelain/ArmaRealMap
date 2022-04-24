using System;
using System.Collections.Generic;
using System.Text;
using ArmaRealMap.ElevationModel;
using ArmaRealMap.Geometries;
using ArmaRealMap.Roads;

namespace ArmaRealMap
{
    internal class MapData
    {
        public GlobalConfig GlobalConfig { get; set; }
        public MapConfig Config { get; set; }
        public MapInfos MapInfos { get; set; }
        public ElevationGrid Elevation { get; set; }
        public List<Road> Roads { get; internal set; }
        public List<Building> WantedBuildings { get; internal set; }
        public TerrainObjectLayer Buildings { get; internal set; }
        public HashSet<string> UsedObjects { get; internal set; }
        public List<TerrainPolygon> Forests { get; internal set; }
        public List<TerrainPolygon> Scrubs { get; internal set; }
        public List<Lake> Lakes { get; internal set; }
    }
}
