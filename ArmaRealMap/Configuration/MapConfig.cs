using System;
using ArmaRealMap.Core.ObjectLibraries;

namespace ArmaRealMap
{
    public class MapConfig
    {
        public int GridSize { get; set; }

        public int CellSize { get; set; }

        public double? Resolution { get; set; }

        public string BottomLeft { get; set; }

        public string WorldName { get; set; }

        public string PboPrefix { get; set; }

        public TargetConfig Target { get; set; }

        public TerrainRegion Terrain { get; set; }

        public int TileSize { get; set; } = 1024;

        public int RealTileOverlap => TileSize / 32;

        public double? TextureScale { get; set; }

        public ForcedElevationArea[] ForcedElevation { get; set; }

        public float? FakeSatBlend { get; set; }

        internal MapConfig ToCook()
        {
            var config = (MapConfig)MemberwiseClone();
            config.ForcedElevation = null;
            config.Target = null;
            return config;
        }
    }
}
