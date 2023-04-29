using GameRealisticMap.Geometries;
using GameRealisticMap.IO;
using GameRealisticMap.Reporting;
using MapToolkit;
using MapToolkit.Databases;
using MapToolkit.DataCells;

namespace GameRealisticMap.ElevationModel
{
    internal class RawElevationBuilder : IDataBuilder<RawElevationData>, IDataSerializer<RawElevationData>
    {
        private readonly IProgressSystem progress;

        public RawElevationBuilder(IProgressSystem progress)
        {
            this.progress = progress;
        }

        public RawElevationData Build(IBuildContext context)
        {
            var db = new DemDatabase(new DemHttpStorage(new Uri("https://dem.pmad.net/SRTM1/")));
            //var db = new DemDatabase(new DemHttpStorage(new Uri("https://dem.pmad.net/AW3D30/")));

            var done = 0;

            var points = new LatLngBounds(context.Area);

            var view = db.CreateView<ushort>(
                new Coordinates(points.Bottom - 0.001, points.Left - 0.001),
                new Coordinates(points.Top + 0.001, points.Right + 0.001))
                .GetAwaiter()
                .GetResult()
                .ToDataCell();

            var size = context.Area.GridSize;
            var cellSize = context.Area.GridCellSize;

            using var report = progress.CreateStep("SRTM1", size);
            var grid = new ElevationGrid(size, cellSize);
            Parallel.For(0, size, y =>
            {
                for (int x = 0; x < size; x++)
                {
                    var latLong = context.Area.TerrainPointToLatLng(new TerrainPoint(x * cellSize, y * cellSize));
                    var elevation = GetElevationBilinear(view, latLong.Y, latLong.X);
                    grid[x, y] = (float)elevation;
                }
                report.Report(Interlocked.Increment(ref done));
            });

            return new RawElevationData(grid);
        }

        public ValueTask<RawElevationData?> Read(IPackageReader package, IContext context)
        {
            return ValueTask.FromResult<RawElevationData?>(null);
        }

        public Task Write(IPackageWriter package, RawElevationData data)
        {
            return Task.CompletedTask;
        }

        private double GetElevationBilinear(DemDataCellBase<ushort> view, double lat, double lon)
        {
            return view.GetLocalElevation(new Coordinates(lat, lon), DefaultInterpolation.Instance);
        }
    }
}
