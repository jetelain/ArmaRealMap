namespace GameRealisticMap.Arma3
{
    /// <summary>
    /// Configuration interface for an Arma 3 map. Provides the parameters used by layer generators
    /// and WRP/PBO compilers. Implemented by <see cref="Arma3MapConfig"/>.
    /// </summary>
    public interface IArma3MapConfig
    {
        /// <summary>Total side length of the terrain in metres.</summary>
        float SizeInMeters { get; }

        /// <summary>Size of each satellite imagery tile in pixels. Typical value: 1024.</summary>
        int TileSize { get; }

        /// <summary>Imagery resolution in metres per pixel. Typical value: 1.0.</summary>
        double Resolution { get; }

        /// <summary>Multiplier for the material ID map resolution relative to satellite tiles. Valid values: 1, 2, or 4.</summary>
        int IdMapMultiplier { get; }

        /// <summary>PBO prefix path used in Arma 3 configs (e.g. <c>z\arm\addons\mymap</c>).</summary>
        string PboPrefix { get; }

        /// <summary>Blend factor between fake procedural satellite (0.0) and real Sentinel-2 satellite imagery (1.0).</summary>
        float FakeSatBlend { get; }

        /// <summary>Arma 3 world name identifier (e.g. <c>mymap</c>). Used as the WRP file name and config class name.</summary>
        string WorldName { get; }

        /// <summary>Whether to apply a colour-correction pass to the final satellite texture.</summary>
        bool UseColorCorrection { get; }
    }
}