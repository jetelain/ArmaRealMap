using System.Text.Json.Serialization;
using GameRealisticMap.Geometries;
using GameRealisticMap.ManMade.Roads;

namespace GameRealisticMap.ManMade.Objects
{
    public class ProceduralStreetLamp : IOrientedObject
    {
        [JsonConstructor]
        public ProceduralStreetLamp(TerrainPoint point, float angle, Road road)
        {
            Point = point;
            Angle = angle;
            Road = road;
        }

        public TerrainPoint Point { get; }

        public float Angle { get; }

        public Road? Road { get; }

        public ObjectTypeId TypeId => ObjectTypeId.StreetLamp;

        [JsonIgnore]
        public float Heading => -Angle;
    }
}
