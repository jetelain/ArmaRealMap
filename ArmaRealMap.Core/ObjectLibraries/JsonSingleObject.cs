using System.Text.Json.Serialization;

namespace ArmaRealMap.Core.ObjectLibraries
{
    public class SingleObjet
    {
        public string Name { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string ClusterName { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public float? PlacementProbability { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public float? PlacementRadius { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public float? ReservedRadius { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public float? MaxZ { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public float? MinZ { get; set; }

        public float Width { get; set; }
        public float Depth { get; set; }
        public float Height { get; set; }
        public float CX { get; set; }
        public float CY { get; set; }
        public float CZ { get; set; }
    }
}