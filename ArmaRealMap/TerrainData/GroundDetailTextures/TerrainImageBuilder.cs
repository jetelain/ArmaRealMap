using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using ArmaRealMap.Geometries;
using ArmaRealMap.GroundTextureDetails;
using ArmaRealMap.Osm;
using BIS.PAA;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors;

namespace ArmaRealMap.TerrainData.GroundDetailTextures
{
    class TerrainImageBuilder
    {
        private static void DrawFakeSat(Config config, MapInfos area, MapData data, List<Polygons> polygonsByCategory)
        {
            var brushes = new Dictionary<TerrainMaterial, ImageBrush>();
            var colors = new Dictionary<TerrainMaterial, Color>();
            foreach (var mat in TerrainMaterial.All.Concat(new[] { TerrainMaterial.Default }))
            {
                var img = GetImage(config, mat);
                brushes.Add(mat, new ImageBrush(img));
                colors.Add(mat, new Color(img[2,2]));
            }

            int chuncking = 0;
            using (var img = new Image<Rgb24>(area.ImageryWidth, area.ImageryHeight, Color.Black))
            {
                img.Mutate(i => i.Fill(brushes[TerrainMaterial.Default]));

                var report = new ProgressReport("Tex-Shapes", polygonsByCategory.Sum(p => p.List.Count));
                img.Mutate(d =>
                {
                    foreach (var category in polygonsByCategory.OrderByDescending(e => e.Category.GroundTexturePriority))
                    {
                        var brush = brushes[category.Category.TerrainMaterial];

                        var edgeBrush = new PatternBrush(colors[category.Category.TerrainMaterial],
                            Color.Transparent,
                            Generate(category.Category.GroundTexturePriority, 0.5f));

                        Draw(area, d, report, category, brush, edgeBrush);
                    }
                });
                report.TaskDone();
                chuncking = DrawHelper.SavePngChuncked(img, config.Target.GetTerrain("sat-fake.png"));
            }

            BlendRawAndFake(config, 4);
        }

        private static void BlendRawAndFake(Config config, int chuncking)
        {
            var report2 = new ProgressReport("Sat", chuncking * chuncking);
            for (int x = 0; x < chuncking; x++)
            {
                for (int y = 0; y < chuncking; y++)
                {
                    var fake = config.Target.GetTerrain($"sat-fake.{x}_{y}.png");
                    var raw = config.Target.GetTerrain($"sat-raw.{x}_{y}.png");
                    var sat = config.Target.GetTerrain($"sat.{x}_{y}.png");
                    using (var rawImg = Image.Load(raw))
                    {
                        using (var fakeImg = Image.Load(fake))
                        {
                            fakeImg.Mutate(d => d.GaussianBlur(10f));
                            rawImg.Mutate(p => p.DrawImage(fakeImg, 0.3f));
                            rawImg.Save(sat);
                            report2.ReportOneDone();
                        }
                    }
                }
            }
            report2.TaskDone();
        }

        private static Image<Bgra32> GetImage(Config config, TerrainMaterial mat)
        {
            var texture = Path.Combine("P:", mat.Co(config.Terrain));
            using (var paaStream = File.OpenRead(texture))
            {
                var paa = new PAA(paaStream);
                var map = paa.Mipmaps.First(m => m.Width == 8);
                var pixels = PAA.GetARGB32PixelData(paa, paaStream, map);
                return Image.LoadPixelData<Bgra32>(pixels, map.Width, map.Height);
            }
        }



        private class Polygons
        {
            internal List<TerrainPolygon> List;
            internal OsmShapeCategory Category;

            public List<TerrainPolygon> MergeAttempt { get; internal set; }
        }

        internal static void GenerateTerrainImages(Config config, MapInfos area, MapData data, List<OsmShape> toRender)
        {
            var polygonsByCategory = GetPolygonsByCategory(toRender);

            DrawFakeSat(config, area, data, polygonsByCategory);

            DrawIdMap(config, area, data, polygonsByCategory);
        }

        private static void DrawIdMap(Config config, MapInfos area, MapData data, List<Polygons> polygonsByCategory)
        {
            using (var img = new Image<Rgb24>(area.ImageryWidth, area.ImageryHeight, TerrainMaterial.Default.GetColor(config.Terrain)))
            {
                var report = new ProgressReport("Tex-Shapes", polygonsByCategory.Sum(p => p.List.Count));
                img.Mutate(d =>
                {
                    foreach (var category in polygonsByCategory.OrderByDescending(e => e.Category.GroundTexturePriority))
                    {
                        var color = category.Category.TerrainMaterial.GetColor(data.Config.Terrain);

                        var brush = new SolidBrush(color);

                        var edgeBrush = new PatternBrush(color,
                            Color.Transparent,
                            Generate(category.Category.GroundTexturePriority, 0.5f));

                        Draw(area, d, report, category, brush, edgeBrush);
                    }
                });
                report.TaskDone();


                // Roads has no effect on terrain material, it will be cover by a specific texture
                /*
                report = new ProgressReport("Tex-Roads", data.Roads.Count);
                img.Mutate(d =>
                {
                    var brush = new SolidBrush(OsmShapeCategory.Road.TerrainMaterial.GetColor(data.Config.Terrain));
                    foreach (var road in data.Roads)
                    {
                        DrawHelper.DrawPath(d, road.Path, (float)(road.Width / area.ImageryResolution), brush, data.MapInfos, false);
                        report.ReportOneDone();
                    }
                });
                report.TaskDone();
                */

                /*
                report = new ProgressReport("Buildings", data.Buildings.Count);
                foreach (var item in data.WantedBuildings)
                {
                    img.Mutate(x => x.FillPolygon(OsmShapeCategory.Building.TerrainMaterial.GetColor(data.Config.Terrain), data.MapInfos.TerrainToPixelsPoints(item.Box.Points).ToArray()));
                    report.ReportOneDone();
                }
                report.TaskDone();
                */

                DrawHelper.SavePngChuncked(img, config.Target.GetTerrain("id.png"));
            }
        }

        private static void Draw(MapInfos area, IImageProcessingContext d, ProgressReport report, Polygons category, IBrush brush, IBrush edgeBrush)
        {
            var crown = category.Category.TerrainMaterial != TerrainMaterial.WetLand ? 12f : 4f;

            foreach (var polygon in category.List)
            {
                DrawHelper.DrawPolygon(d, polygon, brush, area, false);

                foreach (var x in polygon.Crown(crown))
                {
                    DrawHelper.DrawPolygon(d, x, edgeBrush, area, false);
                }
                report.ReportOneDone();
            }
        }

        private static bool[,] Generate(int seed, float coef)
        {
            var rnd = new Random(seed);
            var matrix = new bool[64,64];
            for(var x = 0;x<64;++x)
            {
                for (var y = 0; y < 64; ++y)
                {
                    matrix[x, y] = rnd.NextDouble() >= coef;
                }
            }
            return matrix;
        }

        private static List<Polygons> GetPolygonsByCategory(List<OsmShape> toRender)
        {
            var shapes = toRender.Where(r => r.Category.GroundTexturePriority != 0).ToList();
            var polygonsByCategory = new List<Polygons>();
            var report = new ProgressReport("PolygonsByCategory", shapes.Count);
            foreach (var group in shapes.GroupBy(s => s.Category))
            {
                var shapesOfGroup = group.ToList();
                polygonsByCategory.Add(new Polygons()
                {
                    // MergeAll does not works well, needs investigation...
                    List = /*TerrainPolygon.MergeAll(*/shapesOfGroup.SelectMany(g => g.TerrainPolygons).ToList()/*)*/,
                    Category = group.Key
                });
                report.ReportItemsDone(shapesOfGroup.Count);
            }
            report.TaskDone();
            return polygonsByCategory;
        }
    }
}
