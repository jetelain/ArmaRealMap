using System.Text.Json.Serialization;

namespace GameRealisticMap.Arma3.Assets
{
    public class RailwaysDefinition
    {
        [JsonConstructor]
        public RailwaysDefinition(List<StraightSegmentDefinition> straights)
        {
            Straights = straights;
        }

        public List<StraightSegmentDefinition> Straights { get; }
    }
}