using GameRealisticMap.Geometries;

namespace GameRealisticMap.ManMade.Objects
{
    public interface IOrientedObject
    {
        TerrainPoint Point { get; }

        float Angle { get; }

        ObjectTypeId TypeId { get; }
    }
}
