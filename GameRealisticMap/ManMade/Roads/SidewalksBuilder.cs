using System.Numerics;
using GameRealisticMap.Geometries;
using GameRealisticMap.ManMade.Buildings;
using GameRealisticMap.ManMade.Roads.Libraries;
using GameRealisticMap.Reporting;

namespace GameRealisticMap.ManMade.Roads
{
    internal class SidewalksBuilder : IDataBuilder<SidewalksData>
    {
        private readonly IProgressSystem progress;

        public const float Axis = 0.75f;
        public const float MinimalLength = 4f;
        public const float Epsilon = 0.01f;

        public SidewalksBuilder(IProgressSystem progress)
        {
            this.progress = progress;
        }

        public SidewalksData Build(IBuildContext context)
        {
            var allRoads = context.GetData<RoadsData>().Roads;

            var areas = context.GetData<CategoryAreaData>().Areas;

            var mask = allRoads.ProgressStep(progress, "Mask").Where(r => DoNotCrossSideWalk(r.RoadTypeInfos)).SelectMany(r => r.Path.ToTerrainPolygon(r.Width + Axis - Epsilon)).ToList();

            using (var report = progress.CreateStep("Residential", 1))
            {
                mask.AddRange(context.Area.TerrainBounds.SubstractAllSplitted(areas.Where(a => a.BuildingType == BuildingTypeId.Residential).SelectMany(a => a.PolyList)));
            }

            using (var report = progress.CreateStep("MergeMask", mask.Count))
            {
                mask = TerrainPolygon.MergeAll(mask, report);
            }

            var polygons = allRoads.ProgressStep(progress, "Roads").Where(r => r.RoadTypeInfos.HasSideWalks && r.SpecialSegment == WaySpecialSegment.Normal).SelectMany(r => r.Path.ToTerrainPolygon(r.Width + Axis)).ToList();

            using (var report = progress.CreateStep("MergeRoads", polygons.Count))
            {
                polygons = TerrainPolygon.MergeAll(polygons, report);
            }

            var paths =
                polygons
                .ProgressStep(progress, "SubstractAll")
                .SelectMany(p => p.Holes.Concat(new[] { p.Shell }))
                .Select(p => new TerrainPath(p))
                .SelectMany(p => p.SubstractAllKeepOrientation(mask))
                .Where(p => p.Length > MinimalLength)
                .ToList();

            foreach(var path in paths.ProgressStep(progress, "Orientation"))
            {
                var dx = new TerrainPoint(Vector2.Transform(path.Points[0].Vector + (Vector2.Normalize(path.Points[1].Vector - path.Points[0].Vector) * (Epsilon)),
                    Matrix3x2.CreateRotation(MathF.PI/2, path.Points[0].Vector)));
                if (!polygons.Any(p => p.Contains(dx)))
                {
                    path.Points.Reverse();
                }
            }

            return new SidewalksData(paths);
        }

        private bool DoNotCrossSideWalk(IRoadTypeInfos roadTypeInfos)
        {
            return roadTypeInfos.Id < RoadTypeId.ConcreteFootway;
        }
    }
}
