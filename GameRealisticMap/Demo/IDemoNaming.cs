using GameRealisticMap.ManMade.Buildings;
using GameRealisticMap.ManMade.Fences;
using GameRealisticMap.ManMade.Objects;
using GameRealisticMap.ManMade.Roads;

namespace GameRealisticMap.Demo
{
    public interface IDemoNaming
    {
        string GetFenceName(FenceTypeId id);
        string GetObjectName(ObjectTypeId id);
        string GetRoadName(RoadTypeId id);
        string GetSurfaceName(BuildingTypeId id);
        string GetSurfaceName(Type type);
    }
}
