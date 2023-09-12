using GameRealisticMap.Geometries;
using GameRealisticMap.Reporting;
using GeoAPI.Geometries;
using MapToolkit.Contours;

namespace GameRealisticMap.ElevationModel
{
    internal class ElevationContourBuilder : IDataBuilder<ElevationContourData>
    {
        private readonly IProgressSystem progress;

        public ElevationContourBuilder(IProgressSystem progress)
        {
            this.progress = progress;
        }

        public ElevationContourData Build(IBuildContext context)
        {
            var elevation = context.GetData<ElevationData>().Elevation;

            var contour = new ContourGraph();
            using (var report = progress.CreateStepPercent("Contours"))
            {
                contour.Add(elevation.ToDataCell(), new ContourLevelGenerator(-50, 5), false, report);
            }

            var paths = contour.Lines.Select(l => new TerrainPath(l.Points.Select(p => new TerrainPoint((float)p.Longitude, (float)p.Latitude)).ToList())).ToList();

            return new ElevationContourData(paths);
        }
    }
}
