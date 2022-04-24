using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using ArmaRealMap.ElevationModel;
using ArmaRealMap.Geometries;
using ArmaRealMap.Libraries;
using ArmaRealMap.Osm;
using ArmaRealMap.Roads;
using ArmaRealMap.TerrainData.ElevationModel;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Processing;

namespace ArmaRealMap
{
    class ElevationGridBuilder
    {
        internal static ElevationGrid LoadOrGenerateElevationGrid(MapData data)
        {
            var elevation = new ElevationGrid(data.MapInfos);
            var cacheFile = Path.Combine(data.Config.Target.InputCache, "elevation-raw.bin");
            if (!File.Exists(cacheFile))
            {
                elevation.LoadFromSRTM(data.GlobalConfig.SRTM);
                elevation.SaveToBin(cacheFile);
            }
            else
            {
                elevation.LoadFromBin(cacheFile);
            }
            return elevation;
        }

        internal static void MakeDetailed(MapData data, List<OsmShape> shapes, ObjectLibraries libs)
        {
            var cacheFile = Path.Combine(data.Config.Target.InternalCache, "elevation.bin");
            if (!File.Exists(cacheFile))
            {
                ProcessForced(data.Config, data.MapInfos, data.Elevation);

                var constraintGrid = new ElevationConstraintGrid(data.MapInfos, data.Elevation);

                ProcessRoads(data, constraintGrid, libs);

                ProcessWaterWays(data, shapes, constraintGrid);

                ProtectLakes(data, constraintGrid);

                constraintGrid.SolveAndApplyOnGrid(data);

                ProcessBridgeObjects(data, data.Elevation, libs);

                data.Elevation.SaveToBin(cacheFile);
            }
            else
            {
                data.Elevation.LoadFromBin(cacheFile);
            }

            data.Elevation.SavePreview(System.IO.Path.Combine(data.Config.Target.Debug, "elevation.png"));
            data.Elevation.SaveToAsc(Path.Combine(data.Config.Target.Terrain, "elevation.asc"));
        }

        private static void ProcessForced(MapConfig config, MapInfos mapInfos, ElevationGrid elevation)
        {
            if (config.ForcedElevation != null)
            {
                foreach (var area in config.ForcedElevation)
                {
                    var polyBase = area.Polygon.Offset(mapInfos.CellSize).FirstOrDefault() ?? area.Polygon;
                    var polyExtened = area.Polygon.Offset(mapInfos.CellSize * 2).FirstOrDefault() ?? area.Polygon;
                    var mutate = elevation.PrepareToMutate(polyExtened.MinPoint, polyExtened.MaxPoint, area.Elevation - 10f, area.Elevation + 10f);
                    mutate.Image.Mutate(m => {
                        DrawHelper.DrawPolygon(m, polyExtened, new SolidBrush(mutate.ElevationToColor(area.Elevation).WithAlpha(0.5f)), mutate.ToPixels);
                        DrawHelper.DrawPolygon(m, polyBase, new SolidBrush(mutate.ElevationToColor(area.Elevation)), mutate.ToPixels);
                    });
                    mutate.Apply();
                }
            }
        }

        private static void ProcessRoads(MapData data, ElevationConstraintGrid constraintGrid, ObjectLibraries libs)
        {
            var roads = data.Roads.Where(r => r.RoadType <= RoadType.TwoLanesConcreteRoad).ToList();
            var report = new ProgressReport("Roads", roads.Count);

            foreach (var road in roads)
            {
                if (road.SpecialSegment == RoadSpecialSegment.Bridge)
                {
                    ProcessRoadBridge(road, constraintGrid, libs);
                }
                else if (road.SpecialSegment == RoadSpecialSegment.Embankment)
                {
                    ProcessRoadEmbankment(constraintGrid, road);
                }
                else
                {
                    ProcessNormalRoad(constraintGrid, road);
                }
                report.ReportOneDone();
            }
            report.TaskDone();
        }
        private static void ProcessBridgeObjects(MapData data, ElevationGrid grid, ObjectLibraries libs)
        {
            var roads = data.Roads.Where(r => r.RoadType <= RoadType.TwoLanesConcreteRoad && r.SpecialSegment == RoadSpecialSegment.Bridge).ToList();
            var report = new ProgressReport("Roads", roads.Count);
            var bridgeObjects = new TerrainObjectLayer(data.MapInfos);
            foreach (var road in roads)
            {
                ProcessRoadBridgeObjects(bridgeObjects, road, grid, libs);
                report.ReportOneDone();
            }
            report.TaskDone();
            bridgeObjects.WriteFile(Path.Combine(data.Config.Target.Terrain, "objects", "bridges.abs.txt"));
        }

        private static void ProcessWaterWays(MapData data, List<OsmShape> shapes, ElevationConstraintGrid constraintGrid)
        {
            var waterWays = shapes.Where(s => s.Category == OsmShapeCategory.WaterWay && s.IsPath && !s.OsmGeo.Tags.ContainsKey("tunnel")).ToList();
            var lakes = data.Lakes.Select(l => l.TerrainPolygon);
            var waterWaysPaths = waterWays.SelectMany(w => w.TerrainPaths).SelectMany(p => p.SubstractAll(lakes)).Where(w => w.Length > 10f).ToList();

            var report = new ProgressReport("Waterways", waterWaysPaths.Count);
            foreach (var waterWay in waterWaysPaths)
            {
                var points = GeometryHelper.PointsOnPath(waterWay.Points, 2).Select(constraintGrid.NodeSoft).ToList();
                foreach (var segment in points.Take(points.Count - 1).Zip(points.Skip(1)))
                {
                    if (segment.First != segment.Second)
                    {
                        segment.Second.MustBeLowerThan(segment.First);
                        segment.First.WantedInitialRelativeElevation = -1f;
                        segment.First.LowerLimitRelativeElevation = -4f;
                    }
                }
                report.ReportOneDone();
            }
            report.TaskDone();

        }

        private static void ProtectLakes(MapData data, ElevationConstraintGrid constraintGrid)
        {
            var report = new ProgressReport("LakeLimit", data.Lakes.Count);
            foreach (var lake in data.Lakes)
            {
                foreach (var extended in lake.TerrainPolygon.Offset(2 * data.Config.CellSize))
                {
                    foreach (var node in constraintGrid.Search(extended.MinPoint.Vector, extended.MaxPoint.Vector).Where(p => extended.Contains(p.Point)))
                    {
                        node.SetNotBelow(lake.BorderElevation);
                        node.IsProtected = true;
                    }
                }
                report.ReportOneDone();
            }
            report.TaskDone();
        }

        private static void ProcessRoadEmbankment(ElevationConstraintGrid constraintGrid, Road road)
        {
            // pin start/stop, imposed smoothing as SRTM precision is too low for this kind of elevation detail
            var start = constraintGrid.NodeHard(road.Path.FirstPoint).PinToInitial();
            var stop = constraintGrid.NodeHard(road.Path.LastPoint).PinToInitial();
            var lengthFromStart = 0f;
            var points = GeometryHelper.PointsOnPath(road.Path.Points, 2).Select(constraintGrid.NodeHard).ToList();
            var totalLength = points.Take(points.Count - 1).Zip(points.Skip(1)).Sum(segment => (segment.Second.Point.Vector - segment.First.Point.Vector).Length());
            var smooth = constraintGrid.CreateSmoothSegment(start, road.Width * 4f);
            foreach (var segment in points.Take(points.Count - 1).Zip(points.Skip(1)))
            {
                if (segment.First != segment.Second)
                {
                    var delta = segment.Second.Point.Vector - segment.First.Point.Vector;
                    constraintGrid.AddFlatSegmentHard(segment.First, delta, road.Width);
                    if (segment.First.Elevation == null)
                    {
                        var elevation = start.Elevation.Value + ((stop.Elevation.Value - start.Elevation.Value) * (lengthFromStart / totalLength));
                        segment.First.SetElevation(elevation);
                    }
                    lengthFromStart += delta.Length();
                    smooth.Add(lengthFromStart, segment.Second);
                }
            }
        }
        private static void ProcessNormalRoad(ElevationConstraintGrid constraintGrid, Road road)
        {
            var lengthFromStart = 0f;
            var points = GeometryHelper.PointsOnPath(road.Path.Points, 2).Select(constraintGrid.NodeHard).ToList();
            var smooth = constraintGrid.CreateSmoothSegment(constraintGrid.NodeHard(road.Path.FirstPoint), road.Width * 4f);
            foreach (var segment in points.Take(points.Count - 1).Zip(points.Skip(1)))
            {
                if (segment.First != segment.Second)
                {
                    var delta = segment.Second.Point.Vector - segment.First.Point.Vector;
                    constraintGrid.AddFlatSegmentHard(segment.First, delta, road.Width);
                    lengthFromStart += delta.Length();
                    smooth.Add(lengthFromStart, segment.Second);
                }
            }
        }

        private static void ProcessRoadBridge(Road road, ElevationConstraintGrid constraintGrid, ObjectLibraries libs)
        {
            var lib = libs.Libraries.FirstOrDefault(l => l.Category == Core.ObjectLibraries.ObjectCategory.BridgePrimaryRoad);
            if (lib == null)
            {
                //ProcessNormalRoad(constraintGrid, road);
                return;
            }
            constraintGrid.NodeHard(road.Path.FirstPoint).PinToInitial();
            constraintGrid.NodeHard(road.Path.LastPoint).PinToInitial();

            //var center = new TerrainPoint(Vector2.Lerp(road.Path.FirstPoint.Vector, road.Path.LastPoint.Vector, 0.5f));
            //constraintGrid.Node(center).WantedInitialRelativeElevation = -1.5f;


            /*var delta = road.Path.FirstPoint.Vector - road.Path.LastPoint.Vector;
            var angle = ((MathF.Atan2(delta.Y, delta.X) * 180 / MathF.PI) + 90f) % 360f;
            var obj1 = lib.Objects[0];
            if (road.Path.Length <= obj1.Depth)
            {
                var pitch = (MathF.Atan2(start.Elevation.Value - stop.Elevation.Value, delta.Length()) * 180 / MathF.PI);

                var center = new TerrainPoint(Vector2.Lerp(road.Path.FirstPoint.Vector, road.Path.LastPoint.Vector, 0.5f));
                var elevation = (start.Elevation.Value + stop.Elevation.Value) / 2f;
                objects.Insert(new TerrainObject(obj1, center, angle, elevation, pitch));
            }
            else
            {
                var pitch = (MathF.Atan2(start.Elevation.Value - stop.Elevation.Value, delta.Length()) * 180 / MathF.PI);
                var stObj = lib.Objects[1];
                var ctObj = lib.Objects[2];
                var endObj = lib.Objects[3];

            }*/
        }


        private static void ProcessRoadBridgeObjects(TerrainObjectLayer objects, Road road, ElevationGrid grid, ObjectLibraries libs)
        {
            var lib = libs.Libraries.FirstOrDefault(l => l.Category == Core.ObjectLibraries.ObjectCategory.BridgePrimaryRoad);
            if (lib == null)
            {
                return;
            }
            var delta = road.Path.FirstPoint.Vector - road.Path.LastPoint.Vector;
            var angle = ((MathF.Atan2(delta.Y, delta.X) * 180 / MathF.PI) + 90f) % 360f;
            var bridgeLength = road.Path.Length;
            var obj1 = lib.Objects[0];
            if (bridgeLength <= obj1.Depth) // One object fits
            {
                var center = new TerrainPoint(Vector2.Lerp(road.Path.FirstPoint.Vector, road.Path.LastPoint.Vector, 0.5f));
                var vector = Vector2.Normalize(road.Path.LastPoint.Vector - road.Path.FirstPoint.Vector) * obj1.Depth / 2f;
                var realStart = center - vector;
                var realEnd = center + vector;
                var elevationStart = grid.ElevationAt(realStart);
                var elevationEnd = grid.ElevationAt(realEnd);
                var pitch = (MathF.Atan2(elevationStart - elevationEnd, (realStart.Vector - realEnd.Vector).Length()) * 180 / MathF.PI);
                var elevation = (elevationStart + elevationEnd) / 2f;
                objects.Insert(new TerrainObject(obj1, center, angle, elevation, pitch));
            }
            else // Need more
            {
                var elevationStart = grid.ElevationAt(road.Path.FirstPoint);
                var elevationEnd = grid.ElevationAt(road.Path.LastPoint);
                var pitch = (MathF.Atan2(elevationStart - elevationEnd, delta.Length()) * 180 / MathF.PI);

                var stObj = lib.Objects[1];
                var ctObj = lib.Objects[2];
                var endObj = lib.Objects[3];

                var stDelta = stObj.Depth / 2 / bridgeLength;
                var endDelta = 1f - (endObj.Depth / 2 / bridgeLength);

                objects.Insert(new TerrainObject(stObj, 
                    new TerrainPoint(Vector2.Lerp(road.Path.FirstPoint.Vector, road.Path.LastPoint.Vector, stDelta)), 
                    angle,
                    elevationStart + ((elevationEnd - elevationStart) * (1f-stDelta)),
                     0f, pitch));
                objects.Insert(new TerrainObject(ctObj, 
                    new TerrainPoint(Vector2.Lerp(road.Path.FirstPoint.Vector, road.Path.LastPoint.Vector, 0.5f)), 
                    angle,
                    (elevationStart + elevationEnd) / 2f, 
                    0f, pitch));
                objects.Insert(new TerrainObject(endObj, 
                    new TerrainPoint(Vector2.Lerp(road.Path.FirstPoint.Vector, road.Path.LastPoint.Vector, endDelta)), 
                    angle,
                    elevationStart + ((elevationEnd - elevationStart) * (1f-endDelta)),
                     0f, pitch));
            }
        }
    }
}
