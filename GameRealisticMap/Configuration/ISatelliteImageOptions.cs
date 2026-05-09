namespace GameRealisticMap.Configuration
{
    /// <summary>
    /// Satellite image post processing options.
    /// 
    /// Useful to ensure consistency between satellite image provider and ground textures.
    /// </summary>
    public interface ISatelliteImageOptions
    {
        float Contrast { get; }

        float Brightness { get; }

        float Saturation { get; }
    }
}
