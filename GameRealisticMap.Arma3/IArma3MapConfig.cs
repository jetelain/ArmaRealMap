namespace GameRealisticMap.Arma3
{
    public interface IArma3MapConfig
    {
        float SizeInMeters { get; }

        int TileSize { get; }

        double Resolution { get; } // Typical value: 1m/px

        string PboPrefix { get; }

        double TextureSizeInMeters { get; } // Typical value: 4m

        float FakeSatBlend { get; }

        string WorldName { get; }
    }
}