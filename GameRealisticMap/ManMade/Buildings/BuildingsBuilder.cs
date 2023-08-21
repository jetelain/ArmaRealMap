using System;
using System.Collections.Concurrent;
using GameRealisticMap.Geometries;
using GameRealisticMap.ManMade.Roads;
using GameRealisticMap.Osm;
using GameRealisticMap.Reporting;

namespace GameRealisticMap.ManMade.Buildings
{
    internal class BuildingsBuilder : IDataBuilder<BuildingsData>
    {
        private readonly IProgressSystem progress;

        public BuildingsBuilder(IProgressSystem progress)
        {
            this.progress = progress;
        }

        public BuildingsData Build(IBuildContext context)
        {
            var roads = context.GetData<RoadsData>();
            var categorizers = context.GetData<CategoryAreaData>();

            var pass1 = DetectBuildingsBoundingRects(context.OsmSource, context.Area);
            //Preview(data, removed, pass1, "buildings-pass1.png");

            var pass2 = MergeSmallBuildings(pass1, context.Area);
            //Preview(data, removed, pass2, "buildings-pass2.png");

#if PARALLEL
            var pass3 = RemoveCollidingBuildingsParallel(pass2, context.Area);
#else
            var pass3 = RemoveCollidingBuildingsWithSI(pass2, context.Area);
#endif
            //Preview(data, removed, pass3, "buildings-pass3.png");

            var roadsIndex = new TerrainSpacialIndex<Road>(context.Area);
            roadsIndex.AddRange(roads.Roads);

            var pass4 = RoadCrop(pass3, roadsIndex);
            //Preview(data, removed, pass4, "buildings-pass4.png");

            DetectEntranceSide(pass4, roadsIndex, context.Area);

            var pass5 = DetectBuildingCategory(categorizers.Areas, pass4);
            //Preview(data, removed, pass5, "buildings-pass5.png");

            return new BuildingsData(pass5);
        }

        private void DetectEntranceSide(IReadOnlyList<BuildingCandidate> buildings, TerrainSpacialIndex<Road> roadsIndex, ITerrainArea area)
        {
            using var report = progress.CreateStep("EntranceSide", buildings.Count);
            var buildingsIndex = new TerrainSpacialIndex<BuildingCandidate>(area);
            buildingsIndex.AddRange(buildings);
            Parallel.ForEach(buildings, building =>
            {
                var closestRoads = BoxSideHelper.GetClosestList(building.Box, roadsIndex, 20, r => r.Path, r => r.Factor).ToList();

                var furthestBuildings = BoxSideHelper.GetFurthestList(building.Box, buildingsIndex, 2, b => new TerrainPath(b.Polygon.Shell), b => b != building).ToList();

                // TODO: Check that not face fences/wall/etc.

                building.EntranceSide = closestRoads.FirstOrDefault(s => furthestBuildings.Contains(s));

                if (building.EntranceSide == BoxSide.None)
                {
                    building.EntranceSide = furthestBuildings.FirstOrDefault();
                }

                report.ReportOneDone();
            });
        }

        private List<BuildingCandidate> MergeSmallBuildings(IReadOnlyList<BuildingCandidate> pass1Builidings, ITerrainArea area)
        {

            var size = 6.5f;
            var lsize = 2f;
            var mergeLimit = 100f;

            var small = new TerrainSpacialIndex<BuildingCandidate>(area);
            small.AddRange(pass1Builidings.Where(b => (b.Box.Width < size && b.Box.Height < size) || b.Box.Width < lsize || b.Box.Height < lsize));
            
            var large = new TerrainSpacialIndex<BuildingCandidate>(area);
            large.AddRange(pass1Builidings.Where(b => !((b.Box.Width < size && b.Box.Height < size) || b.Box.Width < lsize || b.Box.Height < lsize) && b.Box.Width < mergeLimit && b.Box.Width < mergeLimit));

            var heavy = pass1Builidings.Where(b => b.Box.Width >= mergeLimit || b.Box.Height >= mergeLimit).ToList();

            using (var report2 = progress.CreateStep("Heavy", heavy.Count))
            {
                foreach (var building in heavy)
                {
                    small.RemoveAll(building, s => building.Poly.Contains(s.Poly));
                    large.RemoveAll(building, s => building.Poly.Contains(s.Poly));
                    report2.ReportOneDone();
                }
            }

            using (var report2 = progress.CreateStep("Small", large.Count))
            {
                foreach (var building in large)
                {
                    if (BuildingTypeIdHelper.CanMerge(building.Category))
                    {
                        var wasUpdated = true;
                        while (wasUpdated && building.Box.Width < mergeLimit && building.Box.Width < mergeLimit)
                        {
                            wasUpdated = false;
                            foreach (var other in small.Where(building, b => b.Polygon.Intersects(building.Polygon)).ToList())
                            {
                                small.Remove(other);
                                building.AddAndMerge(other);
                                wasUpdated = true;
                            }
                        }
                    }
                    report2.ReportOneDone();
                }
            }

            var pass2 = new List<BuildingCandidate>();
            pass2.AddRange(large);
            pass2.AddRange(small);
            pass2.AddRange(heavy);
            return pass2;
        }

        private List<BuildingCandidate> RemoveCollidingBuildingsWithSI(List<TerrainPolygon> removed, List<BuildingCandidate> pass2, ITerrainArea area)
        {
            var pass3 = pass2.OrderByDescending(l => l.Box.Width * l.Box.Height).ToList();

            var merged = 0;
            var todo = new TerrainSpacialIndex<BuildingCandidate>(area);
            todo.AddRange(pass3);
            //var todo = pass3.ToList();
            var total = todo.Count;
            using var report = progress.CreateStep("Collide", total);
            while (todo.Count > 0)
            {
                var building = todo.First();
                todo.Remove(building);
                bool wasChanged;
                do
                {
                    wasChanged = false;
                    foreach (var other in todo.Where(building, o => o.Polygon.Intersects(building.Polygon)).ToList())
                    {
                        var intersectionArea = building.Polygon.IntersectionArea(other.Polygon);
                        if (intersectionArea > other.Polygon.Area * 0.15)
                        {
                            todo.Remove(other);
                            pass3.Remove(other);
                            removed.AddRange(other.Polygons);
                        }
                        else
                        {
                            var mergeSimulation = building.Box.Add(other.Box);
                            if (mergeSimulation.Polygon.Area <= (building.Polygon.Area + other.Polygon.Area - intersectionArea) * 1.05)
                            {
                                building.AddAndMerge(other);
                                pass3.Remove(other);
                                todo.Remove(other);
                                merged++;
                                wasChanged = true;
                            }
                        }
                    }
                    report.Report(total - todo.Count);
                }
                while (wasChanged);
            }
            return pass3;
        }

        private List<BuildingCandidate> RemoveCollidingBuildingsParallel(List<BuildingCandidate> pass2, ITerrainArea area)
        {
            using var report = progress.CreateStep("Collide (Parallel)", pass2.Count);
            return GeometryHelper.ParallelMerge(new Envelope(TerrainPoint.Empty, new TerrainPoint(area.SizeInMeters, area.SizeInMeters)), pass2, 100, l => RemoveCollidingBuildings(l.ToList()), report).Result.ToList();
        }

        private List<BuildingCandidate> RemoveCollidingBuildings(IReadOnlyList<BuildingCandidate> pass2)
        {
            var pass3 = pass2.OrderByDescending(l => l.Box.Width * l.Box.Height).ToList();
            var merged = 0;
            var todo = pass3.ToList();
            var total = todo.Count;
            while (todo.Count > 0)
            {
                var building = todo[0];
                todo.RemoveAt(0);
                bool wasChanged;
                do
                {
                    wasChanged = false;
                    foreach (var other in todo.Where(o => o.Polygon.Intersects(building.Polygon)).ToList())
                    {
                        var intersectionArea = building.Polygon.IntersectionArea(other.Polygon);
                        if (intersectionArea > other.Polygon.Area * 0.15)
                        {
                            todo.Remove(other);
                            pass3.Remove(other);
                        }
                        else
                        {
                            var mergeSimulation = building.Box.Add(other.Box);
                            if (mergeSimulation.Polygon.Area <= (building.Polygon.Area + other.Polygon.Area - intersectionArea) * 1.05)
                            {
                                building.AddAndMerge(other);
                                pass3.Remove(other);
                                todo.Remove(other);
                                merged++;
                                wasChanged = true;
                            }
                        }
                    }
                }
                while (wasChanged);
            }
            return pass3;
        }


        private List<BuildingCandidate> RoadCrop( IReadOnlyCollection<BuildingCandidate> pass3, TerrainSpacialIndex<Road> roadsIndex)
        {
            using var report = progress.CreateStep("Roads", pass3.Count);
            var pass4 = new ConcurrentQueue<BuildingCandidate>();

            Parallel.ForEach(pass3, building =>
            {
                var conflicts = roadsIndex
                    .Where(building.Box, r => r.Polygons.Any(p => p.Intersects(building.Box.Polygon)))
                    .ToList();
                if (conflicts.Count > 0)
                {
                    var result = building.Box.Polygon.SubstractAll(conflicts.SelectMany(r => r.Polygons)).ToList();
                    if (result.Count == 1)
                    {
                        var newbox = BoundingBox.ComputeInner(result[0].Shell.Skip(1));
                        if (newbox != null)
                        {
                            building.Box = newbox;
                            pass4.Enqueue(building);
                        }
                    }
                }
                else
                {
                    pass4.Enqueue(building);
                }
                report.ReportOneDone();
            });
            return pass4.ToList();
        }

        private List<Building> DetectBuildingCategory(IEnumerable<IBuildingCategoryArea> categorizers, IReadOnlyCollection<BuildingCandidate> pass3)
        {
            var pass4 = new ConcurrentQueue<Building>();
            var metas = categorizers
                .Where(b => b.BuildingType != BuildingTypeId.Residential)
                .ToList();

            using var report4 = progress.CreateStep("Category", pass3.Count);
            Parallel.ForEach(pass3, building =>
            {
                if (building.Category == null)
                {
                    var meta = metas.Where(m => m.PolyList.Any(p => p.Intersects(building.Polygon))).FirstOrDefault();
                    if (meta == null)
                    {
                        building.Category = BuildingTypeId.Residential;
                    }
                    else
                    {
                        building.Category = meta.BuildingType;
                    }
                }
                pass4.Enqueue(building.ToBuilding());
                report4.ReportOneDone();
            });
            return pass4.ToList();
        }

        private List<BuildingCandidate> DetectBuildingsBoundingRects(IOsmDataSource osm, ITerrainArea area)
        {
            var buildings = osm.All.Where(o => o.Tags != null && o.Tags.ContainsKey("building")).ToList();

            var areaEnveloppe = new Envelope(TerrainPoint.Empty, new TerrainPoint(area.SizeInMeters, area.SizeInMeters));

            var pass1 = new List<BuildingCandidate>();
            using var report1 = progress.CreateStep("Interpret", buildings.Count);
            foreach (var building in buildings)
            {
                foreach (var geometry in osm.Interpret(building))
                {
                    foreach (var poly in TerrainPolygon.FromGeometry(geometry, area.LatLngToTerrainPoint))
                    {
                        if (areaEnveloppe.EnveloppeContains(poly))
                        { 
                            pass1.Add(new BuildingCandidate(poly, BuildingTypeIdHelper.FromOSM(building.Tags)));
                        }
                    }
                }
                report1.ReportOneDone();
            }
            return pass1;
        }
    }
}
