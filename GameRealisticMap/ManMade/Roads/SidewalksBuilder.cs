using System;
using System.Numerics;
using GameRealisticMap.Geometries;
using GameRealisticMap.ManMade.Buildings;
using GameRealisticMap.ManMade.Fences;
using GameRealisticMap.ManMade.Railways;
using GameRealisticMap.ManMade.Roads.Libraries;
using GameRealisticMap.Reporting;

namespace GameRealisticMap.ManMade.Roads
{
    internal class SidewalksBuilder : IDataBuilder<SidewalksData>
    {
        private readonly IProgressSystem progress;

        public const float Axis = 1f; // 50cm on each side of the road
        public const float MinimalLength = 4f;
        public const float Epsilon = 0.01f;
        public const float FenceMargin = 1f;

        public SidewalksBuilder(IProgressSystem progress)
        {
            this.progress = progress;
        }

        public SidewalksData Build(IBuildContext context)
        {
            var allRoads = context.GetData<RoadsData>().Roads;

            var areas = context.GetData<CategoryAreaData>().Areas;
            var fences = context.GetData<FencesData>().Fences;

            var mask = allRoads.ProgressStep(progress, "Mask")
                .Where(r => DoNotCrossSideWalk(r.RoadTypeInfos))
                .SelectMany(r => r.Path.ToTerrainPolygon(r.Width + Axis - Epsilon))
                .ToList();

            using (var report = progress.CreateStep("Residential", 1))
            {
                // Add to mask non residential areas
                mask.AddRange(context.Area.TerrainBounds.SubstractAllSplitted(areas.Where(a => a.BuildingType == BuildingTypeId.Residential).SelectMany(a => a.PolyList)));
            }

            mask.AddRange(fences.ProgressStep(progress, "Fences")
                .SelectMany(f => f.Path.ToTerrainPolygonButt(f.Width + FenceMargin)));

            mask.AddRange(context.GetData<RailwaysData>().Railways.SelectMany(r => r.ClearPolygons));

            var polygons = allRoads.ProgressStep(progress, "Roads")
                .Where(r => r.RoadTypeInfos.HasSideWalks && r.SpecialSegment == WaySpecialSegment.Normal)
                .SelectMany(r => r.Path.ToTerrainPolygon(r.Width + Axis))
                .ToList();

            using (var report = progress.CreateStep("MergeRoads", polygons.Count))
            {
                polygons = TerrainPolygon.MergeAllParallel(polygons, report);
            }

            var paths =
                polygons
                .ProgressStep(progress, "SubstractAll")
                .SelectMany(p => p.Holes.Concat(new[] { p.Shell }))
                .Select(p => new TerrainPath(p))
                .SelectMany(p => p.SubstractAll(mask))
                .Where(p => p.Length > MinimalLength)
                .ToList();

            var result = new List<TerrainPath>();

            foreach (var path in paths.ProgressStep(progress, "Orientation"))
            {
                var pathToUse = EnsureOrientation(polygons, path);
                if (pathToUse != null)
                {
                    result.Add(pathToUse);
                }
            }

            return new SidewalksData(result);
        }

        private static TerrainPath? EnsureOrientation(List<TerrainPolygon> polygons, TerrainPath path)
        {
            for (var index = 0; index < path.Points.Count - 1; index++)
            {
                var epsilonPoint = path.Points[index].Vector + (Vector2.Normalize(path.Points[index + 1].Vector - path.Points[index].Vector) * Epsilon);

                var left = new TerrainPoint(Vector2.Transform(epsilonPoint, Matrix3x2.CreateRotation(MathF.PI / 2, path.Points[index].Vector)));
                var right = new TerrainPoint(Vector2.Transform(epsilonPoint, Matrix3x2.CreateRotation(-MathF.PI / 2, path.Points[index].Vector)));

                var leftInRoad = polygons.Any(p => p.Contains(left));
                var rightInRoad = polygons.Any(p => p.Contains(right));

                if (leftInRoad && !rightInRoad)
                {
                    // left is in road, all good
                    return path;
                }
                if (rightInRoad && !leftInRoad)
                {
                    // left should be in a road, revering path will solve that
                    return path.Reversed();
                }
                // none or both in road, will look at next point
            }
            return null;
        }

        private bool DoNotCrossSideWalk(IRoadTypeInfos roadTypeInfos)
        {
            return roadTypeInfos.Id < RoadTypeId.ConcreteFootway;
        }
    }
}
