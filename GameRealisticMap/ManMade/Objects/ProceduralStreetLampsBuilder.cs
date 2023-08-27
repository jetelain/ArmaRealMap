using System.Numerics;
using GameRealisticMap.Geometries;
using GameRealisticMap.ManMade.Roads;
using GameRealisticMap.Reporting;

namespace GameRealisticMap.ManMade.Objects
{
    internal class ProceduralStreetLampsBuilder : IDataBuilder<ProceduralStreetLampsData>
    {
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
            foreach(var o in nonProcedural)
            {
                index.Insert(o.Point.Vector, o);
            }

            var allRoads = context.GetData<RoadsData>().Roads;
            var roadsWithLamps = allRoads.Where(r => r.RoadTypeInfos.HasStreetLamp).ToList();
            var lamps = new List<ProceduralStreetLamp>();

            var roadsPolygon = allRoads.SelectMany(r => r.ClearPolygons).ToList();

            foreach(var road in roadsWithLamps.ProgressStep(progress, "Roads"))
            {
                var spacing = road.ClearWidth * 2.5f;
                var margin = new Vector2(road.ClearWidth * 1.5f);

                var paths = road.Path.ToTerrainPolygon(road.ClearWidth + 0.1f)
                    .SelectMany(p => p.Holes.Concat(new[] { p.Shell }))
                    .Select(p => new TerrainPath(p))
                    .SelectMany(p => p.SubstractAll(roadsPolygon))
                    .Where(p => p.Length > spacing)
                    .ToList();

                foreach(var path in paths)
                {
                    foreach (var point in GeometryHelper.PointsOnPathRegular(path.Points, spacing))
                    {
                        if (index.Search(point.Vector - margin, point.Vector + margin).Count == 0)
                        { 
                            var angle = GeometryHelper.GetFacing(point, new[] { road.Path }, spacing) ?? OrientedObjectBuilder.GetRandomAngle(point);
                            var lamp = new ProceduralStreetLamp(point, angle);
                            lamps.Add(lamp);
                            index.Insert(point.Vector, lamp);
                        }
                    }
                }
            }

            return new ProceduralStreetLampsData(lamps);
        }
    }
}
