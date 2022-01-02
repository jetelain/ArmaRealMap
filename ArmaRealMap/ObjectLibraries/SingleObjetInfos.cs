using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using ArmaRealMap.Geometries;

namespace ArmaRealMap.Libraries
{
    public class SingleObjetInfos : ObjetInfosBase
    {
        public string Name { get; set; }

        public float? PlacementProbability { get; set; }

        public float? PlacementRadius { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public float? ReservedRadius { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public float? MaxZ { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public float? MinZ { get; set; }

        public float GetPlacementRadius()
        {
            return PlacementRadius ?? (Math.Max(Width, Depth) / 2f);
        }

        public float GetReservedRadius()
        {
            return ReservedRadius ?? GetPlacementRadius();
        }

        internal override IEnumerable<TerrainObject> ToObjects(IBoundingShape box)
        {
            yield return new TerrainObject(this, box);
        }
    }
}