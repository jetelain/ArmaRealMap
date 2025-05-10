using GameRealisticMap.Configuration;

namespace GameRealisticMap.Arma3
{
    public class Arma3MapConfigJson
    {
        public int? TileSize { get; set; }

        public double Resolution { get; set; } = 1;

        public string? PboPrefix { get; set; }

        public float FakeSatBlend { get; set; } = 0.5f;

        public string? WorldName { get; set; }

        public float GridCellSize { get; set; } = 5;

        public int GridSize { get; set; } = 1024;

        public string? SouthWest { get; set; }

        public string? Center { get; set; }

        public string? AssetConfigFile { get; set; }

        public string? TargetModDirectory { get; set; }

        public bool UseColorCorrection { get; set; }

        public int IdMapMultiplier { get; set; } = 1;

        public float? PrivateServiceRoadThreshold { get; set; }

        public SatelliteImageOptions? Satellite { get; set; }

        public bool IsPersisted { get; set; } = true;

        public Arma3MapConfig ToArma3MapConfig()
        {
            return new Arma3MapConfig(this);
        }
    }
}
