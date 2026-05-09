using GameRealisticMap.Configuration;

namespace GameRealisticMap.Generic
{
    /// <summary>
    /// JSON-serializable configuration for a generic (non-Arma3) map generation run.
    /// Contains geographic bounds and processing options loaded from a <c>.grmm</c> config file.
    /// </summary>
    public class GenericMapConfigJson
    {
        public double Resolution { get; set; } = 1;

        public float GridCellSize { get; set; } = 5;

        public int GridSize { get; set; } = 1024;

        public string? SouthWest { get; set; }

        public string? Center { get; set; }

        public string? ExportProfileFile { get; set; }

        public string? TargetDirectory { get; set; }

        public float? PrivateServiceRoadThreshold { get; set; }

        public SatelliteImageOptions? Satellite { get; set; }

        public GenericMapConfig ToMapConfig()
        {
            return new GenericMapConfig(this);
        }
    }
}
