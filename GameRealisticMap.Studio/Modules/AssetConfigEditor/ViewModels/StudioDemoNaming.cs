using System;
using GameRealisticMap.Demo;
using GameRealisticMap.ManMade.Buildings;
using GameRealisticMap.ManMade.Farmlands;
using GameRealisticMap.ManMade.Fences;
using GameRealisticMap.ManMade.Objects;
using GameRealisticMap.ManMade.Roads;
using GameRealisticMap.Nature.Forests;
using GameRealisticMap.Nature.Lakes;
using GameRealisticMap.Nature.RockAreas;
using GameRealisticMap.Nature.Scrubs;
using GameRealisticMap.Nature.Surfaces;
using GameRealisticMap.Nature.Watercourses;

namespace GameRealisticMap.Studio.Modules.AssetConfigEditor.ViewModels
{
    internal class StudioDemoNaming : IDemoNaming
    {
        public string GetBuildingName(BuildingTypeId id)
        {
            var idText = id.ToString();
            return Labels.ResourceManager.GetString("Asset" + idText) ?? idText;
        }

        public string GetFenceName(FenceTypeId id)
        {
            var idText = id.ToString();
            return Labels.ResourceManager.GetString("Asset" + idText) ?? idText;
        }

        public string GetObjectName(ObjectTypeId id)
        {
            var idText = id.ToString();
            return Labels.ResourceManager.GetString("Asset" + idText) ?? idText;
        }

        public string GetRoadName(RoadTypeId id)
        {
            var idText = id.ToString();
            return Labels.ResourceManager.GetString("Asset" + idText) ?? idText;
        }

        public string GetSurfaceName(BuildingTypeId id)
        {
            switch(id)
            {
                case BuildingTypeId.Industrial:
                    return Labels.AssetDefaultIndustrial;
                default:
                    return Labels.AssetDefaultUrban;
            }
        }

        public string GetSurfaceName(Type type)
        {
            if ( type == typeof(ForestData))
            {
                return Labels.Forest;
            }
            if (type == typeof(MeadowsData))
            {
                return Labels.AssetMeadow;
            }
            if (type == typeof(SandSurfacesData))
            {
                return Labels.AssetSand;
            }
            if (type == typeof(RocksData))
            {
                return Labels.AssetRocks;
            }
            if (type == typeof(GrassData))
            {
                return Labels.AssetGrass;
            }
            if (type == typeof(LakesData))
            {
                return Labels.Lakes;
            }
            if (type == typeof(WatercoursesData))
            {
                return Labels.Watercourses;
            }
            if (type == typeof(FarmlandsData))
            {
                return Labels.AssetFarmLand;
            }
            if (type == typeof(ScrubData))
            {
                return Labels.AssetScrub;
            }
            return type.Name.Replace("Data", "");
        }
    }
}