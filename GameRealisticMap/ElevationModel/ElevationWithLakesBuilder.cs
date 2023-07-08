using System.Diagnostics;
using System.Numerics;
using GameRealisticMap.Geometries;
using GameRealisticMap.IO;
using GameRealisticMap.ManMade;
using GameRealisticMap.ManMade.Buildings;
using GameRealisticMap.ManMade.Railways;
using GameRealisticMap.ManMade.Roads;
using GameRealisticMap.Nature.Lakes;
using GameRealisticMap.Reporting;
using GeoJSON.Text.Geometry;
using MapToolkit;
using MapToolkit.Contours;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace GameRealisticMap.ElevationModel
{
    internal class ElevationWithLakesBuilder : IDataBuilder<ElevationWithLakesData>, IDataSerializer<ElevationWithLakesData>
    {
        private readonly IProgressSystem progress;

        private static readonly Vector2 FlatAreaMargin = new Vector2(FlatAreaOffset25 + 10f);
        private const float FlatAreaOffset25 = 15f;
        private const float FlatAreaOffset50 = 10f;
        private const float FlatAreaOffset = 2.5f;

        private static readonly Vector2 LakeDrawMargin = new Vector2(40);
        private const float LakeDepth100Offset = -15f;
        private const float LakeDepth50Offset = -7.5f;
        private const float LakeDepthOutsideOffset = 7.5f;
        private const float LakeDepthMinElevationOffset = 10;

        private static readonly Vector2 LakeScanMargin = new Vector2(10);

        public ElevationWithLakesBuilder(IProgressSystem progress)
        {
            this.progress = progress;
        }

        public ElevationWithLakesData Build(IBuildContext context)
        {
            var raw = context.GetData<RawElevationData>();
            var lakesData = context.GetData<LakesData>();
            var buildings = context.GetData<BuildingsData>();
            var roads = context.GetData<RoadsData>();
            var railways = context.GetData<RailwaysData>();

            var grid = raw.RawElevation;

            MakeWayEmbankmentsSmooth(grid, roads.Roads.Cast<IWay>().Concat(railways.Railways).Where(r => r.SpecialSegment == WaySpecialSegment.Embankment));

            var lakes = DigLakes(grid, lakesData, context.Area);

            var withElevation = context.OsmSource.Ways.Where(w => w.Tags != null && (w.Tags.ContainsKey("ele") || w.Tags.ContainsKey("ele:wgs84"))).ToList();

            // TODO :
            // - Forced elevation for airstrip,
            // - (Flat area for parking lots ?)
            // - Forced elevation by config

            FlatLargeAreasSmooth(
                GetLargeBuildings(buildings)
                .Concat(GetOutdoorPitch(context)), grid);

            FlatSmallAreasHard(GetOtherBuildings(buildings), grid);

            return new ElevationWithLakesData(grid, lakes);
        }

        private void MakeWayEmbankmentsSmooth(ElevationGrid grid, IEnumerable<IWay> embankmentsSegments)
        {
            var margin = grid.CellSize * 4;

            foreach (var em in embankmentsSegments.ProgressStep(progress, "Embankments"))
            {
                var elevationA = grid.ElevationAround(em.Path.FirstPoint, em.Width);
                var elevationB = grid.ElevationAround(em.Path.LastPoint, em.Width);

                var mutate = grid.PrepareToMutate(em.Path.MinPoint - margin, em.Path.MaxPoint + margin,
                    Math.Min(elevationA, elevationB) - 10, Math.Max(elevationA, elevationB) + 10);

                var brush = new LinearGradientBrush(
                    mutate.ToPixel(em.Path.FirstPoint),
                    mutate.ToPixel(em.Path.LastPoint),
                    GradientRepetitionMode.None,
                    new ColorStop(0f, mutate.ElevationToColor(elevationA)),
                    new ColorStop(1f, mutate.ElevationToColor(elevationB)));

                mutate.Image.Mutate(p =>
                {
                    foreach (var poly in em.ClearPolygons)
                    {
                        PolygonDrawHelper.DrawPolygon(p, poly, brush, mutate.ToPixels);
                    }
                });
                mutate.Apply();

            }
        }

        /// <summary>
        /// Large buildings will better fit on a flat ground
        /// </summary>
        /// <param name="buildings"></param>
        /// <returns></returns>
        private IEnumerable<TerrainPolygon> GetLargeBuildings(BuildingsData buildings)
        {
            var largeBuildingArea = 750f;

            return buildings.Buildings.Where(b => b.Box.Width * b.Box.Height >= largeBuildingArea).Select(b => b.Box.Polygon);
        }

        private IEnumerable<TerrainPolygon> GetOtherBuildings(BuildingsData buildings)
        {
            var largeBuildingArea = 750f;

            return buildings.Buildings.Where(b => b.Box.Width * b.Box.Height < largeBuildingArea).Select(b => b.Box.Polygon);
        }

        /// <summary>
        /// Outdoor pitch are always flat
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private static IEnumerable<TerrainPolygon> GetOutdoorPitch(IBuildContext context)
        {
            return context.OsmSource.Ways.Where(w => w.Tags != null && (w.Tags.GetValue("leisure") == "pitch" && !w.Tags.ContainsKey("indoor")))
                .SelectMany(w => context.OsmSource.Interpret(w))
                .SelectMany(g => TerrainPolygon.FromGeometry(g, context.Area.LatLngToTerrainPoint));
        }

        private void FlatLargeAreasSmooth(IEnumerable<TerrainPolygon> flatAreas, ElevationGrid grid)
        {
            foreach (var flat in flatAreas.ProgressStep(progress, "FlatLarge"))
            {
                var avg = grid.GetAverageElevation(flat);
                var mutate = grid.PrepareToMutate(flat.MinPoint - FlatAreaMargin, flat.MaxPoint + FlatAreaMargin, avg - 10, avg + 10);
                mutate.Image.Mutate(d =>
                {
                    foreach (var scaled in flat.Offset(FlatAreaOffset25))
                    {
                        PolygonDrawHelper.DrawPolygon(d, scaled, new SolidBrush(mutate.ElevationToColor(avg).WithAlpha(0.25f)), mutate.ToPixels);
                    }
                    foreach (var scaled in flat.Offset(FlatAreaOffset50))
                    {
                        PolygonDrawHelper.DrawPolygon(d, scaled, new SolidBrush(mutate.ElevationToColor(avg).WithAlpha(0.5f)), mutate.ToPixels);
                    }
                    foreach (var scaled in flat.Offset(FlatAreaOffset))
                    {
                        PolygonDrawHelper.DrawPolygon(d, scaled, new SolidBrush(mutate.ElevationToColor(avg)), mutate.ToPixels);
                    }
                    d.BoxBlur(1);
                });
                mutate.Apply();
            }
        }

        private void FlatSmallAreasHard(IEnumerable<TerrainPolygon> flatAreas, ElevationGrid grid)
        {
            foreach (var flat in flatAreas.ProgressStep(progress, "FlatSmall"))
            {
                var avg = grid.GetAverageElevation(flat);
                var mutate = grid.PrepareToMutate(flat.MinPoint - FlatAreaMargin, flat.MaxPoint + FlatAreaMargin, avg - 10, avg + 10);
                mutate.Image.Mutate(d =>
                {
                    foreach (var scaled in flat.Offset(FlatAreaOffset))
                    {
                        PolygonDrawHelper.DrawPolygon(d, scaled, new SolidBrush(mutate.ElevationToColor(avg)), mutate.ToPixels);
                    }
                });
                mutate.Apply();
            }
        }

        private List<LakeWithElevation> DigLakes(ElevationGrid elevationGrid, LakesData lakesData, ITerrainArea area)
        {
            var minimalArea = Math.Pow(LakeDepth50Offset * -2.5f, 2);
            var minimalOffsetArea = Math.Pow(LakeDepth50Offset * -1.25f, 2);
            var lakesToProcess = lakesData.Polygons.Where(p => p.Area >= minimalArea && p.Offset(-area.GridCellSize).Sum(a => a.Area) >= minimalOffsetArea).ToList();

            using var report = progress.CreateStep("DigLakes", lakesToProcess.Count * 2);
            var lakes = new List<LakeWithElevation>();

            var lakesWithBorder = lakesToProcess.Select(l => new { Polygon = l, BorderElevation = GeometryHelper.PointsOnPath(l.Shell, elevationGrid.CellSize.X / 10f).Min(p => elevationGrid.ElevationAt(p)) }).ToList();
            var index = 0;
            foreach (var lake in lakesWithBorder.OrderBy(l => l.BorderElevation))
            {
                DigLake(elevationGrid, lakes, lake.Polygon, lake.BorderElevation, index);
                index++;
                report.ReportOneDone();
            }

            foreach (var lake in lakesWithBorder)
            {
                if (!DetectRealLake(elevationGrid, lakes, lake.Polygon, lake.BorderElevation))
                {
                    progress.WriteLine($"Lake with initial area {lake.Polygon.Area} cannot be generated. It might be too small.");
                }
                report.ReportOneDone();
            }
            return lakes;
        }

        private static void DigLake(ElevationGrid elevationGrid, List<LakeWithElevation> lakes, TerrainPolygon lakePolygon, float borderElevation, int lakeIndex)
        {
            ApplyLakeElevation(elevationGrid, lakePolygon, borderElevation, lakes.Count);

            MinimalElevationAroundLake(elevationGrid, lakePolygon, borderElevation, lakes.Count);
        }

        private static bool DetectRealLake(ElevationGrid elevationGrid, List<LakeWithElevation> lakes, TerrainPolygon lakePolygon, float borderElevation)
        {
            var ok = false;
            var min = lakePolygon.MinPoint - LakeScanMargin;
            var max = lakePolygon.MaxPoint + LakeScanMargin;
            float waterElevation = borderElevation - 0.2f;
            var polys = GetElevationSurfaceBelow(elevationGrid, min, max, waterElevation);
            while (polys.Count == 0 && waterElevation > borderElevation - 2.5f)
            {
                waterElevation -= 0.1f;
                polys = GetElevationSurfaceBelow(elevationGrid, min, max, waterElevation);
            }
            foreach (var poly in polys)
            {
                var tpoly = TerrainPolygon.FromGeoJson(poly);
                if (tpoly.Shell.Any(p => lakePolygon.ContainsRaw(p)))
                {
                    lakes.Add(new LakeWithElevation(TerrainPolygon.FromGeoJson(poly), borderElevation, waterElevation));
                    ok = true;
                }
            }
            return ok;
        }

        private static void ApplyLakeElevation(ElevationGrid elevationGrid, TerrainPolygon lakePolygon, float borderElevation, int lakeIndex)
        {
            var min = lakePolygon.MinPoint - LakeDrawMargin;
            var max = lakePolygon.MaxPoint + LakeDrawMargin;

            using var lakeElevation = elevationGrid.PrepareToMutate(min, max, borderElevation - 2.5f, borderElevation);
            lakeElevation.Image.Mutate(d =>
            {
                var blurRadius = (int)Math.Ceiling(LakeDepthOutsideOffset / elevationGrid.CellSize.X);
                foreach (var scaled in lakePolygon.Offset(LakeDepthOutsideOffset))
                {
                    PolygonDrawHelper.DrawPolygon(d, scaled, new SolidBrush(lakeElevation.ElevationToColor(borderElevation)), lakeElevation.ToPixels);
                }
                d.BoxBlur(blurRadius);
                PolygonDrawHelper.DrawPolygon(d, lakePolygon, new SolidBrush(lakeElevation.ElevationToColor(borderElevation)), lakeElevation.ToPixels);

                using var layer = CreateDepthLayer(elevationGrid, lakePolygon, borderElevation, lakeElevation);
                d.DrawImage(layer, 1);
            });
            lakeElevation.Apply();
        }

        private static Image<Rgba64> CreateDepthLayer(ElevationGrid elevationGrid, TerrainPolygon lakePolygon, float borderElevation, ElevationGridArea lakeElevation)
        {
            var layer = lakeElevation.Image.Clone();
            layer.Mutate(d =>
            {
                var blurRadius = (int)Math.Ceiling(-LakeDepth50Offset / elevationGrid.CellSize.X);
                PolygonDrawHelper.DrawPolygon(d, lakePolygon, new SolidBrush(lakeElevation.ElevationToColor(borderElevation)), lakeElevation.ToPixels);
                foreach (var scaled in lakePolygon.Offset(LakeDepth50Offset))
                {
                    PolygonDrawHelper.DrawPolygon(d, scaled, new SolidBrush(lakeElevation.ElevationToColor(borderElevation - 1.25f)), lakeElevation.ToPixels);
                }
                foreach (var scaled in lakePolygon.Offset(LakeDepth100Offset))
                {
                    PolygonDrawHelper.DrawPolygon(d, scaled, new SolidBrush(lakeElevation.ElevationToColor(borderElevation - 2.5f)), lakeElevation.ToPixels);
                }
                d.BoxBlur(blurRadius);
            });
            return layer;
        }

        private static void MinimalElevationAroundLake(ElevationGrid rawElevation, TerrainPolygon lakePolygon, float borderElevation, int lakeIndex)
        {
            var min = lakePolygon.MinPoint - LakeDrawMargin;
            var max = lakePolygon.MaxPoint + LakeDrawMargin;
            using var lakeElevation = rawElevation.PrepareToMutate(min, max, borderElevation - 10f, borderElevation + 10f);
            lakeElevation.Image.Mutate(d =>
            {
                foreach (var scaled in lakePolygon.OuterCrown(LakeDepthMinElevationOffset))
                {
                    PolygonDrawHelper.DrawPolygon(d, scaled, new SolidBrush(lakeElevation.ElevationToColor(borderElevation)), lakeElevation.ToPixels);
                }
            });
            lakeElevation.ApplyAsMinimal();
        }

        private static List<Polygon> GetElevationSurfaceBelow(ElevationGrid rawElevation, TerrainPoint min, TerrainPoint max, float waterElevation)
        {
            var w = new ContourGraph();
            w.Add(rawElevation.ToDataCell().CreateView(
            new Coordinates(min.Y, min.X),
            new Coordinates(max.Y, max.X)),
                new ContourFixedLevel(new List<double>() { waterElevation }));
            return w.ToPolygonsReverse().ToList();
        }

        public async ValueTask<ElevationWithLakesData> Read(IPackageReader package, IContext context)
        {
            var lakes = await package.ReadJson<List<LakeWithElevation>>("LakesElevation.json");

            return new ElevationWithLakesData(context.GetData<ElevationData>().Elevation, lakes);
        }

        public Task Write(IPackageWriter package, ElevationWithLakesData data)
        {
            return package.WriteJson("LakesElevation.json", data.Lakes);
        }
    }
}
