namespace GameRealisticMap.Arma3
{
    public interface IArma3MapConfig
    {
        float SizeInMeters { get; }

        int TileSize { get; } // Typical value: 1024

        double Resolution { get; } // Typical value: 1m/px

        int IdMapMultiplier { get; } // 1, 2 or 4

        string PboPrefix { get; }

        float FakeSatBlend { get; }

        string WorldName { get; }

        bool UseColorCorrection { get; }
    }
}