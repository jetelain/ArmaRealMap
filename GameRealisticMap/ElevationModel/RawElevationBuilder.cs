using System.Linq;
using GameRealisticMap.Geometries;
using GameRealisticMap.IO;
using GameRealisticMap.Nature.Ocean;
using GameRealisticMap.Reporting;
using MapToolkit;
using MapToolkit.Databases;
using MapToolkit.DataCells;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

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
            // Prepare data source
            var source = CreateSource(context);

            // Prepare ocean mask
            var oceanMask = CreateOceanMask(context);

            // Create grid
            var size = context.Area.GridSize;
            var cellSize = context.Area.GridCellSize;
            var done = 0;

            using var report = progress.CreateStep("Elevation", size);
            var grid = new ElevationGrid(size, cellSize);
            Parallel.For(0, size, y =>
            {
                for (int x = 0; x < size; x++)
                {
                    var latLong = context.Area.TerrainPointToLatLng(new TerrainPoint(x * cellSize, y * cellSize));

                    var oceanMakeValue = oceanMask[x, y].PackedValue;
                    if (oceanMakeValue > 128)
                    {
                        var elevation = GetElevationBilinear(source.Ground, latLong.Y, latLong.X);
                        if (elevation >= -1)
                        {
                            elevation = ((oceanMakeValue - 128) / 128f * -0.9f) - 0.1f;
                        }
                        grid[x, y] = (float)elevation;
                    }
                    // TODO: Create an intermediate value ?
                    else
                    {
                        var elevation = GetElevationBilinear(source.SurfaceOnly, latLong.Y, latLong.X);
                        if (double.IsNaN(elevation) || elevation < 1)
                        {
                            elevation = ((128 - oceanMakeValue) / 128f * 0.9f) + 0.1f;
                        }
                        grid[x, y] = (float)elevation;
                    }
                }
                report.Report(Interlocked.Increment(ref done));
            });

            return new RawElevationData(grid, source.Credits);
        }

        private static Image<L8> CreateOceanMask(IBuildContext context)
        {
            var ocean = context.GetData<OceanData>().Polygons;
            var oceanMask = new Image<L8>(context.Area.GridSize, context.Area.GridSize, new L8(0));
            oceanMask.Mutate(m =>
            {
                foreach (var o in ocean)
                {
                    PolygonDrawHelper.DrawPolygon(m, o, new SolidBrush(Color.White), l => l.Select(p => new PointF(p.X / context.Area.GridCellSize, p.Y / context.Area.GridCellSize)));
                }
                m.BoxBlur(1);
            });
            return oceanMask;
        }

        private static RawElevationSource CreateSource(IBuildContext context)
        {
            var points = new LatLngBounds(context.Area);
            var start = new Coordinates(points.Bottom - 0.002, points.Left - 0.002);
            var end = new Coordinates(points.Top + 0.002, points.Right + 0.002);
            var dbCredits = new List<string>() { "SRTM1", "STRM15+" };

            // Elevation of ground, but really low resolution (460m at equator)
            var fulldb = new DemDatabase(new DemHttpStorage(new Uri("https://dem.pmad.net/SRTM15Plus/")));
            var viewFull = fulldb.CreateView<float>(start, end).GetAwaiter().GetResult().ToDataCell();

            // Elevation of surface, Partial world covergae, but better resolution (30m at equator)
            var srtm = new DemDatabase(new DemHttpStorage(new Uri("https://dem.pmad.net/SRTM1/")));
            IDemDataCell view;
            if (!srtm.HasFullData(start, end).GetAwaiter().GetResult())
            {
                // Alternative Elevation of surface, but requires JAXA credits
                var aw3d30 = new DemDatabase(new DemHttpStorage(new Uri("https://dem.pmad.net/AW3D30/")));
                if (!srtm.HasFullData(start, end).GetAwaiter().GetResult())
                {
                    view = viewFull;
                }
                else
                {
                    dbCredits.Add("AW3D30");
                    view = aw3d30.CreateView<ushort>(start, end).GetAwaiter().GetResult().ToDataCell(); // TODO: check AW3D30 internal format
                }
            }
            else
            {
                view = srtm.CreateView<ushort>(start, end).GetAwaiter().GetResult().ToDataCell();
            }

            return new RawElevationSource(dbCredits, view, viewFull);
        }

        public ValueTask<RawElevationData> Read(IPackageReader package, IContext context)
        {
            return ValueTask.FromResult<RawElevationData>(new RawElevationData(context.GetData<ElevationData>().Elevation, new List<string>()));
        }

        public Task Write(IPackageWriter package, RawElevationData data)
        {
            return Task.CompletedTask;
        }

        private double GetElevationBilinear(IDemDataCell view, double lat, double lon)
        {
            return view.GetLocalElevation(new Coordinates(lat, lon), DefaultInterpolation.Instance);
        }
    }
}
