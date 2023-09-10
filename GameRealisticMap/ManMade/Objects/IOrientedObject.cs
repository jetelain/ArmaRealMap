using GameRealisticMap.Geometries;
using GameRealisticMap.ManMade.Roads;

namespace GameRealisticMap.ManMade.Objects
{
    public interface IOrientedObject
    {
        TerrainPoint Point { get; }

        float Angle { get; }

        Road? Road { get; }

        ObjectTypeId TypeId { get; }
    }
}
