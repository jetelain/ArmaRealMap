﻿using GameRealisticMap.ElevationModel.Constrained;
using GameRealisticMap.Geometries;
using GameRealisticMap.Nature.Watercourses;
using GameRealisticMap.Reporting;
using GameRealisticMap.Roads;
using GeoJSON.Text.Geometry;
using MapToolkit.Contours;
using SixLabors.ImageSharp;

namespace GameRealisticMap.ElevationModel
{
    internal class ElevationBuilder : IDataBuilder<ElevationData>
    {
        private readonly IProgressSystem progress;

        public ElevationBuilder(IProgressSystem progress)
        {
            this.progress = progress;
        }

        public ElevationData Build(IBuildContext context)
        {
            var raw = context.GetData<ElevationWithLakesData>();
            var roadsData = context.GetData<RoadsData>();
            var waterData = context.GetData<WatercoursesData>();

            var constraintGrid = new ElevationConstraintGrid(context.Area, raw.Elevation, progress);

            ProcessRoads(constraintGrid, roadsData);

            ProcessWatercourses(constraintGrid, waterData);

            ProtectLakes(constraintGrid, raw.Lakes, context.Area);

            constraintGrid.SolveAndApplyOnGrid();

            var contour = new ContourGraph();
            using (var report = progress.CreateStepPercent("Contours"))
            {
                contour.Add(constraintGrid.Grid.ToDataCell(), new ContourLevelGenerator(1, 1), false, report);
            }

            return new ElevationData(constraintGrid.Grid, contour.Lines.Select(l => new LineString(l.Points)));
        }

        private void ProcessRoads(ElevationConstraintGrid constraintGrid, RoadsData roadsData)
        {
            var roads = roadsData.Roads.Where(r => r.RoadType < RoadTypeId.Trail).ToList();

            using var report = progress.CreateStep("Roads", roads.Count);

            foreach (var road in roads)
            {
                if (road.SpecialSegment == RoadSpecialSegment.Bridge)
                {
                    ProcessRoadBridge(road, constraintGrid);
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
        }

        private void ProcessWatercourses(ElevationConstraintGrid constraintGrid, WatercoursesData waterData)
        {
            var waterWaysPaths = waterData.Paths.Where(w => !w.IsTunnel).Select(w => w.Path).Where(w => w.Length > 10f).ToList();
            using var report = progress.CreateStep("Waterways", waterWaysPaths.Count);
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
        }

        private void ProtectLakes(ElevationConstraintGrid constraintGrid, List<LakeWithElevation> lakes, ITerrainArea area)
        {
            using var report = progress.CreateStep("LakeLimit", lakes.Count);
            foreach (var lake in lakes)
            {
                foreach (var extended in lake.TerrainPolygon.Offset(2 * area.GridCellSize))
                {
                    foreach (var node in constraintGrid.Search(extended.MinPoint.Vector, extended.MaxPoint.Vector).Where(p => extended.Contains(p.Point)))
                    {
                        node.SetNotBelow(lake.BorderElevation);
                        node.IsProtected = true;
                    }
                }
                report.ReportOneDone();
            }
        }

        private void ProcessRoadEmbankment(ElevationConstraintGrid constraintGrid, Road road)
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
        private void ProcessNormalRoad(ElevationConstraintGrid constraintGrid, Road road)
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

        private void ProcessRoadBridge(Road road, ElevationConstraintGrid constraintGrid)
        {
            constraintGrid.NodeHard(road.Path.FirstPoint).PinToInitial();
            constraintGrid.NodeHard(road.Path.LastPoint).PinToInitial();
        }
    }
}