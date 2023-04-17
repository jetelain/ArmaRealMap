using OsmSharp.Tags;

namespace GameRealisticMap.Buildings
{
    internal static class BuildingTypeIdHelper
    {
        internal static bool CanMerge(BuildingTypeId? category)
        {
            if (category != null)
            {
                if (category.Value == BuildingTypeId.Hut || category.Value == BuildingTypeId.Shed)
                {
                    return false;
                }
            }
            return true;
        }

        internal static BuildingTypeId? FromOSM(TagsCollectionBase tags)
        {

            if (tags.ContainsKey("building") && !tags.IsFalse("building"))
            {
                if (tags.GetValue("tower:type") == "communication")
                {
                    return BuildingTypeId.RadioTower;
                }
                switch (tags.GetValue("building"))
                {
                    case "church":
                        return BuildingTypeId.Church;

                    case "hut":
                        return BuildingTypeId.Hut;

                    case "shed":
                        return BuildingTypeId.Shed;

                    case "commercial":
                        return BuildingTypeId.Commercial;

                    case "retail":
                        return BuildingTypeId.Retail;

                    case "farm_auxiliary":
                    case "barn":
                    case "owshed":
                        return BuildingTypeId.Agricultural;

                    case "water_tower":
                        return BuildingTypeId.WaterTower;

                }
                if (tags.GetValue("historic") == "fort")
                {
                    return BuildingTypeId.HistoricalFort;
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