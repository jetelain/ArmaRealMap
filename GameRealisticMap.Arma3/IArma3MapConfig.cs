namespace GameRealisticMap.Arma3
{
    public interface IArma3MapConfig
    {
        float SizeInMeters { get; }

        int TileSize { get; } // Typical value: 1024

        double Resolution { get; } // Typical value: 1m/px

        string PboPrefix { get; }

        float FakeSatBlend { get; }

        string WorldName { get; }

        bool UseColorCorrection { get; }
    }
}