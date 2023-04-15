using System.Numerics;
using GameRealisticMap.Geometries;
using GameRealisticMap.Osm;
using GameRealisticMap.Reporting;
using GameRealisticMap.Roads;

namespace GameRealisticMap.Buildings
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

            var removed = new List<TerrainPolygon>();

            var pass1 = DetectBuildingsBoundingRects(context.OsmSource, context.Area);
            //Preview(data, removed, pass1, "buildings-pass1.png");

            var pass2 = MergeSmallBuildings(pass1, removed);
            //Preview(data, removed, pass2, "buildings-pass2.png");

            var pass3 = RemoveCollidingBuildings(removed, pass2);
            //Preview(data, removed, pass3, "buildings-pass3.png");

            var pass4 = RoadCrop(removed, pass3, roads.Roads);
            //Preview(data, removed, pass4, "buildings-pass4.png");

            pass4 = pass4.Where(b => context.Area.IsInside(b.Box.Center)).ToList();

            DetectEntranceSide(pass4, roads.Roads);

            var pass5 = DetectBuildingCategory(categorizers.Areas, pass4);
            //Preview(data, removed, pass5, "buildings-pass5.png");

            return new BuildingsData(pass5);
        }

        private void DetectEntranceSide(List<BuildingCandidate> buildings, List<Road> roads)
        {
            using var report = progress.CreateStep("EntranceSide", buildings.Count);
            foreach (var building in buildings)
            {
                building.EntranceSide = BoxSideHelper.GetClosest(building.Box, roads.Select(r => r.Path), 25);
                report.ReportOneDone();
            }
        }

        private List<BuildingCandidate> MergeSmallBuildings(List<BuildingCandidate> pass1Builidings, List<TerrainPolygon> removed)
        {
            var pass2 = new List<BuildingCandidate>();

            var size = 6.5f;
            var lsize = 2f;
            var mergeLimit = 100f;

            var small = pass1Builidings.Where(b => (b.Box.Width < size && b.Box.Height < size) || b.Box.Width < lsize || b.Box.Height < lsize).ToList();
            var large = pass1Builidings.Where(b => !((b.Box.Width < size && b.Box.Height < size) || b.Box.Width < lsize || b.Box.Height < lsize) && b.Box.Width < mergeLimit && b.Box.Width < mergeLimit).ToList();
            var heavy = pass1Builidings.Where(b => b.Box.Width >= mergeLimit || b.Box.Height >= mergeLimit).ToList();

            using (var report2 = progress.CreateStep("Heavy", large.Count))
            {
                foreach (var building in heavy)
                {
                    removed.AddRange(small.Concat(large).Where(s => building.Poly.Contains(s.Poly)).SelectMany(b => b.Polygons));
                    small.RemoveAll(s => building.Poly.Contains(s.Poly));
                    large.RemoveAll(s => building.Poly.Contains(s.Poly));
                    report2.ReportOneDone();
                }
            }

            using (var report2 = progress.CreateStep("Small", large.Count))
            {
                foreach (var building in large)
                {
                    if (building.Category == BuildingTypeId.Hut) continue;
                    var wasUpdated = true;
                    while (wasUpdated && building.Box.Width < mergeLimit && building.Box.Width < mergeLimit)
                    {
                        wasUpdated = false;
                        foreach (var other in small.Where(b => b.Poly.Intersects(building.Poly)).ToList())
                        {
                            small.Remove(other);
                            building.Add(other);
                            wasUpdated = true;
                        }
                    }
                    report2.ReportOneDone();
                }
            }

            pass2.AddRange(large);
            pass2.AddRange(small);
            pass2.AddRange(heavy);
            return pass2;
        }

        private List<BuildingCandidate> RemoveCollidingBuildings(List<TerrainPolygon> removed, List<BuildingCandidate> pass2)
        {
            var pass3 = pass2.OrderByDescending(l => l.Box.Width * l.Box.Height).ToList();

            var merged = 0;
            var todo = pass3.ToList();
            var total = todo.Count;
            using var report = progress.CreateStep("Collide", total);
            while (todo.Count > 0)
            {
                var building = todo[0];
                todo.RemoveAt(0);
                bool wasChanged;
                do
                {
                    wasChanged = false;
                    foreach (var other in todo.Where(o => o.Poly.Intersects(building.Poly)).ToList())
                    {
                        var intersection = building.Poly.Intersection(other.Poly);
                        if (intersection.Area > other.Poly.Area * 0.15)
                        {
                            todo.Remove(other);
                            pass3.Remove(other);
                            removed.AddRange(other.Polygons);
                        }
                        else
                        {
                            var mergeSimulation = building.Box.Add(other.Box);
                            if (mergeSimulation.Poly.Area <= (building.Poly.Area + other.Poly.Area - intersection.Area) * 1.05)
                            {
                                building.Add(other);
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


        private List<BuildingCandidate> RoadCrop(List<TerrainPolygon> removed, List<BuildingCandidate> pass3, List<Road> roads)
        {
            using var report = progress.CreateStep("Roads", pass3.Count);
            var pass4 = new List<BuildingCandidate>();
            foreach (var building in pass3)
            {
                var conflicts = roads
                    .Where(r => r.EnveloppeIntersects(building.Box))
                    .Where(r => r.Polygons.Any(p => p.AsPolygon.Intersects(building.Box.Poly)))
                    .ToList();
                if (conflicts.Count > 0)
                {
                    var result = building.Box.Polygon.SubstractAll(conflicts.SelectMany(r => r.Polygons)).ToList();
                    if (result.Count == 1)
                    {
                        var newbox = BoundingBox.ComputeInner(result[0].Shell.Skip(1));
                        if (newbox != null)
                        {
                            if (newbox.Poly.Area < building.Box.Poly.Area / 5)
                            {

                            }

                            building.Box = newbox;
                            pass4.Add(building);
                        }
                        else
                        {
                            removed.AddRange(building.Polygons);
                        }
                    }
                    else
                    {
                        removed.AddRange(building.Polygons);
                    }
                }
                else
                {
                    pass4.Add(building);
                }
                report.ReportOneDone();
            }
            return pass4;
        }

        private List<Building> DetectBuildingCategory(IEnumerable<IBuildingCategoryArea> categorizers, List<BuildingCandidate> pass3)
        {
            var pass4 = new List<Building>(pass3.Count);
            var metas = categorizers
                .Where(b => b.BuildingType != BuildingTypeId.Residential)
                .ToList();

            using var report4 = progress.CreateStep("Category", pass4.Count);
            foreach (var building in pass3)
            {
                if (building.Category == null)
                {
                    var meta = metas.Where(m => m.PolyList.Any(p => p.AsPolygon.Intersects(building.Poly))).FirstOrDefault();
                    if (meta == null)
                    {
                        building.Category = BuildingTypeId.Residential;
                    }
                    else
                    {
                        building.Category = meta.BuildingType;
                    }
                }
                pass4.Add(building.ToBuilding());
                report4.ReportOneDone();
            }
            return pass4;
        }

        private List<BuildingCandidate> DetectBuildingsBoundingRects(IOsmDataSource osm, ITerrainArea area)
        {
            var buildings = osm.All.Where(o => o.Tags != null && o.Tags.ContainsKey("building")).ToList();

            var pass1 = new List<BuildingCandidate>();
            using var report1 = progress.CreateStep("Interpret", buildings.Count);
            foreach (var building in buildings)
            {
                foreach (var geometry in osm.Interpret(building))
                {
                    foreach(var poly in TerrainPolygon.FromGeometry(geometry, area.LatLngToTerrainPoint))
                    {
                        pass1.Add(new BuildingCandidate(poly, OsmBuildingCategorizer.ToBuildingType(building.Tags)));
                    }
                }
                report1.ReportOneDone();
            }
            return pass1;
        }
    }
}
