namespace GameRealisticMap.Arma3
{
    public class Arma3MapConfigJson
    {
        public int TileSize { get; set; } = 512;

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

        public Arma3MapConfig ToArma3MapConfig()
        {
            return new Arma3MapConfig(this);
        }
    }
}
