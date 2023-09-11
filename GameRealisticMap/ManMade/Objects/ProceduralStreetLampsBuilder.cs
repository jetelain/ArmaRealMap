using System.Numerics;
using GameRealisticMap.Geometries;
using GameRealisticMap.ManMade.Railways;
using GameRealisticMap.ManMade.Roads;
using GameRealisticMap.ManMade.Roads.Libraries;
using GameRealisticMap.Reporting;

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

        private readonly IProgressSystem progress;

        public ProceduralStreetLampsBuilder(IProgressSystem progress)
        {
            this.progress = progress;
        }

        public ProceduralStreetLampsData Build(IBuildContext context)
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

            var urbanMask = CreateUrbanMask(context, roadsWithLamps, everywhereMask, urban);

            var nearUrbanMask = CreateNearUrbanMask(context, roadsWithLamps, everywhereMask, urban, urbanMask);

            foreach (var road in roadsWithLamps.ProgressStep(progress, "Roads"))
            {
                var spacing = road.RoadTypeInfos.DistanceBetweenStreetLamps;
                var marginDistance = spacing / 2;
                var margin = new Vector2(marginDistance);

                var mask = GetMask(road.RoadTypeInfos.ProceduralStreetLamps, everywhereMask, urbanMask, nearUrbanMask);

                var paths = road.Path.ToTerrainPolygon(road.ClearWidth + 0.1f)
                    .SelectMany(p => p.Holes.Concat(new[] { p.Shell }))
                    .Select(p => new TerrainPath(p))
                    .SelectMany(p => p.SubstractAll(mask))
                    .Where(p => p.Length > spacing)
                    .ToList();

                foreach (var path in paths)
                {
                    foreach (var point in GeometryHelper.PointsOnPathRegular(path.Points, spacing))
                    {
                        if (!index.Search(point.Vector - margin, point.Vector + margin).Any(o => (o.Point.Vector - point.Vector).Length() < marginDistance))
                        {
                            var angle = GeometryHelper.GetFacing(point, new[] { road.Path }, spacing) ?? OrientedObjectBuilder.GetRandomAngle(point);
                            var lamp = new ProceduralStreetLamp(point, angle, road);
                            lamps.Add(lamp);
                            index.Insert(point.Vector, lamp);
                        }
                    }
                }
            }

            return new ProceduralStreetLampsData(lamps);
        }

        private List<TerrainPolygon> CreateNearUrbanMask(IBuildContext context, List<Road> roadsWithLamps, List<TerrainPolygon> everywhereMask, IEnumerable<TerrainPolygon> urban, List<TerrainPolygon> urbanMask)
        {
            if (!roadsWithLamps.Any(e => e.RoadTypeInfos.ProceduralStreetLamps == StreetLampsCondition.NearUrbanAreas))
            {
                return everywhereMask;
            }
            using var report = progress.CreateStep("CreateNearUrbanMask", 1);
            return everywhereMask.Concat(context.Area.TerrainBounds.SubstractAllSplitted(urban.SelectMany(u => u.Offset(NearUrbanMargin)))).ToList();
        }

        private List<TerrainPolygon> CreateUrbanMask(IBuildContext context, List<Road> roadsWithLamps, List<TerrainPolygon> everywhereMask, IEnumerable<TerrainPolygon> urban)
        {
            if (!roadsWithLamps.Any(e => e.RoadTypeInfos.ProceduralStreetLamps == StreetLampsCondition.UrbanAreas))
            {
                return everywhereMask;
            }
            using var report = progress.CreateStep("CreateUrbanMask", 1);
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
