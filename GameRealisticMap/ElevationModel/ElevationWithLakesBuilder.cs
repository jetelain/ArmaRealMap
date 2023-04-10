using System.Diagnostics;
using GameRealisticMap.Buildings;
using GameRealisticMap.Geometries;
using GameRealisticMap.Nature.Lakes;
using GameRealisticMap.Nature.Watercourses;
using GameRealisticMap.Reporting;
using GameRealisticMap.Roads;
using GeoJSON.Text.Geometry;
using MapToolkit;
using MapToolkit.Contours;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Processing;

namespace GameRealisticMap.ElevationModel
{
    internal class ElevationWithLakesBuilder : IDataBuilder<ElevationWithLakesData>
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

            var lakes = DigLakes(raw.RawElevation, lakesData, context.Area);

            var withElevation = context.OsmSource.Ways.Where(w => w.Tags != null && (w.Tags.ContainsKey("ele") || w.Tags.ContainsKey("ele:wgs84"))).ToList();

            // TODO :
            // - Forced elevation for airstrip,
            // - Flat area for sports area, (parking lots ?)
            // - Forced elevation by config

            var grid = raw.RawElevation;

            FlatAreas(buildings, grid);

            return new ElevationWithLakesData(grid, lakes);
        }

        private void FlatAreas(BuildingsData buildings, ElevationGrid grid)
        {
            var largeBuildingArea = 750f;

            var flatAreas = buildings.Buildings
                .Where(b => b.Box.Width * b.Box.Height >= largeBuildingArea).Select(b => b.Box.Polygon)
                .ProgressStep(progress, "Flat");

            foreach (var flat in flatAreas)
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
                    PolygonDrawHelper.DrawPolygon(d, g, new SolidBrush(Color.FromRgba(255, 255, 255, 128)), lakeElevation.ToPixels);
                    foreach (var scaled in g.Offset(cellSize.X * -2f))
                    {
                        PolygonDrawHelper.DrawPolygon(d, scaled, new SolidBrush(Color.FromRgba(128, 128, 128, 192)), lakeElevation.ToPixels);
                    }
                    foreach (var scaled in g.Offset(cellSize.X * -4f))
                    {
                        PolygonDrawHelper.DrawPolygon(d, scaled, new SolidBrush(Color.FromRgba(0, 0, 0, 255)), lakeElevation.ToPixels);
                    }
                });
                lakeElevation.Apply();

                lakeElevation = rawElevation.PrepareToMutate(min, max, borderElevation - 10f, borderElevation + 10f);
                lakeElevation.Image.Mutate(d =>
                {
                    foreach (var scaled in g.OuterCrown(cellSize.X * 4f))
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
                    lakes.Add(new LakeWithElevation(TerrainPolygon.FromGeoJson(poly), borderElevation, waterElevation));
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

    }
}
