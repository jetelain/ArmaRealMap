using GameRealisticMap.Geometries;
using Pmad.ProgressTracking;

namespace GameRealisticMap.Nature.Ocean
{
    internal class OceanBuilder : IDataBuilder<OceanData>
    {
        public OceanData Build(IBuildContext context, IProgressScope scope)
        {
            var coastlines = context.OsmSource.Ways
                .Where(w => w.Tags != null && w.Tags.GetValue("natural") == "coastline")
                .SelectMany(w => context.OsmSource.Interpret(w))
                .SelectMany(w => TerrainPath.FromGeometry(w, context.Area.LatLngToTerrainPoint))
                .ToList();
            coastlines = MergeOriented(coastlines, scope)
                .SelectMany(p => p.ClippedKeepOrientation(context.Area.TerrainBounds))
                .ToList();

            if ( coastlines.Count == 0)
            {
                // No coastlines, assume land-only
                return new OceanData(new List<TerrainPolygon>(0), new List<TerrainPolygon>(1) { context.Area.TerrainBounds }, false);
            }

            coastlines = CompleteCounterClockWiseOnEdges(coastlines, context.Area.TerrainBounds);

            var mergedCoastlines = MergeOriented(coastlines, scope)
                .Where(r => r.IsClosed)
                .ToList();

            // CounterClockWise => Lands
            // ClockWise => Ocean

            var oceanBaseLine = context.Area.TerrainBounds.
                SubstractAll(mergedCoastlines.Where(r => r.IsCounterClockWise).Select(l => l.ToPolygon()));

            var oceanPolygons = TerrainPolygon.MergeAll(oceanBaseLine.Concat(mergedCoastlines.Where(r => r.IsClockWise).Select(l => l.ToPolygon())).ToList());

            var landPolygons = context.Area.TerrainBounds.SubstractAll(oceanPolygons).ToList();

            return new OceanData(oceanPolygons, landPolygons, IsIsland(oceanPolygons, context.Area));
        }

        internal static bool IsIsland(List<TerrainPolygon> oceanPolygons, ITerrainArea area)
        {
            if (oceanPolygons.Count == 1)
            {
                var outer = oceanPolygons[0].Shell;
                return (new TerrainPolygon(outer).Area / area.TerrainBounds.Area) > 0.99;
            }
            return false;
        }

        private static List<TerrainPath> CompleteCounterClockWiseOnEdges(IEnumerable<TerrainPath> sourcePaths, ITerrainEnvelope envelope)
        {
            var paths = sourcePaths.Select(p => new MutableTerrainPath(p)).ToList();
            CompleteCounterClockWiseOnEdges(paths, envelope.MinPoint, envelope.MaxPoint);
            return paths.Select(p => p.ToPath()).ToList();
        }

        private static void CompleteCounterClockWiseOnEdges(IEnumerable<MutableTerrainPath> paths, TerrainPoint edgeSW, TerrainPoint edgeNE)
        {            
            foreach (var line in paths)
            {
                if (!line.IsClosed)
                {
                    if (line.FirstPoint.Y == edgeNE.Y) // First On North, look EAST
                    {
                        LookEast(edgeSW, edgeNE, paths, line, line.FirstPoint);
                    }
                    else if (line.FirstPoint.X == edgeNE.X) // First On East, look SOUTH
                    {
                        LookSouth(edgeSW, edgeNE, paths, line, line.FirstPoint);
                    }
                    else if (line.FirstPoint.Y == edgeSW.Y) // First On South, look WEST
                    {
                        LookWest(edgeSW, edgeNE, paths, line, line.FirstPoint);
                    }
                    else if (line.FirstPoint.X == edgeSW.X) // First On West, look North
                    {
                        LookNorth(edgeSW, edgeNE, paths, line, line.FirstPoint);
                    }
                }
            }
        }

        private static void LookEast(TerrainPoint edgeSW, TerrainPoint edgeNE, IEnumerable<MutableTerrainPath> notClosed, MutableTerrainPath line, TerrainPoint lookFrom, int depth = 0)
        {
            var other = notClosed
                .Where(n => !n.IsClosed && n.LastPoint.Y == edgeNE.Y && n.LastPoint.X > lookFrom.X)
                .OrderBy(n => n.LastPoint.X)
                .FirstOrDefault();
            if (other == line)
            {
                line.Points.Add(line.FirstPoint);
            }
            else if (other != null)
            {
                line.Points.Insert(0, other.LastPoint);
            }
            else if (other == null && depth < 4)
            {
                line.Points.Insert(0, edgeNE);
                LookSouth(edgeSW, edgeNE, notClosed, line, edgeNE, depth + 1);
            }
        }

        private static void LookSouth(TerrainPoint edgeSW, TerrainPoint edgeNE, IEnumerable<MutableTerrainPath> notClosed, MutableTerrainPath line, TerrainPoint lookFrom, int depth = 0)
        {
            var other = notClosed
                .Where(n => !n.IsClosed && n.LastPoint.X == edgeNE.X && n.LastPoint.Y < lookFrom.Y)
                .OrderByDescending(n => n.LastPoint.Y)
                .FirstOrDefault();
            if (other == line)
            {
                line.Points.Add(line.FirstPoint);
            }
            else if (other != null)
            {
                line.Points.Insert(0, other.LastPoint);
            }
            else if (other == null && depth < 4)
            {
                var southEast = new TerrainPoint(edgeNE.X, edgeSW.Y);
                line.Points.Insert(0, southEast);
                LookWest(edgeSW, edgeNE, notClosed, line, southEast, depth + 1);
            }
        }

        private static void LookWest(TerrainPoint edgeSW, TerrainPoint edgeNE, IEnumerable<MutableTerrainPath> notClosed, MutableTerrainPath line, TerrainPoint lookFrom, int depth = 0)
        {
            var other = notClosed
                .Where(n => !n.IsClosed && n.LastPoint.Y == edgeSW.Y && n.LastPoint.X < lookFrom.X)
                .OrderByDescending(n => n.LastPoint.X)
                .FirstOrDefault();
            if (other == line)
            {
                line.Points.Add(line.FirstPoint);
            }
            else if (other != null)
            {
                line.Points.Insert(0, other.LastPoint);
            }
            else if (other == null && depth < 4)
            {
                line.Points.Insert(0, edgeSW);
                LookNorth(edgeSW, edgeNE, notClosed, line, edgeSW, depth + 1);
            }
        }

        private static void LookNorth(TerrainPoint edgeSW, TerrainPoint edgeNE, IEnumerable<MutableTerrainPath> notClosed, MutableTerrainPath line, TerrainPoint lookFrom, int depth = 0)
        {
            var other = notClosed
                .Where(n => !n.IsClosed && n.LastPoint.X == edgeSW.X && n.LastPoint.Y > lookFrom.Y)
                .OrderBy(n => n.LastPoint.Y)
                .FirstOrDefault();
            if (other == line)
            {
                line.Points.Add(line.FirstPoint);
            }
            else if (other != null)
            {
                line.Points.Insert(0, other.LastPoint);
            }
            else if (other == null && depth < 4)
            {
                var northWest = new TerrainPoint(edgeSW.X, edgeNE.Y);
                line.Points.Insert(0, northWest);
                LookEast(edgeSW, edgeNE, notClosed, line, northWest, depth + 1);
            }
        }

        internal List<TerrainPath> MergeOriented(List<TerrainPath> paths, IProgressScope scope)
        {
            using var report = scope.CreateInteger("MergeOriented", paths.Count);
            var todo = new HashSet<TerrainPath>(paths);
            var result = new List<TerrainPath>();
            foreach (var wpath in todo.ToList())
            {
                var path = wpath;
                if (todo.Contains(path))
                {
                    todo.Remove(path);
                    if (!path.IsClosed)
                    {
                        var other = GetMergeable(paths, todo, path);
                        while (other != null)
                        {
                            todo.Remove(other);
                            if (TerrainPoint.Equals(path.LastPoint, other.FirstPoint))
                            {
                                path = new TerrainPath(path.Points.Concat(other.Points.Skip(1)).ToList());
                            }
                            else
                            {
                                path = new TerrainPath(other.Points.Take(other.Points.Count - 1).Concat(path.Points).ToList());
                            }
                            other = GetMergeable(paths, todo, path);
                        }
                    }
                    result.Add(path);
                    report.ReportOneDone();
                }
                
            }
            return result;
        }


        private static TerrainPath? GetMergeable(List<TerrainPath> paths, HashSet<TerrainPath> todo, TerrainPath path)
        {
            return paths.FirstOrDefault(other => other != path && !other.IsClosed && todo.Contains(other) && (TerrainPoint.Equals(other.LastPoint, path.FirstPoint) || TerrainPoint.Equals(other.FirstPoint, path.LastPoint)));
        }



    }
}
