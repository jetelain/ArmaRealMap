using System.Numerics;
using GameRealisticMap.Algorithms;
using GameRealisticMap.Geometries;
using GameRealisticMap.ManMade.Railways;
using GameRealisticMap.ManMade.Roads;
using GameRealisticMap.ManMade.Roads.Libraries;
using Pmad.ProgressTracking;

namespace GameRealisticMap.ManMade.Objects
{
    internal class ProceduralStreetLampsBuilder : IDataBuilder<ProceduralStreetLampsData>
    {
        private const float NearUrbanMargin = 500;

        internal static RoadTypeId[] RoadsWithLamps = new[] {
            RoadTypeId.TwoLanesPrimaryRoad,
            RoadTypeId.TwoLanesSecondaryRoad,
            RoadTypeId.TwoLanesConcreteRoad,
            RoadTypeId.SingleLaneConcreteRoad,
        };

        public ProceduralStreetLampsData Build(IBuildContext context, IProgressScope scope)
        {
            // Build Index of non-procedural elements
            var index = new SimpleSpacialIndex<IOrientedObject>(Vector2.Zero, new Vector2(context.Area.SizeInMeters));
            var nonProcedural = context.GetData<OrientedObjectData>().Objects.Where(o => o.TypeId == ObjectTypeId.StreetLamp).ToList();
            foreach (var o in nonProcedural)
            {
                index.Insert(o.Point.Vector, o);
            }

            var allRoads = context.GetData<RoadsData>().Roads;
            var roadsWithLamps = allRoads.Where(r => r.RoadTypeInfos.ProceduralStreetLamps != StreetLampsCondition.None).OrderBy(r => r.RoadType).ToList();
            var lamps = new List<ProceduralStreetLamp>();

            var everywhereMask = allRoads.SelectMany(r => r.ClearPolygons).ToList();
            everywhereMask.AddRange(context.GetData<RailwaysData>().Railways.SelectMany(r => r.ClearPolygons));

            var urban = context.GetData<CategoryAreaData>().Areas.SelectMany(a => a.PolyList);

            var urbanMask = CreateUrbanMask(context, roadsWithLamps, everywhereMask, urban, scope);

            var nearUrbanMask = CreateNearUrbanMask(context, roadsWithLamps, everywhereMask, urban, urbanMask, scope);

            foreach (var road in roadsWithLamps.WithProgress(scope, "Roads"))
            {
                var spacingMin = road.RoadTypeInfos.DistanceBetweenStreetLamps;
                var spacingMax = road.RoadTypeInfos.DistanceBetweenStreetLampsMax;
                var marginDistance = spacingMin / 2;
                var margin = new Vector2(marginDistance);

                var mask = GetMask(road.RoadTypeInfos.ProceduralStreetLamps, everywhereMask, urbanMask, nearUrbanMask);

                var paths = road.Path.ToTerrainPolygon(road.ClearWidth + 0.1f)
                    .SelectMany(p => p.Holes.Concat(new[] { p.Shell }))
                    .Select(p => new TerrainPath(p))
                    .SelectMany(p => p.SubstractAll(mask))
                    .Where(p => p.Length > spacingMin)
                    .ToList();

                foreach (var path in paths)
                {
                    var rnd = RandomHelper.CreateRandom(path.FirstPoint);
                    var follow = new FollowPath(path.Points);
                    var nextMove = RandomHelper.GetBetween(rnd, spacingMin, spacingMax);
                    do
                    {
                        var point = follow.Current;
                        if (!index.Search(point.Vector - margin, point.Vector + margin).Any(o => (o.Point.Vector - point.Vector).Length() < marginDistance))
                        {
                            var angle = GeometryHelper.GetFacing(point, new[] { road.Path }, spacingMin) ?? OrientedObjectBuilder.GetRandomAngle(point);
                            var lamp = new ProceduralStreetLamp(point, angle, road);
                            lamps.Add(lamp);
                            index.Insert(point.Vector, lamp);
                            nextMove = RandomHelper.GetBetween(rnd, spacingMin, spacingMax);
                        }
                        else
                        {
                            nextMove = marginDistance;
                        }
                    }
                    while (follow.Move(nextMove));
                }
            }

            return new ProceduralStreetLampsData(lamps);
        }

        private List<TerrainPolygon> CreateNearUrbanMask(IBuildContext context, List<Road> roadsWithLamps, List<TerrainPolygon> everywhereMask, IEnumerable<TerrainPolygon> urban, List<TerrainPolygon> urbanMask, IProgressScope scope)
        {
            if (!roadsWithLamps.Any(e => e.RoadTypeInfos.ProceduralStreetLamps == StreetLampsCondition.NearUrbanAreas))
            {
                return everywhereMask;
            }
            using var report = scope.CreateSingle("CreateNearUrbanMask");
            return everywhereMask.Concat(context.Area.TerrainBounds.SubstractAllSplitted(urban.SelectMany(u => u.Offset(NearUrbanMargin)))).ToList();
        }

        private List<TerrainPolygon> CreateUrbanMask(IBuildContext context, List<Road> roadsWithLamps, List<TerrainPolygon> everywhereMask, IEnumerable<TerrainPolygon> urban, IProgressScope scope)
        {
            if (!roadsWithLamps.Any(e => e.RoadTypeInfos.ProceduralStreetLamps == StreetLampsCondition.UrbanAreas))
            {
                return everywhereMask;
            }
            using var report = scope.CreateSingle("CreateUrbanMask");
            return everywhereMask.Concat(context.Area.TerrainBounds.SubstractAllSplitted(urban)).ToList();
        }

        private static List<TerrainPolygon> GetMask(StreetLampsCondition condition, List<TerrainPolygon> everywhereMask, List<TerrainPolygon> urbanMask, List<TerrainPolygon> nearUrbanMask)
        {
            if (condition == StreetLampsCondition.NearUrbanAreas)
            {
                return nearUrbanMask;
            }
            if (condition == StreetLampsCondition.UrbanAreas)
            {
                return urbanMask;
            }
            return everywhereMask;
        }
    }
}
