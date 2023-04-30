using System.Diagnostics;
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
using SixLabors.ImageSharp.Processing;

namespace GameRealisticMap.ElevationModel
{
    internal class ElevationWithLakesBuilder : IDataBuilder<ElevationWithLakesData>, IDataSerializer<ElevationWithLakesData>
    {
        private readonly IProgressSystem progress;

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

            FlatAreas(
                GetLargeBuildings(buildings)
                .Concat(GetOutdoorPitch(context)), grid);

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

        private void FlatAreas(IEnumerable<TerrainPolygon> flatAreas, ElevationGrid grid)
        {
            foreach (var flat in flatAreas.ProgressStep(progress, "Flat"))
            {
                var avg = grid.GetAverageElevation(flat);
                var mutate = grid.PrepareToMutate(flat.MinPoint - grid.CellSize, flat.MaxPoint + grid.CellSize, avg - 10, avg);
                mutate.Image.Mutate(d =>
                {
                    foreach (var scaled in flat.Offset(grid.CellSize.X))
                    {
                        PolygonDrawHelper.DrawPolygon(d, flat, new SolidBrush(mutate.ElevationToColor(avg).WithAlpha(0.5f)), mutate.ToPixels);
                    }
                    PolygonDrawHelper.DrawPolygon(d, flat, new SolidBrush(mutate.ElevationToColor(avg)), mutate.ToPixels);

                });
                mutate.Apply();
            }
        }


        private List<LakeWithElevation> DigLakes(ElevationGrid rawElevation, LakesData lakesData, ITerrainArea area)
        {
            using var report = progress.CreateStep("DigLakes", lakesData.Polygons.Count);

            var minimalArea = Math.Pow(5 * area.GridCellSize, 2); // 5 x 5 nodes minimum
            var minimalOffsetArea = area.GridCellSize * area.GridCellSize;
            var lakes = new List<LakeWithElevation>();
            var cellSize = rawElevation.CellSize;
            foreach (var g in lakesData.Polygons)
            {
                if (g.Area < minimalArea)
                {
                    continue; // too small
                }
                var offsetArea = g.Offset(area.GridCellSize * -2f).Sum(a => a.Area);
                if (offsetArea < minimalOffsetArea)
                {
                    Trace.WriteLine($"Lake {g}");
                    Trace.WriteLine($"is in fact too small, {offsetArea} offseted -- {Math.Round(g.Area)} area");
                    continue; // too small
                }
                var min = g.MinPoint - (2 * cellSize);
                var max = g.MaxPoint + (2 * cellSize);

                var borderElevation = GeometryHelper.PointsOnPath(g.Shell, area.GridCellSize / 10f).Min(p => rawElevation.ElevationAt(p));
                var lakeElevation = rawElevation.PrepareToMutate(min, max, borderElevation - 2.5f, borderElevation);
                lakeElevation.Image.Mutate(d =>
                {
                    PolygonDrawHelper.DrawPolygon(d, g, new SolidBrush(Color.FromRgba(255, 255, 255, 255)), lakeElevation.ToPixels);
                    foreach (var scaled in g.Offset(cellSize.X * -2f))
                    {
                        PolygonDrawHelper.DrawPolygon(d, scaled, new SolidBrush(Color.FromRgba(128, 128, 128, 255)), lakeElevation.ToPixels);
                    }
                    foreach (var scaled in g.Offset(cellSize.X * -4f))
                    {
                        PolygonDrawHelper.DrawPolygon(d, scaled, new SolidBrush(Color.FromRgba(0, 0, 0, 255)), lakeElevation.ToPixels);
                    }
                    d.BoxBlur(1);
                });
                //lakeElevation.Image.SaveAsPng($"lake{lakes.Count}.png");
                lakeElevation.Apply();

                lakeElevation = rawElevation.PrepareToMutate(min, max, borderElevation - 10f, borderElevation + 10f);
                lakeElevation.Image.Mutate(d =>
                {
                    foreach (var scaled in g.OuterCrown(cellSize.X * 2f))
                    {
                        PolygonDrawHelper.DrawPolygon(d, scaled, new SolidBrush(lakeElevation.ElevationToColor(borderElevation)), lakeElevation.ToPixels);
                    }
                });
                lakeElevation.ApplyAsMinimal();
                report.ReportOneDone();

                float waterElevation = borderElevation - 0.1f;
                var polys = GetElevationSurfaceBelow(rawElevation, min, max, waterElevation);
                while (polys.Count == 0 && waterElevation > borderElevation - 2.5f)
                {
                    waterElevation -= 0.1f;
                    polys = GetElevationSurfaceBelow(rawElevation, min, max, waterElevation);
                }
                foreach (var poly in polys)
                {
                    var tpoly = TerrainPolygon.FromGeoJson(poly);
                    if (polys.Count == 1 || tpoly.Shell.Any(p => g.ContainsRaw(p)))
                    {
                        lakes.Add(new LakeWithElevation(TerrainPolygon.FromGeoJson(poly), borderElevation, waterElevation));
                    }
                }

            }
            return lakes;
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
