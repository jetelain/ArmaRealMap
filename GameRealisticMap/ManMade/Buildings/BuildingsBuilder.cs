﻿using System.Collections.Concurrent;
using System.Numerics;
using GameRealisticMap.Algorithms;
using GameRealisticMap.Geometries;
using GameRealisticMap.ManMade.Roads;
using GameRealisticMap.Osm;
using OsmSharp.Geo;
using Pmad.ProgressTracking;

namespace GameRealisticMap.ManMade.Buildings
{
    internal class BuildingsBuilder : IDataBuilder<BuildingsData>
    {
        private readonly IBuildingSizeLibrary library;

        public BuildingsBuilder(IBuildingSizeLibrary library)
        {
            this.library = library;
        }

        public BuildingsData Build(IBuildContext context, IProgressScope scope)
        {
            var roads = context.GetData<RoadsData>();
            var categorizers = context.GetData<CategoryAreaData>();

            var pass1 = DetectBuildingsBoundingRects(context.OsmSource, context.Area, scope);
            //Preview(data, removed, pass1, "buildings-pass1.png");

            var pass2 = MergeSmallBuildings(pass1, context.Area, scope);
            //Preview(data, removed, pass2, "buildings-pass2.png");

            var roadsIndex = new TerrainSpacialIndex<Road>(context.Area);
            roadsIndex.AddRange(roads.Roads);

            AddNodeBuildings(pass2, context.OsmSource, context.Area, roadsIndex, scope) ;

#if PARALLEL
            var pass3 = RemoveCollidingBuildingsParallel(pass2, context.Area, scope);
#else
            var pass3 = RemoveCollidingBuildingsWithSI(pass2, context.Area);
#endif
            //Preview(data, removed, pass3, "buildings-pass3.png");

            var pass4 = RoadCrop(pass3, roadsIndex, scope);
            //Preview(data, removed, pass4, "buildings-pass4.png");

            DetectEntranceSide(pass4, roadsIndex, context.Area, scope);

            var pass5 = DetectBuildingCategory(categorizers.Areas, pass4, scope);
            //Preview(data, removed, pass5, "buildings-pass5.png");

            return new BuildingsData(pass5);
        }

        private void AddNodeBuildings(List<BuildingCandidate> candidates, IOsmDataSource osmSource, ITerrainArea area, TerrainSpacialIndex<Road> roadsIndex, IProgressScope scope)
        {
            var cache = new ConcurrentDictionary<BuildingTypeId, List<Vector2>>();

            foreach(var node in osmSource.Nodes
                .Where(n => n.Tags != null && (n.Tags.ContainsKey("man_made") || n.Tags.GetValue("generator:source") == "wind"))
                .WithProgress(scope, "Nodes"))
            {
                var type = BuildingTypeIdHelper.FromOSM(node.Tags);
                if (type != null)
                {
                    var sizes = cache.GetOrAdd(type.Value, k => library.GetSizes(type.Value).ToList());
                    if (sizes.Count > 0)
                    {
                        var center = area.LatLngToTerrainPoint(node.GetCoordinate());
                        var random = RandomHelper.CreateRandom(center);
                        var closestRoad = roadsIndex.Search(center.WithMargin(100f)).OrderBy(r => r.Path.Distance(center)).FirstOrDefault();
                        float angle;
                        if (closestRoad == null)
                        {
                            angle = RandomHelper.GetAngle(random);
                        }
                        else
                        {
                            var delta = closestRoad.Path.NearestPointBoundary(center).Vector - center.Vector;
                            angle = MathF.Atan2(delta.Y, delta.X);
                        }
                        var size = sizes.GetEquiprobale(random);
                        var box = new BoundingBox(center, size.X, size.Y, angle);
                        candidates.Add(new BuildingCandidate(box, type));
                    }
                }
            }
        }

        private void DetectEntranceSide(IReadOnlyList<BuildingCandidate> buildings, TerrainSpacialIndex<Road> roadsIndex, ITerrainArea area, IProgressScope scope)
        {
            using var report = scope.CreateInteger("EntranceSide", buildings.Count);
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

        private List<BuildingCandidate> MergeSmallBuildings(IReadOnlyList<BuildingCandidate> pass1Builidings, ITerrainArea area, IProgressScope scope)
        {

            var size = 6.5f;
            var lsize = 2f;
            var mergeLimit = 100f;

            var small = new TerrainSpacialIndex<BuildingCandidate>(area);
            small.AddRange(pass1Builidings.Where(b => (b.Box.Width < size && b.Box.Height < size) || b.Box.Width < lsize || b.Box.Height < lsize));
            
            var large = new TerrainSpacialIndex<BuildingCandidate>(area);
            large.AddRange(pass1Builidings.Where(b => !((b.Box.Width < size && b.Box.Height < size) || b.Box.Width < lsize || b.Box.Height < lsize) && b.Box.Width < mergeLimit && b.Box.Width < mergeLimit));

            var heavy = pass1Builidings.Where(b => b.Box.Width >= mergeLimit || b.Box.Height >= mergeLimit).ToList();

            using (var report2 = scope.CreateInteger("Heavy", heavy.Count))
            {
                foreach (var building in heavy)
                {
                    small.RemoveAll(building, s => building.Poly.Contains(s.Poly));
                    large.RemoveAll(building, s => building.Poly.Contains(s.Poly));
                    report2.ReportOneDone();
                }
            }

            using (var report2 = scope.CreateInteger("Small", large.Count))
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

        private List<BuildingCandidate> RemoveCollidingBuildingsWithSI(List<TerrainPolygon> removed, List<BuildingCandidate> pass2, ITerrainArea area, IProgressScope scope)
        {
            var pass3 = pass2.OrderByDescending(l => l.Box.Width * l.Box.Height).ToList();

            var merged = 0;
            var todo = new TerrainSpacialIndex<BuildingCandidate>(area);
            todo.AddRange(pass3);
            //var todo = pass3.ToList();
            var total = todo.Count;
            using var report = scope.CreateInteger("Collide", total);
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

        private List<BuildingCandidate> RemoveCollidingBuildingsParallel(List<BuildingCandidate> pass2, ITerrainArea area, IProgressScope scope)
        {
            using var report = scope.CreateInteger("Collide (Parallel)", pass2.Count);
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


        private List<BuildingCandidate> RoadCrop( IReadOnlyCollection<BuildingCandidate> pass3, TerrainSpacialIndex<Road> roadsIndex, IProgressScope scope)
        {
            using var report = scope.CreateInteger("Roads", pass3.Count);
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

        private List<Building> DetectBuildingCategory(IEnumerable<IBuildingCategoryArea> categorizers, IReadOnlyCollection<BuildingCandidate> pass3, IProgressScope scope)
        {
            var pass4 = new ConcurrentQueue<Building>();
            var metas = categorizers
                .Where(b => b.BuildingType != BuildingTypeId.Residential)
                .ToList();

            using var report4 = scope.CreateInteger("Category", pass3.Count);
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

        private List<BuildingCandidate> DetectBuildingsBoundingRects(IOsmDataSource osm, ITerrainArea area, IProgressScope scope)
        {
            var buildings = osm.All.Where(o => o.Tags != null && o.Tags.ContainsKey("building")).ToList();
            var manMadeAreas = osm.Ways.Where(o => o.Tags != null && o.Tags.ContainsKey("man_made") && !o.Tags.ContainsKey("building")).ToList();

            var areaEnveloppe = new Envelope(TerrainPoint.Empty, new TerrainPoint(area.SizeInMeters, area.SizeInMeters));

            var candidateSurfaces = new List<BuildingCandidate>();

            using var report = scope.CreateInteger("Interpret", buildings.Count + manMadeAreas.Count);
            foreach (var building in buildings)
            {
                foreach (var geometry in osm.Interpret(building))
                {
                    foreach (var poly in TerrainPolygon.FromGeometry(geometry, area.LatLngToTerrainPoint))
                    {
                        if (areaEnveloppe.EnveloppeContains(poly))
                        { 
                            candidateSurfaces.Add(new BuildingCandidate(poly, BuildingTypeIdHelper.FromOSM(building.Tags)));
                        }
                    }
                }
                report.ReportOneDone();
            }

            foreach (var manMade in manMadeAreas)
            {
                var type = BuildingTypeIdHelper.FromOSM(manMade.Tags);
                if (type != null)
                {
                    foreach (var geometry in osm.Interpret(manMade))
                    {
                        foreach (var poly in TerrainPolygon.FromGeometry(geometry, area.LatLngToTerrainPoint))
                        {
                            if (areaEnveloppe.EnveloppeContains(poly))
                            {
                                candidateSurfaces.Add(new BuildingCandidate(poly, type));
                            }
                        }
                    }
                }
                report.ReportOneDone();
            }

            return candidateSurfaces;
        }
    }
}
