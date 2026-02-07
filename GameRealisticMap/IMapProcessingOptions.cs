using GameRealisticMap.Configuration;

namespace GameRealisticMap
{
    public interface IMapProcessingOptions
    {
        double Resolution { get; }

        float PrivateServiceRoadThreshold { get; }

        ISatelliteImageOptions Satellite { get; }
    }
}