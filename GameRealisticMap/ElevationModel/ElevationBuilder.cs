using GameRealisticMap.ElevationModel.Constrained;
using GameRealisticMap.Geometries;
using GameRealisticMap.IO;
using GameRealisticMap.ManMade;
using GameRealisticMap.ManMade.Railways;
using GameRealisticMap.ManMade.Roads;
using GameRealisticMap.Nature.Watercourses;
using GameRealisticMap.Reporting;
using MapToolkit.DataCells;
using SixLabors.ImageSharp;

namespace GameRealisticMap.ElevationModel
{
    internal class ElevationBuilder : IDataBuilder<ElevationData>, IDataSerializer<ElevationData>
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
            var railways = context.GetData<RailwaysData>();
            var waterData = context.GetData<WatercoursesData>();

            var constraintGrid = new ElevationConstraintGrid(context.Area, raw.Elevation, progress);

            ProcessWays(constraintGrid, roadsData.Roads.Where(r => r.RoadType < RoadTypeId.Trail).ProgressStep(progress, "Roads"));

            ProcessWays(constraintGrid, railways.Railways.ProgressStep(progress, "Railways"));

            ProcessWatercourses(constraintGrid, waterData);

            ProtectLakes(constraintGrid, raw.Lakes, context.Area);

            constraintGrid.SolveAndApplyOnGrid();

            return new ElevationData(constraintGrid.Grid);
        }

        private void ProcessWays(ElevationConstraintGrid constraintGrid, IEnumerable<IWay> ways)
        {
            foreach (var way in ways)
            {
                if (way.SpecialSegment == WaySpecialSegment.Bridge)
                {
                    ProcessRoadBridge(way, constraintGrid);
                }
                else if (way.SpecialSegment == WaySpecialSegment.Embankment)
                {
                    ProcessRoadEmbankment(constraintGrid, way);
                }
                else
                {
                    ProcessNormalRoad(constraintGrid, way);
                }
            }
        }

        private void ProcessWatercourses(ElevationConstraintGrid constraintGrid, WatercoursesData waterData)
        {
            var stepSize = GetStepSize(constraintGrid);
            var waterWaysPaths = waterData.Paths.Where(w => !w.IsTunnel).Where(w => w.Path.Length > 10f).ToList();
            using var report = progress.CreateStep("Waterways", waterWaysPaths.Count);
            foreach (var waterWay in waterWaysPaths)
            {
                var points = GeometryHelper.PointsOnPath(waterWay.Path.Points, stepSize).Select(constraintGrid.NodeSoft).ToList();
                foreach (var segment in points.Take(points.Count - 1).Zip(points.Skip(1)))
                {
                    if (segment.First != segment.Second)
                    {
                        segment.Second.MustBeLowerThan(segment.First);
                        if (waterWay.TypeId == WatercourseTypeId.Stream)
                        {
                            segment.First.WantedInitialRelativeElevation = -0.25f;
                            segment.First.LowerLimitRelativeElevation = -1f;
                        }
                        else
                        {
                            segment.First.WantedInitialRelativeElevation = -1f;
                            segment.First.LowerLimitRelativeElevation = -4f;
                        }
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

        private void ProcessRoadEmbankment(ElevationConstraintGrid constraintGrid, IWay road)
        {
            var stepSize = GetStepSize(constraintGrid);
            // pin start/stop, imposed smoothing as SRTM precision is too low for this kind of elevation detail
            var start = constraintGrid.NodeHard(road.Path.FirstPoint).PinToInitial();
            var stop = constraintGrid.NodeHard(road.Path.LastPoint).PinToInitial();
            var lengthFromStart = 0f;
            var points = GeometryHelper.PointsOnPath(road.Path.Points, stepSize).Select(constraintGrid.NodeHard).ToList();
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
        private void ProcessNormalRoad(ElevationConstraintGrid constraintGrid, IWay road)
        {
            var stepSize = GetStepSize(constraintGrid);
            var lengthFromStart = 0f;
            var points = GeometryHelper.PointsOnPath(road.Path.Points, stepSize).Select(constraintGrid.NodeHard).ToList();
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

        /// <summary>
        /// Compute the best value for call to <see cref="GeometryHelper.PointsOnPath"/>
        /// </summary>
        /// <param name="constraintGrid"></param>
        /// <returns></returns>
        private static float GetStepSize(ElevationConstraintGrid constraintGrid)
        {
            // Original value was 2, but algorithm was designed using Gossi (which have a 10m CellSize)
            // Result was not really good with lower CellSize (Studio tries to keep cellsize between 2 and 8m)
            // 1/4 of cell size let have at worst case 3 points within each cell wich is far enough to keep roads flat
            // In the future, could be changed to 1/3, but need some automated tests to ensure result quality
            return constraintGrid.Grid.CellSize.X / 4;
        }

        private void ProcessRoadBridge(IWay road, ElevationConstraintGrid constraintGrid)
        {
            constraintGrid.NodeHard(road.Path.FirstPoint).PinToInitial();
            constraintGrid.NodeHard(road.Path.LastPoint).PinToInitial();
        }

        public ValueTask<ElevationData> Read(IPackageReader package, IContext context)
        {
            using var stream = package.ReadFile("Elevation.ddc");

            var grid = new ElevationGrid(DemDataCell.Load(stream).To<float>().AsPixelIsPoint());

            return new ValueTask<ElevationData>(new ElevationData(grid));
        }

        public Task Write(IPackageWriter package, ElevationData data)
        {
            using (var stream = package.CreateFile("Elevation.ddc"))
            {
                data.Elevation.ToDataCell().Save(stream);
            }
            return Task.CompletedTask;
        }
    }
}
