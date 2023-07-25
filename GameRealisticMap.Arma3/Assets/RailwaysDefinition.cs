using System.Text.Json.Serialization;
using GameRealisticMap.ManMade.Railways;
using GameRealisticMap.ManMade.Roads.Libraries;

namespace GameRealisticMap.Arma3.Assets
{
    public class RailwaysDefinition : IRailwayCrossingResolver
    {
        [JsonConstructor]
        public RailwaysDefinition(List<StraightSegmentDefinition> straights, List<RailwayCrossingDefinition> crossings)
        {
            Straights = straights;
            Crossings = crossings;
        }

        public List<StraightSegmentDefinition> Straights { get; }

        public List<RailwayCrossingDefinition> Crossings { get; }

        public float GetCrossingWidth(IRoadTypeInfos? roadTypeInfos, float factor)
        {
            if (Crossings.Count == 0)
            {
                return 0f;
            }

            var wanted = (roadTypeInfos?.ClearWidth ?? 6f) * factor;

            var crossing = Crossings.OrderBy(c => c.RoadMaxSize).FirstOrDefault(c => c.RoadMaxSize > wanted)
                ?? Crossings.OrderByDescending(c => c.RoadMaxSize).First();

            return crossing.Size;
        }
    }
}