using System.Numerics;
using GameRealisticMap.Geometries;
using GameRealisticMap.ManMade.Roads;
using GameRealisticMap.Reporting;

namespace GameRealisticMap.ManMade.Railways
{
    internal class RailwaysBuilder : IDataBuilder<RailwaysData>
    {
        private readonly IProgressSystem progress;
        private readonly IRailwayCrossingResolver crossingResolver;

        public RailwaysBuilder(IProgressSystem progress, IRailwayCrossingResolver? crossingResolver = null)
        {
            this.progress = progress;
            this.crossingResolver = crossingResolver ?? new DefaultRailwayCrossingResolver();
        }

        public RailwaysData Build(IBuildContext context)
        {
            var nodes = context.OsmSource.All
                .Where(s => s.Tags != null && s.Tags.GetValue("railway") == "rail")
                .ToList();

            var crossing = context.OsmSource.Nodes
                .Where(s => s.Tags != null && s.Tags.GetValue("railway") == "level_crossing" && s.Longitude != null && s.Latitude != null)
                .Select(s => context.Area.LatLngToTerrainPoint(new GeoAPI.Geometries.Coordinate(s.Longitude ?? 0, s.Latitude ?? 0)).ToIntPointPrecision())
                .Where(p => context.Area.IsInside(p))
                .ToList();

            var roadsIndex = new TerrainSpacialIndex<Road>(context.Area);
            roadsIndex.AddRange(context.GetData<RoadsData>().Roads);

            var railways = new List<Railway>();
            foreach (var way in nodes.ProgressStep(progress, "Paths"))
            {
                foreach (var segment in context.OsmSource.Interpret(way)
                                                .SelectMany(geometry => TerrainPath.FromGeometry(geometry, context.Area.LatLngToTerrainPoint))
                                                .SelectMany(path => path.ClippedBy(context.Area.TerrainBounds)))
                {
                    var special = WaySpecialSegmentHelper.FromOSM(way.Tags);
                    if (special == WaySpecialSegment.Normal)
                    {
                        AddNormalSegment(railways, crossing, roadsIndex, segment);
                    }
                    else
                    {
                        railways.Add(new Railway(special, segment));
                    }
                }
            }

            return new RailwaysData(railways);
        }

        private void AddNormalSegment(List<Railway> railways, List<TerrainPoint> crossing, TerrainSpacialIndex<Road> roadsIndex, TerrainPath segment)
        {
            var crossingPoint = segment.Points.FirstOrDefault(p => crossing.Contains(p));
            if (crossingPoint != null)
            {
                var indexInRailway = segment.Points.IndexOf(crossingPoint);

                var road = roadsIndex.Search(crossingPoint, crossingPoint).FirstOrDefault(r => r.Path.Points.Contains(crossingPoint));

                var factor = 1f;

                if ( road != null)
                {
                    var indexInRoad = road.Path.Points.IndexOf(crossingPoint);
                    if (indexInRoad != -1)
                    {
                        factor = 1 + Math.Abs(Vector2.Dot(road.Path.GetNormalizedVectorAtIndex(indexInRoad), segment.GetNormalizedVectorAtIndex(indexInRailway)));
                    }
                }

                var crossingWidth = crossingResolver.GetCrossingWidth(road?.RoadTypeInfos, factor);

                if (crossingWidth > 0)
                {
                    // split in 3 segments


                    var pathPart1 = new FollowPath(segment.Points.Take(indexInRailway + 1).Reverse());
                    pathPart1.Move(crossingWidth / 2);
                    var part1Index = pathPart1.Index;
                    var point1 = pathPart1.Current;

                    var pathPart2 = new FollowPath(segment.Points.Skip(indexInRailway));
                    pathPart2.Move(crossingWidth / 2);
                    var part2Index = pathPart2.Index;
                    var point2 = pathPart2.Current;

                    var seg1 = new TerrainPath(segment.Points.Take(indexInRailway - part1Index + 1).Concat(new[] { point1 }).ToList());
                    var seg2 = new TerrainPath(point1, crossingPoint, point2);
                    var seg3 = new TerrainPath(new[] { point2 }.Concat(segment.Points.Skip(indexInRailway - part2Index + 2)).ToList());

                    AddNormalSegment(railways, crossing, roadsIndex, seg1);

                    railways.Add(new Railway(WaySpecialSegment.Crossing, seg2));

                    AddNormalSegment(railways, crossing, roadsIndex, seg3);

                    return;
                }
            }

            railways.Add(new Railway(WaySpecialSegment.Normal, segment));
        }
    }
}
