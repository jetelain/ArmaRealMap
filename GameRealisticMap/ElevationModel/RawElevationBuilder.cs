using GameRealisticMap.Geometries;
using GameRealisticMap.Reporting;
using MapToolkit;
using MapToolkit.Databases;
using MapToolkit.DataCells;

namespace GameRealisticMap.ElevationModel
{
    internal class RawElevationBuilder : IDataBuilder<RawElevationData>
    {
        private readonly IProgressSystem progress;

        public RawElevationBuilder(IProgressSystem progress)
        {
            this.progress = progress;
        }

        public RawElevationData Build(IBuildContext context)
        {
            var db = new DemDatabase(new DemHttpStorage(new Uri("https://dem.pmad.net/SRTM1/")));

            var done = 0;

            var points = context.Area.GetLatLngBounds().ToList();

            var view = db.CreateView<ushort>(
                new Coordinates(points.Min(p => p.Y) - 0.001, points.Min(p => p.X) - 0.001),
                new Coordinates(points.Max(p => p.Y) + 0.001, points.Max(p => p.X) + 0.001))
                .GetAwaiter()
                .GetResult()
                .ToDataCell();

            var size = context.Area.GridSize;
            var cellSize = context.Area.GridCellSize;

            using var report = progress.CreateStep("RawElevation", size);
            var grid = new ElevationGrid(size, cellSize);
            var elevationGrid = grid.Data;
            Parallel.For(0, size, y =>
            {
                for (int x = 0; x < size; x++)
                {
                    var latLong = context.Area.TerrainPointToLatLng(new TerrainPoint(x * cellSize, y * cellSize));
                    var elevation = GetElevationBilinear(view, latLong.Y, latLong.X);
                    elevationGrid[x, y] = (float)elevation;
                }
                report.Report(Interlocked.Increment(ref done));
            });

            return new RawElevationData(grid);
        }

        private double GetElevationBilinear(DemDataCellBase<ushort> view, double lat, double lon)
        {
            return view.GetLocalElevation(new Coordinates(lat, lon), DefaultInterpolation.Instance);
        }
    }
}
