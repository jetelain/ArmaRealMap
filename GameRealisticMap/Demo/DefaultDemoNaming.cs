using GameRealisticMap.ManMade.Buildings;
using GameRealisticMap.ManMade.Fences;
using GameRealisticMap.ManMade.Objects;
using GameRealisticMap.ManMade.Roads;

namespace GameRealisticMap.Demo
{
    public class DefaultDemoNaming : IDemoNaming
    {
        public string GetFenceName(FenceTypeId id)
        {
            return id.ToString();
        }

        public string GetObjectName(ObjectTypeId id)
        {
            return id.ToString();
        }

        public string GetRoadName(RoadTypeId id)
        {
            return id.ToString();
        }

        public string GetSurfaceName(BuildingTypeId id)
        {
            return id.ToString();
        }

        public string GetSurfaceName(Type type)
        {
            return type.Name.Replace("Data", "");
        }
    }
}
