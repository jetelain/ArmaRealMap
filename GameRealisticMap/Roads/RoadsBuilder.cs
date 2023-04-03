using System.Diagnostics;
using GameRealisticMap.Geometries;
using GameRealisticMap.Osm;
using GameRealisticMap.Reporting;

namespace GameRealisticMap.Roads
{
    public class RoadsBuilder : IDataBuilder<RoadsData>
    {
        private readonly IProgressSystem progress;
        private readonly IRoadTypeLibrary library;

        public RoadsBuilder(IProgressSystem progress, IRoadTypeLibrary library)
        {
            this.progress = progress;
            this.library = library;
        }

        internal List<Road> MergeRoads(List<Road> roads)
        {
            using var report = progress.CreateStep("Roads.Merge", roads.Count);
            var todo = new HashSet<Road>(roads);
            var roadsPass2 = new List<Road>();
            foreach (var road in todo.ToList())
            {
                if (todo.Contains(road))
                {
                    todo.Remove(road);
                    roadsPass2.Add(road);

                    var connectedAtLast = ConnectedAt(road, todo, road.Path.LastPoint);
                    if (connectedAtLast.Count == 1)
                    {
                        todo.Remove(connectedAtLast[0]);
                        MergeInto(road, connectedAtLast[0]);
                    }

                    var connectedAtFirst = ConnectedAt(road, todo, road.Path.FirstPoint);
                    if (connectedAtFirst.Count == 1)
                    {
                        todo.Remove(connectedAtFirst[0]);
                        MergeInto(road, connectedAtFirst[0]);
                    }
                }
                report.ReportOneDone();
            }
            return roadsPass2;
        }

        private static void MergeInto(Road target, Road source)
        {
            if (target.Path.LastPoint == source.Path.FirstPoint)
            {
                var a = target.Path.Points.ToList();
                var b = source.Path.Points.ToList();
                target.Path = new TerrainPath(a.Concat(b.Skip(1)).ToList());
            }
            else if (target.Path.FirstPoint == source.Path.LastPoint)
            {
                var a = source.Path.Points.ToList();
                var b = target.Path.Points.ToList();
                target.Path = new TerrainPath(a.Concat(b.Skip(1)).ToList());
            }
            else if (target.Path.LastPoint == source.Path.LastPoint)
            {
                var a = target.Path.Points.ToList();
                var b = Enumerable.Reverse(source.Path.Points).ToList();
                target.Path = new TerrainPath(a.Concat(b.Skip(1)).ToList());
            }
            else if (target.Path.FirstPoint == source.Path.FirstPoint)
            {
                var a = Enumerable.Reverse(target.Path.Points).ToList();
                var b = source.Path.Points.ToList();
                target.Path = new TerrainPath(a.Concat(b.Skip(1)).ToList());
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        private static List<Road> ConnectedAt(Road self, IEnumerable<Road> roads, TerrainPoint point)
        {
            return roads.Where(r => r != self && r.RoadType == self.RoadType && r.SpecialSegment == self.SpecialSegment && (r.Path.FirstPoint == point || r.Path.LastPoint == point)).ToList();
        }

        internal List<Road> PrepareRoads(IOsmDataSource osm, ITerrainArea area)
        {
            var osmRoads = osm.Ways.Where(o => o.Tags != null && o.Tags.ContainsKey("highway")).ToList();
            var roads = new List<Road>();
            using var report = progress.CreateStep("Roads.Interpret", osmRoads.Count);
            foreach (var road in osmRoads)
            {
                var kind = OsmRoadCategorizer.ToRoadType(road.Tags);
                if (kind != null)
                {
                    var type = library.GetInfo(kind.Value);
                    var count = 0;
                    foreach (var geometry in osm.Interpret(road))
                    {
                        foreach (var path in TerrainPath.FromGeometry(geometry, area.LatLngToTerrainPoint))
                        {
                            if (path.Length >= 3)
                            {
                                foreach (var pathSegment in path.ClippedBy(area.TerrainBounds))
                                {
                                    roads.Add(new Road(OsmRoadCategorizer.ToRoadSpecialSegment(road.Tags), pathSegment, type));
                                }
                            }
                        }
                        count++;
                    }
                    if (count == 0)
                    {
                        Trace.TraceWarning($"NO GEOMETRY FOR {road.Tags}");
                    }
                }
                report.ReportOneDone();
            }
            return roads;
        }

        public RoadsData Build(IBuildContext context)
        {
            return new RoadsData(MergeRoads(PrepareRoads(context.OsmSource, context.Area)));
        }
    }
}
