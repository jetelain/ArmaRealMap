using System.Numerics;

namespace GameRealisticMap.ManMade.Buildings
{
    public class DefaultBuildingSizeLibrary : IBuildingSizeLibrary
    {
        public IEnumerable<Vector2> GetSizes(BuildingTypeId buildingType)
        {
            yield return new Vector2(10, 10);
        }
    }
}
