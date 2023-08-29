using GameRealisticMap.Geometries;
using GameRealisticMap.ManMade.Buildings;
using GameRealisticMap.ManMade.Roads.Libraries;
using GameRealisticMap.Reporting;

namespace GameRealisticMap.ManMade.Roads
{
    internal class SidewalksBuilder : IDataBuilder<SidewalksData>
    {
        private readonly IProgressSystem progress;

        public const float Axis = 0.5f;
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

            var polygons = allRoads.ProgressStep(progress, "Roads").Where(r => HasSideWalk(r.RoadTypeInfos)).SelectMany(r => r.Path.ToTerrainPolygon(r.Width + Axis)).ToList();

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
                .ToList();

            return new SidewalksData(paths);
        }

        private bool HasSideWalk(IRoadTypeInfos roadTypeInfos)
        {
            return roadTypeInfos.Id >= RoadTypeId.TwoLanesPrimaryRoad && roadTypeInfos.Id <= RoadTypeId.SingleLaneConcreteRoad;
        }

        private bool DoNotCrossSideWalk(IRoadTypeInfos roadTypeInfos)
        {
            return roadTypeInfos.Id < RoadTypeId.ConcreteFootway;
        }
    }
}
