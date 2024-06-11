using System.Text.Json;
using System.Text.Json.Serialization;
using GameRealisticMap.Geometries;
using GameRealisticMap.IO;
using GameRealisticMap.ManMade.Roads.Libraries;
using GameRealisticMap.Osm;
using GameRealisticMap.Reporting;

namespace GameRealisticMap.ManMade.Roads
{
    public class RoadsBuilder : IDataBuilder<RoadsData>, IDataSerializer<RoadsData>
    {
        private readonly IProgressSystem progress;
        private readonly IRoadTypeLibrary<IRoadTypeInfos> library;
        private readonly JsonSerializerOptions options;

        public RoadsBuilder(IProgressSystem progress, IRoadTypeLibrary<IRoadTypeInfos> library)
        {
            this.progress = progress;
            this.library = library;

            this.options = new JsonSerializerOptions();
            options.Converters.Add(new RoadTypeInfosConverter(library));
            options.Converters.Add(new JsonStringEnumConverter());
        }

        internal List<Road> MergeRoads(List<Road> roads)
        {
            using var report = progress.CreateStep("Merge", roads.Count);
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
            if (TerrainPoint.Equals(target.Path.LastPoint,source.Path.FirstPoint))
            {
                var a = target.Path.Points.ToList();
                var b = source.Path.Points.ToList();
                target.Path = new TerrainPath(a.Concat(b.Skip(1)).ToList());
            }
            else if (TerrainPoint.Equals(target.Path.FirstPoint,source.Path.LastPoint))
            {
                var a = source.Path.Points.ToList();
                var b = target.Path.Points.ToList();
                target.Path = new TerrainPath(a.Concat(b.Skip(1)).ToList());
            }
            else if (TerrainPoint.Equals(target.Path.LastPoint,source.Path.LastPoint))
            {
                var a = target.Path.Points.ToList();
                var b = Enumerable.Reverse(source.Path.Points).ToList();
                target.Path = new TerrainPath(a.Concat(b.Skip(1)).ToList());
            }
            else if (TerrainPoint.Equals(target.Path.FirstPoint,source.Path.FirstPoint))
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
            return roads.Where(r => r != self && r.RoadType == self.RoadType && r.SpecialSegment == self.SpecialSegment && (TerrainPoint.Equals(r.Path.FirstPoint,point) || TerrainPoint.Equals(r.Path.LastPoint,point))).ToList();
        }

        internal List<Road> PrepareRoads(IOsmDataSource osm, ITerrainArea area)
        {
            // Do not process bridges that length is not significantly larger than grid cell size, because they wont be noticiable
            var strictBridgeMinimalLength = area.GridCellSize * 1.25f;

            var osmRoads = osm.Ways.Where(o => o.Tags != null && o.Tags.ContainsKey("highway")).ToList();
            var roads = new List<Road>();
            using var report = progress.CreateStep("Interpret", osmRoads.Count);
            foreach (var road in osmRoads)
            {
                var kind = RoadTypeIdHelper.FromOSM(road.Tags);
                if (kind != null)
                {
                    var special = RoadTypeIdHelper.GetSpecialSegment(kind.Value, road.Tags);
                    var type = library.GetInfo(kind.Value);
                    foreach (var geometry in osm.Interpret(road))
                    {
                        foreach (var path in TerrainPath.FromGeometry(geometry, area.LatLngToTerrainPoint))
                        {
                            foreach (var pathSegment in path.ClippedBy(area.TerrainBounds))
                            {
                                if (special == WaySpecialSegment.Bridge)
                                {
                                    var bridgeInfos = library.GetBridge(type.Id);
                                    if (pathSegment.Length < strictBridgeMinimalLength || !bridgeInfos.HasBridge || pathSegment.Length < bridgeInfos.MinimalBridgeLength)
                                    {
                                        special = WaySpecialSegment.Normal;
                                    }
                                }
                                roads.Add(new Road(special, pathSegment, type));
                            }
                        }
                    }
                }
                report.ReportOneDone();
            }
            return roads;
        }

        public RoadsData Build(IBuildContext context)
        {
            return new RoadsData(
                IgnoreSmallIsolated(
                    MergeRoads(
                        PrepareRoads(context.OsmSource, context.Area)), 
                    context.Options)
                );
        }

        private static bool AnyConnected(Road self, IEnumerable<Road> roads)
        {
            return roads
                .Where(o => o != self && GeometryHelper.EnveloppeIntersects(self, o))
                .Any(o => o.Path.Points.Any(p => self.Path.Points.Contains(p)));
        }

        private List<Road> IgnoreSmallIsolated(List<Road> roads, IMapProcessingOptions options)
        {
            var kept = roads
                .ProgressStep(progress, "Filter")

                // Ignore small isolated roads
                .Where(road => AnyConnected(road, roads) || road.Path.Length > road.Width * 10)

                // Ignore small private roads for optimisation purproses
                // Filtered at the end to ensure all segments are merged
                .Where(road => road.SpecialSegment != WaySpecialSegment.PrivateService || road.Path.Length > options.PrivateServiceRoadThreshold)
                
                .ToList();

            return kept;
        }

        public async ValueTask<RoadsData> Read(IPackageReader package, IContext context)
        {
            return await package.ReadJson<RoadsData>("Roads.json", options);
        }

        public Task Write(IPackageWriter package, RoadsData data)
        {
            return package.WriteJson<RoadsData>("Roads.json", data, options);
        }
    }
}
