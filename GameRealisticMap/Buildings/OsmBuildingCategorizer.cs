using OsmSharp.Tags;

namespace GameRealisticMap.Buildings
{
    internal static class OsmBuildingCategorizer
    {
        internal static BuildingTypeId? ToBuildingType(TagsCollectionBase tags)
        {
            if (tags.ContainsKey("building") && !tags.IsFalse("building"))
            {
                switch (tags.GetValue("building"))
                {
                    case "church":
                        return BuildingTypeId.Church;

                    case "hut":
                        return BuildingTypeId.Hut;
                }
                if (tags.GetValue("historic") == "fort")
                {
                    return BuildingTypeId.HistoricalFort;
                }
                if (tags.GetValue("tower:type") == "communication")
                {
                    return BuildingTypeId.RadioTower;
                }
                if (tags.ContainsKey("brand"))
                {
                    return BuildingTypeId.Retail;
                }
            }
            return null;
        }
    }
}