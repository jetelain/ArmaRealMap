using System.Numerics;
using GameRealisticMap.ElevationModel;
using GameRealisticMap.Geometries;
using GameRealisticMap.ManMade;
using GameRealisticMap.ManMade.Buildings;
using GameRealisticMap.ManMade.Places;
using GameRealisticMap.ManMade.Roads;
using GameRealisticMap.Nature.Ocean;

namespace GameRealisticMap.Conditions
{
    public class ConditionEvaluator
    {
        private readonly CategoryAreaData areas;
        private readonly CitiesData cities;
        private readonly ElevationData elevation;
        private readonly OceanData ocean;
        private readonly TerrainSpacialIndex<Road> roads;
        private readonly Vector2 roadSearch = new Vector2(75, 75);

        public ConditionEvaluator(IBuildContext context)
        {
            this.areas = context.GetData<CategoryAreaData>();
            this.cities = context.GetData<CitiesData>();
            this.elevation = context.GetData<ElevationData>();
            this.ocean = context.GetData<OceanData>();
            this.roads = new TerrainSpacialIndex<Road>(context.Area);
            roads.AddRange(context.GetData<RoadsData>().Roads);
        }

        public IPointConditionContext GetPointContext(TerrainPoint point, Road? road = null)
        {
            return new PointConditionContext(this, point, road);
        }

        public bool IsArea(TerrainPoint point, BuildingTypeId buildingType)
        {
            return areas.Areas.Any(a => a.BuildingType == buildingType && a.PolyList.Any(p => p.Contains(point)));
        }

        public IEnumerable<BuildingTypeId> GetAreas(TerrainPoint point)
        {
            return areas.Areas.Where(a => a.PolyList.Any(p => p.Contains(point))).Select(a => a.BuildingType);
        }

        public float GetElevation(TerrainPoint point)
        {
            return elevation.Elevation.ElevationAt(point);
        }

        public float GetSlope(TerrainPoint point)
        {
            return elevation.Elevation.SlopeAt(point);
        }

        public bool IsCity(TerrainPoint point, CityTypeId cityTypeId)
        {
            return cities.Cities.Any(c => c.Type == cityTypeId && c.Boundary.Any(p => p.Contains(point)));
        }

        public City? GetCity(TerrainPoint point)
        {
            return cities.Cities
                .Where(c => c.Boundary.Any(p => p.Contains(point))).OrderBy(c => c.Population)
                .FirstOrDefault()
                ?? cities.Cities.OrderBy(c => (c.Center.Vector - point.Vector).LengthSquared()).FirstOrDefault();
        }

        public float DistanceCityCenter(TerrainPoint point, CityTypeId cityTypeId)
        {
            var filter = cities.Cities.Where(c => c.Type == cityTypeId).ToList();
            if (filter.Count == 0)
            {
                return float.NaN;
            }
            return filter.Min(c => (c.Center.Vector - point.Vector).Length());
        }

        public float DistanceToCityCenter(TerrainPoint point)
        {
            var city = GetCity(point);
            if (city == null)
            {
                return float.NaN;
            }
            return (city.Center.Vector - point.Vector).Length();
        }

        public bool IsOcean(TerrainPoint point)
        {
            if (ocean.Polygons.Count == 0)
            {
                return false;
            }
            var elevation = GetElevation(point);
            if (elevation < -1)
            {
                return true;
            }
            if (elevation > 2.5)
            {
                return false;
            }
            if (ocean.IsIsland)
            {
                return !ocean.Land.Any(p => p.Contains(point));
            }
            return ocean.Polygons.Any(p => p.Contains(point));
        }

        public float DistanceToOcean(TerrainPoint point)
        {
            if (ocean.Polygons.Count == 0)
            {
                return float.MaxValue; // Assume very far away ocean
            }
            return ocean.Polygons.Min(p => p.Distance(point));
        }

        public IEnumerable<Road> GetRoads(TerrainPoint point)
        {
            var nearby = roads.Search(point.Vector - roadSearch, point.Vector + roadSearch);
            if (nearby.Count == 0)
            {
                return nearby;
            }
            return nearby.OrderBy(r => r.Path.Distance(point));
        }
    }
}
