using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using ArmaRealMap.ElevationModel;
using ArmaRealMap.Geometries;
using ArmaRealMap.Roads;
using ArmaRealMap.TerrainData.ElevationModel;
using MathNet.Numerics.LinearRegression;

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

        internal static void MakeDetailed(MapData data)
        {
            var roads = data.RoadsRaw.Where(r => r.RoadType <= RoadType.TwoLanesConcreteRoad).ToList();

            var constraintGrid = new ElevationConstraintGrid(data.MapInfos, data.Elevation);

            var report = new ProgressReport("Roads", roads.Count);

            foreach (var road in roads)
            {
                if (road.SpecialSegment == RoadSpecialSegment.Bridge)
                {
                    // pin start/stop
                    // XXX: should imposes lower terrain between
                    constraintGrid.Node(road.Path.FirstPoint).PinToInitial();
                    constraintGrid.Node(road.Path.LastPoint).PinToInitial();
                }
                else if (road.SpecialSegment == RoadSpecialSegment.Embankment)
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
                else
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
                report.ReportOneDone();
            }
            report.TaskDone();

            constraintGrid.SolveAndApplyOnGrid();
            //data.Elevation.SaveToObj(data.Config.Target.GetDebug("elevation-after.obj"));
            data.Elevation.SavePreview(data.Config.Target.GetDebug("elevation-after.png"));
            data.Elevation.SaveToAsc(data.Config.Target.GetTerrain("elevation.asc"));
        }
    }
}
