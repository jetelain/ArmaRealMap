using System.Text.Json.Serialization;
using GameRealisticMap.Geometries;
using GameRealisticMap.ManMade.Roads;

namespace GameRealisticMap.ManMade.Objects
{
    public class OrientedObject : IOrientedObject
    {
        [JsonConstructor]
        public OrientedObject(TerrainPoint point, float angle, ObjectTypeId typeId, Road? road = null)
        {
            Point = point;
            Angle = angle;
            TypeId = typeId;
            Road = road;
        }

        public TerrainPoint Point { get; }

        public float Angle { get; }

        public ObjectTypeId TypeId { get; }

        [JsonIgnore]
        public float Heading => -Angle;

        public Road? Road { get; }
    }
}
