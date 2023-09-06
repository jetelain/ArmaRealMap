using System.Numerics;

namespace GameRealisticMap.ManMade.Buildings
{
    public interface IBuildingSizeLibrary
    {
        IEnumerable<Vector2> GetSizes(BuildingTypeId buildingType);
    }
}
