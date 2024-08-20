using System.Numerics;
using GameRealisticMap.Configuration;
using GameRealisticMap.Geometries;
using GameRealisticMap.IO;
using GameRealisticMap.Nature.Ocean;
using MapToolkit;
using MapToolkit.Databases;
using MapToolkit.DataCells;
using Pmad.ProgressTracking;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace GameRealisticMap.ElevationModel
{
    internal class RawElevationBuilder : IDataBuilder<RawElevationData>, IDataSerializer<RawElevationData>
    {
        private readonly ISourceLocations sources;

        public RawElevationBuilder(ISourceLocations sources)
        {
            this.sources = sources;
        }

        private const float InvSinCos45 = 1.4142135623730949f;
        private const int OutOfBoundsDistance = ElevationOutOfBoundsData.Distance;
        private const int OutOfBoundsStep = 20;

        public RawElevationData Build(IBuildContext context, IProgressScope scope)
        {
            // Prepare data source
            var source = CreateSource(context, scope);

            // Prepare ocean mask
            var oceanMask = CreateOceanMask(context);

            // Create grid
            var size = context.Area.GridSize;
            var cellSize = context.Area.GridCellSize;
            var done = 0;
            var grid = new ElevationGrid(size, cellSize);
            using (var report = scope.CreateInteger("Elevation", size))
            {
                Parallel.For(0, size, y =>
                {
                    for (int x = 0; x < size; x++)
                    {
                        var latLong = context.Area.TerrainPointToLatLng(new TerrainPoint(x * cellSize, y * cellSize));
                        grid[x, y] = source.GetElevation(latLong, oceanMask[x, y].PackedValue);
                    }
                    report.Report(Interlocked.Increment(ref done));
                });
            }

            // Out of bounds
            var outOfBounds = GetOutOfBounds(context, source, grid, scope);

            return new RawElevationData(grid, source.Credits, outOfBounds);
        }

        private ElevationMinMax[] GetOutOfBounds(IBuildContext context, RawElevationSource source, ElevationGrid grid, IProgressScope scope)
        {
            var outOfBounds = new ElevationMinMax[512];
            using (var report = scope.CreateInteger("OutOfBounds", 512))
            {
                var done = 0;
                var boxSize = new Vector2(context.Area.SizeInMeters);
                var center = boxSize / 2;
                Parallel.For(0, 512, a =>
                {
                    var boundary = GetBoundaryPoint(a, center);
                    var vector = Vector2.Normalize(boundary - center) * OutOfBoundsStep;
                    var pos = boundary;
                    var min = (double)grid.ElevationAt(new TerrainPoint(pos));
                    var max = min;
                    for (int i = 0; i < OutOfBoundsDistance; i += OutOfBoundsStep)
                    {
                        pos += vector;
                        var value = source.GetElevationNoMask(context.Area.TerrainPointToLatLng(new TerrainPoint(pos)));
                        if (value < min)
                        {
                            min = value;
                        }
                        if (value > max)
                        {
                            max = value;
                        }
                    }
                    outOfBounds[a] = new ElevationMinMax(min, max);
                    report.Report(Interlocked.Increment(ref done));
                });
            }
            return outOfBounds;
        }

        private static Vector2 GetBoundaryPoint(int angleIndex, Vector2 center)
        {
            var angle = MathF.PI * angleIndex / 256f;
            if (angleIndex < 64 || angleIndex >= 448)
            {
                return center + new Vector2(center.X, center.Y * MathF.Sin(angle) * InvSinCos45);
            }
            if (angleIndex < 192)
            {
                return center + new Vector2(center.X * MathF.Cos(angle) * InvSinCos45, center.Y);
            }
            if (angleIndex < 320)
            {
                return center + new Vector2(-center.X, center.Y * MathF.Sin(angle) * InvSinCos45);
            }
            return center + new Vector2(center.X * MathF.Cos(angle) * InvSinCos45, -center.Y);
        }

        private static Image<L8> CreateOceanMask(IBuildContext context)
        {
            var cellSize = context.Area.GridCellSize;
            var oceanData = context.GetData<OceanData>();
            var oceanMask = new Image<L8>(context.Area.GridSize, context.Area.GridSize, new L8(0));
            if (oceanData.Polygons.Count > 0)
            {
                oceanMask.Mutate(m =>
                {
                    foreach (var o in oceanData.Polygons)
                    {
                        PolygonDrawHelper.DrawPolygon(m, o, new SolidBrush(Color.White), l => l.Select(p => new PointF(p.X / cellSize, p.Y / cellSize)));
                    }
                    m.GaussianBlur(25f / cellSize);

                    foreach (var o in oceanData.Land)
                    {
                        PolygonDrawHelper.DrawPolygon(m, o, new SolidBrush(Color.Black), l => l.Select(p => new PointF(p.X / cellSize, p.Y / cellSize)));
                    }
                    m.GaussianBlur(5f / cellSize);
                });
            }
            return oceanMask;
        }

        private RawElevationSource CreateSource(IBuildContext context, IProgressScope scope)
        {
            using var report = scope.CreateSingle("CreateSource");

            var points = new LatLngBounds(context.Area);
            var start = new Coordinates(points.Bottom - 0.002, points.Left - 0.002);
            var end = new Coordinates(points.Top + 0.002, points.Right + 0.002);

            var pointsView = CreateOutOfBounds(context);
            var startView = new Coordinates(pointsView.Bottom - 0.002, pointsView.Left - 0.002);
            var endView = new Coordinates(pointsView.Top + 0.002, pointsView.Right + 0.002);

            var dbCredits = new List<string>() { "SRTM1", "STRM15+" };

            // Elevation of ground, but really low resolution (460m at equator)
            var fulldb = new DemDatabase(new DemHttpStorage(sources.MapToolkitSRTM15Plus));
            var viewFull = fulldb.CreateView<float>(startView, endView).GetAwaiter().GetResult().ToDataCell();
            var detaildb = fulldb;

            // Elevation of surface, Partial world covergae, but better resolution (30m at equator)
            var srtm = new DemDatabase(new DemHttpStorage(sources.MapToolkitSRTM1));
            IDemDataCell view;
            if (!srtm.HasFullData(start, end).GetAwaiter().GetResult())
            {
                // Alternative Elevation of surface, but requires JAXA credits
                var aw3d30 = new DemDatabase(new DemHttpStorage(sources.MapToolkitAW3D30));
                if (!aw3d30.HasFullData(start, end).GetAwaiter().GetResult())
                {
                    view = viewFull;
                }
                else
                {
                    dbCredits.Add("AW3D30");
                    detaildb = aw3d30;
                    view = aw3d30.CreateView<short>(startView, endView).GetAwaiter().GetResult().ToDataCell(); // TODO: check AW3D30 internal format
                }
            }
            else
            {
                detaildb = srtm;
                view = srtm.CreateView<ushort>(startView, endView).GetAwaiter().GetResult().ToDataCell();
            }

            return new RawElevationSource(dbCredits, view, viewFull);
        }

        private static LatLngBounds CreateOutOfBounds(IBuildContext context)
        {
            var wantedView = TerrainPolygon.FromRectangle(
                new TerrainPoint(-OutOfBoundsDistance, -OutOfBoundsDistance),
                new TerrainPoint(context.Area.SizeInMeters + OutOfBoundsDistance, context.Area.SizeInMeters + OutOfBoundsDistance));
            return new LatLngBounds(context.Area, wantedView.Shell);
        }

        public ValueTask<RawElevationData> Read(IPackageReader package, IContext context)
        {
            return ValueTask.FromResult<RawElevationData>(new RawElevationData(context.GetData<ElevationData>().Elevation, new List<string>(), new ElevationMinMax[0]));
        }

        public Task Write(IPackageWriter package, RawElevationData data)
        {
            return Task.CompletedTask;
        }
    }
}
