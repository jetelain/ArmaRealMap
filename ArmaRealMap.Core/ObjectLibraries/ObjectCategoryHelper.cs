using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArmaRealMap.Core.ObjectLibraries
{
    public static class ObjectCategoryHelper
    {
        public static ObjectCategory[] SingleObject = new[] {
            ObjectCategory.IsolatedTree,
            ObjectCategory.PicnicTable,
            ObjectCategory.Bench,
            ObjectCategory.WaterWell
        };

        public static ObjectCategory[] AreaFiller = new[] {
            ObjectCategory.ForestTree,
            ObjectCategory.RandomVegetation,
            ObjectCategory.ForestAdditionalObjects,
            ObjectCategory.Scrub,
            ObjectCategory.GroundRock
        };

        public static ObjectCategory[] LineObject = new[] {
            ObjectCategory.RoadConcreteWall,
            ObjectCategory.ForestEdge,
            ObjectCategory.RoadSideWalk,
            ObjectCategory.Fence,
            ObjectCategory.Wall,
            ObjectCategory.Cliff
        };

        public static ObjectCategory[] Building = new[] { 
            ObjectCategory.Residential,
            ObjectCategory.Industrial,
            ObjectCategory.Retail,
            ObjectCategory.Military,
            ObjectCategory.HistoricalFort,
            ObjectCategory.Church,
            ObjectCategory.RadioTower,
            ObjectCategory.Hut
        };

        public static bool IsBestFit(this ObjectCategory category)
        {
            return Building.Contains(category);
        }

        public static bool HasProbability(this ObjectCategory category)
        {
            return SingleObject.Contains(category) || AreaFiller.Contains(category) || LineObject.Contains(category);
        }

        public static bool HasPlacementRadius(this ObjectCategory category)
        {
            return AreaFiller.Contains(category) || LineObject.Contains(category);
        }

        public static bool HasDensity(this ObjectCategory category)
        {
            return AreaFiller.Contains(category);
        }

        public static bool HasReservedRadius(this ObjectCategory category)
        {
            return AreaFiller.Contains(category);
        }

        public static bool HasMaxMinZ(this ObjectCategory category)
        {
            return AreaFiller.Contains(category) || LineObject.Contains(category);
        }
    }
}
