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
using MathNet.Numerics.LinearRegression;
using NetTopologySuite.Geometries;
using OsmSharp;
using OsmSharp.Streams;

namespace ArmaRealMap
{
    class ElevationGridBuilder
    {
        internal static ElevationGrid LoadOrGenerateElevationGrid(MapData data)
        {
            var elevation = new ElevationGrid(data.MapInfos);
            var cacheFile = data.Config.Target.GetCache("elevation-raw.bin");
            if (!File.Exists(cacheFile))
            {
                elevation.LoadFromSRTM(data.Config.SRTM);
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
            var constraintGrid = new ElevationConstraintGrid(data.MapInfos, data.Elevation);

            ProcessRoads(data, constraintGrid, libs);

            ProcessWaterWays(data, shapes, constraintGrid);

            constraintGrid.SolveAndApplyOnGrid(data);
            //data.Elevation.SaveToObj(data.Config.Target.GetDebug("elevation-after.obj"));
            data.Elevation.SavePreview(data.Config.Target.GetDebug("elevation-after.png"));
            data.Elevation.SaveToAsc(data.Config.Target.GetTerrain("elevation.asc"));
        }

        private static void ProcessRoads(MapData data, ElevationConstraintGrid constraintGrid, ObjectLibraries libs)
        {
            var roads = data.Roads.Where(r => r.RoadType <= RoadType.TwoLanesConcreteRoad).ToList();
            var report = new ProgressReport("Roads", roads.Count);
            var bridgeObjects = new TerrainObjectLayer(data.MapInfos);

            foreach (var road in roads)
            {
                if (road.SpecialSegment == RoadSpecialSegment.Bridge)
                {
                    ProcessRoadBridge(bridgeObjects, road, constraintGrid, libs);
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

            bridgeObjects.WriteFile(data.Config.Target.GetTerrain("bridges.txt"));
        }

        private static void ProcessWaterWays(MapData data, List<OsmShape> shapes, ElevationConstraintGrid constraintGrid)
        {
            var waterWays = shapes.Where(s => s.Category == OsmShapeCategory.WaterWay && s.IsPath && !s.OsmGeo.Tags.ContainsKey("tunnel")).ToList();
            var lakes = data.Lakes.Select(l => l.TerrainPolygon);
            var waterWaysPaths = waterWays.SelectMany(w => w.TerrainPaths).SelectMany(p => p.SubstractAll(lakes)).Where(w => w.Length > 10f).ToList();

            var report = new ProgressReport("Waterways", waterWays.Count);
            foreach (var waterWay in waterWaysPaths)
            {
                var points = GeometryHelper.PointsOnPath(waterWay.Points, data.MapInfos.CellSize / 2f).Select(constraintGrid.Node).ToList();
                foreach (var segment in points.Take(points.Count - 1).Zip(points.Skip(1)))
                {
                    if (segment.First != segment.Second)
                    {
                        var delta = segment.Second.Point.Vector - segment.First.Point.Vector;
                        segment.Second.MustBeLowerThan(segment.First);
                        segment.First.WantedInitialShift = -1f;
                        foreach (var side in constraintGrid.GetReference(segment.First, delta, 15f))
                        {
                            segment.First.MustBeLowerThan(side, 1.5f, true);
                        }
                    }
                }
                report.ReportOneDone();
            }
            report.TaskDone();
        }

        private static void ProcessRoadEmbankment(ElevationConstraintGrid constraintGrid, Road road)
        {
            // pin start/stop, imposed smoothing as SRTM precision is too low for this kind of elevation detail
            var start = constraintGrid.Node(road.Path.FirstPoint).PinToInitial();
            var stop = constraintGrid.Node(road.Path.LastPoint).PinToInitial();
            var lengthFromStart = 0f;
            var points = GeometryHelper.PointsOnPath(road.Path.Points, 2).Select(constraintGrid.Node).ToList();
            var totalLength = points.Take(points.Count - 1).Zip(points.Skip(1)).Sum(segment => (segment.Second.Point.Vector - segment.First.Point.Vector).Length());
            var smooth = constraintGrid.CreateSmoothSegment(start, road.Width * 4f);
            foreach (var segment in points.Take(points.Count - 1).Zip(points.Skip(1)))
            {
                if (segment.First != segment.Second)
                {
                    var delta = segment.Second.Point.Vector - segment.First.Point.Vector;
                    constraintGrid.AddFlatSegment(segment.First, delta, road.Width);
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
            var points = GeometryHelper.PointsOnPath(road.Path.Points, 2).Select(constraintGrid.Node).ToList();
            var smooth = constraintGrid.CreateSmoothSegment(constraintGrid.Node(road.Path.FirstPoint), road.Width * 4f);
            foreach (var segment in points.Take(points.Count - 1).Zip(points.Skip(1)))
            {
                if (segment.First != segment.Second)
                {
                    var delta = segment.Second.Point.Vector - segment.First.Point.Vector;
                    constraintGrid.AddFlatSegment(segment.First, delta, road.Width);
                    lengthFromStart += delta.Length();
                    smooth.Add(lengthFromStart, segment.Second);
                }
            }
        }

        private static void ProcessRoadBridge(TerrainObjectLayer objects, Road road, ElevationConstraintGrid constraintGrid, ObjectLibraries libs)
        {
            var lib = libs.Libraries.FirstOrDefault(l => l.Category == Core.ObjectLibraries.ObjectCategory.BridgePrimaryRoad);
            if (lib == null)
            {
                //ProcessNormalRoad(constraintGrid, road);
                return;
            }
            var start = constraintGrid.Node(road.Path.FirstPoint).PinToInitial();
            var stop = constraintGrid.Node(road.Path.LastPoint).PinToInitial();
            var delta = road.Path.FirstPoint.Vector - road.Path.LastPoint.Vector;
            var angle = ((MathF.Atan2(delta.Y, delta.X) * 180 / MathF.PI) + 90f) % 360f;
            var pitch = (MathF.Atan2(start.Elevation.Value - stop.Elevation.Value, delta.Length()) * 180 / MathF.PI);
            var obj1 = lib.Objects[0];
            if (road.Path.Length <= obj1.Depth)
            {
                var center = new TerrainPoint(Vector2.Lerp(road.Path.FirstPoint.Vector, road.Path.LastPoint.Vector, 0.5f));
                var elevation = (start.Elevation.Value + stop.Elevation.Value) / 2f;
                objects.Insert(new TerrainObject(obj1, center, angle, elevation, pitch));
            }
            else
            {
                var stObj = lib.Objects[1];
                var ctObj = lib.Objects[2];
                var endObj = lib.Objects[3];

            }
        }
    }
}
