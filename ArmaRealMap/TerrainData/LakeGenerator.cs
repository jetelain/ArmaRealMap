using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using GameRealisticMap.Geometries;
using ArmaRealMap.Libraries;
using ArmaRealMap.Osm;
using ArmaRealMap.Roads;
using ArmaRealMap.TerrainData.Forests;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using GameRealisticMap.ManMade;
using GameRealisticMap.ManMade.Roads;

namespace ArmaRealMap
{
    internal static class LakeGenerator
    {
        internal static void ProcessLakes(MapData data, MapInfos area, List<OsmShape> shapes, GenerateOptions options)
        {
            var initialLakes = shapes.Where(s => s.Category == OsmShapeCategory.Lake).SelectMany(s => s.TerrainPolygons).ToList();

            var cropped = NatureBuilder.CropPolygonsToTerrain(data, initialLakes);

            var objects = new List<TerrainObject>();

            var minimalArea = Math.Pow(5 * data.Config.CellSize, 2); // 5 x 5 nodes minimum
            var minimalOffsetArea = data.Config.CellSize * data.Config.CellSize;

            var embankmentsSegments = data.Roads.Where(r => r.SpecialSegment == WaySpecialSegment.Embankment).ToList();

            ProcessEmbankments(data, embankmentsSegments);

            var embankments = embankmentsSegments.SelectMany(s => s.Path.ToTerrainPolygon(EmbankmentWidth(data, s))).ToList();

            var lakes = cropped.SelectMany(l => l.SubstractAll(embankments)).ToList();

            data.Lakes = new List<Lake>();

            var report = new ProgressReport("Lakes", lakes.Count);
            var cellSize = new Vector2(area.CellSize, area.CellSize);
            foreach (var g in lakes)
            {
                if (g.Area < minimalArea)
                {
                    continue; // too small
                }

                var offsetArea = g.Offset(data.Config.CellSize * -2f).Sum(a => a.Area);
                if (offsetArea < minimalOffsetArea)
                {
                    Trace.WriteLine($"Lake {g}");
                    Trace.WriteLine($"is in fact too small, {offsetArea} offseted -- {Math.Round(g.Area)} area");
                    continue; // too small
                }

                var lake = new Lake();
                var oldBorderElevation = g.Shell.Min(p => data.Elevation.ElevationAt(p));

                lake.BorderElevation = GeometryHelper.PointsOnPath(g.Shell).Min(p => data.Elevation.ElevationAt(p));
                lake.WaterElevation = lake.BorderElevation - 0.1f;
                lake.TerrainPolygon = g;

                var lakeElevation = data.Elevation.PrepareToMutate(g.MinPoint - cellSize, g.MaxPoint + cellSize, lake.BorderElevation - 2.5f, lake.BorderElevation);
                lakeElevation.Image.Mutate(d =>
                {
                    DrawHelper.DrawPolygon(d, g, new SolidBrush(Color.FromRgba(255, 255, 255, 128)), lakeElevation.ToPixels);
                    foreach (var scaled in g.Offset(-10))
                    {
                        DrawHelper.DrawPolygon(d, scaled, new SolidBrush(Color.FromRgba(128, 128, 128, 192)), lakeElevation.ToPixels);
                    }
                    foreach (var scaled in g.Offset(-20))
                    {
                        DrawHelper.DrawPolygon(d, scaled, new SolidBrush(Color.FromRgba(0, 0, 0, 255)), lakeElevation.ToPixels);
                    }
                });
                lakeElevation.Apply();
                GenerateTiles(area, objects, g, lake.WaterElevation, g.MinPoint, g.MaxPoint);
                report.ReportOneDone();
                data.Lakes.Add(lake);
            }
            report.TaskDone();
            if (!options.DoNotGenerateObjects)
            {
                using (var writer = new StreamWriter(new FileStream(Path.Combine(data.Config.Target.Objects, "watertiles.abs.txt"), FileMode.Create, FileAccess.Write)))
                {
                    foreach (var obj in objects)
                    {
                        writer.WriteLine(obj.ToTerrainBuilderCSV());
                    }
                }
            }
            Trace.Flush();
        }

        private static void ProcessEmbankments(MapData data, List<Road> embankmentsSegments)
        {
            var margin = new Vector2(4 * data.Config.CellSize);

            foreach (var em in embankmentsSegments)
            {
                var elevationA = data.Elevation.ElevationAround(em.Path.FirstPoint, em.Width);
                var elevationB = data.Elevation.ElevationAround(em.Path.LastPoint, em.Width);

                var x = data.Elevation.PrepareToMutate(em.Path.MinPoint - margin, em.Path.MaxPoint + margin,
                    Math.Min(elevationA, elevationB), Math.Max(elevationA, elevationB));

                var brush = new LinearGradientBrush(
                    x.ToPixel(em.Path.FirstPoint),
                    x.ToPixel(em.Path.LastPoint),
                    GradientRepetitionMode.None,
                    new ColorStop(0f, x.ElevationToColor(elevationA)),
                    new ColorStop(1f, x.ElevationToColor(elevationB)));

                x.Image.Mutate(p =>
                {
                    float width = EmbankmentWidth(data, em);

                    foreach (var poly in em.Path.ToTerrainPolygon(width))
                    {
                        DrawHelper.DrawPolygon(p, poly, brush, x.ToPixels);
                    }
                });
                x.Apply();

            }
        }

        private static float EmbankmentWidth(MapData data, Road em)
        {
            return em.Width + (2.5f * data.Config.CellSize);
        }

        private static void GenerateTiles(MapInfos area, List<TerrainObject> objects, TerrainPolygon g, float ajustedWaterElevation, TerrainPoint min, TerrainPoint max)
        {
            var w10 = (int)Math.Ceiling((max.X - min.X) / 10);
            var h10 = (int)Math.Ceiling((max.Y - min.Y) / 10);
            var grid10 = new bool[w10, h10];


            using (var img = new Image<Rgba32>(w10, h10, Color.Transparent))
            {
                img.Mutate(d => DrawHelper.DrawPolygon(d, g, new SolidBrush(Color.White), l => l.Select(p => new PointF((p.X - min.X) / 10f, (p.Y - min.Y) / 10f))));
                for (int x = 0; x < w10; ++x)
                {
                    for (int y = 0; y < h10; ++y)
                    {
                        grid10[x, y] = img[x, y].A > 0;
                    }
                }
            }

            /*
            for (int x = 0; x < w10; ++x)
            {
                for (int y = 0; y < h10; ++y)
                {
                    var p1 = new TerrainPoint((x * 10) + min.X, (y * 10) + min.Y);
                    var p2 = new TerrainPoint(p1.X + 10, p1.Y);
                    var p3 = new TerrainPoint(p1.X, p1.Y + 10);
                    var p4 = new TerrainPoint(p1.X + 10, p1.Y + 10);
                    if (g.Contains(p1) || g.Contains(p2) || g.Contains(p3) || g.Contains(p4))
                    {
                        if (area.IsInside(p1))
                        {
                            grid10[x, y] = true;
                        }
                    }
                }
            }*/

            Process(objects, grid10, w10, h10, min, 80, ajustedWaterElevation);
            Process(objects, grid10, w10, h10, min, 40, ajustedWaterElevation);
            Process(objects, grid10, w10, h10, min, 20, ajustedWaterElevation);
            Process(objects, grid10, w10, h10, min, 10, ajustedWaterElevation);
        }

        private static void Process(List<TerrainObject> objects, bool[,] grid10, int w10, int h10, TerrainPoint min, int size, float ajustedWaterElevation)
        {
            var obj = Pond(size);
            var objSize = size / 10;
            var hsize = size / 2;
            for (int x = 0; x < w10; x += objSize)
            {
                for (int y = 0; y < h10; y += objSize)
                {
                    if (x + objSize <= w10 && y + objSize < h10 && Match(grid10, x, y, x + objSize, y + objSize))
                    {
                        Take(grid10, x, y, x + objSize, y + objSize);
                        var ax = (x * 10) + hsize;
                        var ay = (y * 10) + hsize;
                        objects.Add(new TerrainObject(obj, new TerrainPoint(min.X + ax, min.Y + ay), 0.0f, ajustedWaterElevation));
                    }
                }
            }
        }

        private static void Take(bool[,] grid10, int minX, int minY, int maxX, int maxY)
        {
            for (int x = minX; x < maxX; ++x)
            {
                for (int y = minY; y < maxY; ++y)
                {
                    grid10[x, y] = false;
                }
            }
        }

        private static bool Match(bool[,] grid10, int minX, int minY, int maxX, int maxY)
        {
            for (int x = minX; x < maxX; ++x)
            {
                for (int y = minY; y < maxY; ++y)
                {
                    if (!grid10[x, y])
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private static SingleObjetInfos Pond(int v)
        {
            return new SingleObjetInfos()
            {
                Name = "arm_pond_" + v,
                Depth = v,
                Width = v,
                PlacementRadius = v / 2
            };
        }
    }
}
