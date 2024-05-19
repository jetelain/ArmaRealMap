using OsmSharp.Tags;

namespace GameRealisticMap.ManMade.Buildings
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
            if (tags.GetValue("shelter_type") == "public_transport")
            {
                return BuildingTypeId.BusStopShelter;
            }
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
                        if (tags.GetValue("public_transport") == "platform")
                        {
                            return BuildingTypeId.BusStopShelter;
                        }
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

                    case "garage":
                        return BuildingTypeId.IndividualGarage;

                    case "house":
                    case "detached":
                    case "semidetached_house":
                    case "farm":
                    case "terrace":
                        return BuildingTypeId.Residential;
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
            switch (tags.GetValue("man_made"))
            {
                case "lighthouse":
                    return BuildingTypeId.Lighthouse;

                case "water_tower":
                    return BuildingTypeId.WaterTower;
            }

            if (tags.GetValue("power") == "generator" && tags.GetValue("generator:source") == "wind")
            {
                var method = tags.GetValue("generator:method");
                if ( method == "wind_turbine" || string.IsNullOrEmpty(method))
                {
                    return BuildingTypeId.WindTurbine;
                }
            }

            return null;
        }
    }
}