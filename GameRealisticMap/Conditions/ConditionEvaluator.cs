using System.ComponentModel;
using System.Numerics;
using GameRealisticMap.ElevationModel;
using GameRealisticMap.Geometries;
using GameRealisticMap.ManMade;
using GameRealisticMap.ManMade.Buildings;
using GameRealisticMap.ManMade.Places;
using GameRealisticMap.ManMade.Roads;
using GameRealisticMap.Nature.Ocean;
using WeatherStats.Stats;

namespace GameRealisticMap.Conditions
{
    public class ConditionEvaluator : IConditionEvaluator
    {
        private readonly CategoryAreaData areas;
        private readonly CitiesData cities;
        private readonly ElevationData elevation;
        private readonly OceanData ocean;
        private readonly TerrainSpacialIndex<Road> roads;
        private readonly Vector2 roadSearch = new Vector2(MaxRoadBoxSearch, MaxRoadBoxSearch);

        internal const float MaxRoadBoxSearch = 75f;
        internal const float MaxRoadDistance = MaxRoadBoxSearch * 1.414f; // MaxRoadBoxSearch * sqrt(2)

        //public ConditionEvaluator(IBuildContext context)
        //{
        //    this.areas = context.GetData<CategoryAreaData>();
        //    this.cities = context.GetData<CitiesData>();
        //    this.elevation = context.GetData<ElevationData>();
        //    this.ocean = context.GetData<OceanData>();
        //    this.roads = new TerrainSpacialIndex<Road>(context.Area);
        //    roads.AddRange(context.GetData<RoadsData>().Roads);
        //}

        public ConditionEvaluator(ITerrainArea aera, CategoryAreaData areas, CitiesData cities, ElevationData elevation, OceanData ocean, RoadsData roads)
        {
            this.areas = areas;
            this.cities = cities;
            this.elevation = elevation;
            this.ocean = ocean;
            this.roads = new TerrainSpacialIndex<Road>(aera);
            this.roads.AddRange(roads.Roads);
        }

        public static async Task<ConditionEvaluator> CreateAsync(IBuildContext context)
        {
            var areas = context.GetDataAsync<CategoryAreaData>();
            var cities = context.GetDataAsync<CitiesData>();
            var elevation = context.GetDataAsync<ElevationData>();
            var ocean = context.GetDataAsync<OceanData>();
            var roads = context.GetDataAsync<RoadsData>();
            return new ConditionEvaluator(context.Area, await areas, await cities, await elevation, await ocean, await roads);
        }

        public IPointConditionContext GetPointContext(TerrainPoint point, Road? road = null)
        {
            return new PointConditionContext(this, point, road);
        }

        public IPathConditionContext GetPathContext(TerrainPath path)
        {
            return new PathConditionContext(this, path);
        }

        public IPolygonConditionContext GetPolygonContext(TerrainPolygon polygon)
        {
            return new PolygonConditionContext(this, polygon);
        }

        public bool IsArea(TerrainPoint point, BuildingTypeId buildingType)
        {
            return areas.Areas.Any(a => a.BuildingType == buildingType && a.PolyList.Any(p => p.ContainsIncludingBorder(point)));
        }

        public IEnumerable<BuildingTypeId> GetAreas(TerrainPoint point)
        {
            return areas.Areas.Where(a => a.PolyList.Any(p => p.ContainsIncludingBorder(point))).Select(a => a.BuildingType);
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
            return cities.Cities.Any(c => c.Type == cityTypeId && c.Boundary.Any(p => p.ContainsIncludingBorder(point)));
        }

        public City? GetCity(TerrainPoint point)
        {
            return cities.Cities
                .Where(c => c.Boundary.Any(p => p.ContainsIncludingBorder(point))).OrderBy(c => c.Population)
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
            if (elevation < -3) // Lake can be up to 2.5m deep, but is not formally ocean
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

        public MinMaxAvg GetElevation(TerrainPath path)
        {
            return MinMaxAvg.From(elevation.Elevation.GetElevationOnPath(path.Points));
        }

        public MinMaxAvg GetElevation(TerrainPolygon polygon)
        {
            var around = elevation.Elevation.GetElevationOnPath(polygon.Shell).Concat(polygon.Holes.SelectMany(elevation.Elevation.GetElevationOnPath));
            var inside = elevation.Elevation.GetElevationInside(polygon).ToList();
            var ext = MinMaxAvg.From(around.Concat(inside));
            if (inside.Count > 0)
            {
                return new MinMaxAvg(ext.Min, inside.Average(), ext.Max);
            }
            var centroid = elevation.Elevation.ElevationAround(polygon.Centroid);
            return new MinMaxAvg(Math.Min(ext.Min, centroid), centroid, Math.Max(ext.Max, centroid));
        }

        internal bool IsArea(TerrainPolygon polygon, BuildingTypeId typeId)
        {
            var match = GetAreas(polygon, typeId);
            if (match.Count > 0)
            {
                var outside = polygon.SubstractAll(match).Sum(p => p.Area);
                return outside < (polygon.Area / 4);
            }
            return false;
        }

        internal bool IsArea(TerrainPath path, BuildingTypeId typeId)
        {
            var match = GetAreas(path, typeId);
            if (match.Count > 0)
            {
                var outside = path.SubstractAll(match).Sum(p => p.Length);
                return outside < (path.Length / 3);
            }
            return false;
        }

        private List<TerrainPolygon> GetAreas(ITerrainEnvelope envelope, BuildingTypeId typeId)
        {
            return areas.Areas.Where(a => a.BuildingType == typeId)
                .SelectMany(a => a.PolyList)
                .Where(p => p.EnveloppeIntersects(envelope))
                .ToList();
        }
    }
}
