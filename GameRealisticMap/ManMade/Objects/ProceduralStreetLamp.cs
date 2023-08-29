using System.Text.Json.Serialization;
using GameRealisticMap.Geometries;

namespace GameRealisticMap.ManMade.Objects
{
    public class ProceduralStreetLamp : IOrientedObject
    {
        [JsonConstructor]
        public ProceduralStreetLamp(TerrainPoint point, float angle)
        {
            Point = point;
            Angle = angle;
        }

        public TerrainPoint Point { get; }

        public float Angle { get; }

        public ObjectTypeId TypeId => ObjectTypeId.StreetLamp;

        [JsonIgnore]
        public float Heading => -Angle;
    }
}
