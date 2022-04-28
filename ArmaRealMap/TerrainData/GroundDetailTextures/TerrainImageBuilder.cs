using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ArmaRealMap.Geometries;
using ArmaRealMap.GroundTextureDetails;
using ArmaRealMap.Osm;
using ArmaRealMap.Roads;
using BIS.PAA;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace ArmaRealMap.TerrainData.GroundDetailTextures
{
    class TerrainImageBuilder
    {
        private static void DrawFakeSat(MapConfig config, MapInfos area, MapData data, List<Polygons> polygonsByCategory, TerrainMaterialLibrary terrainMaterialLibrary)
        {
            var brushes = new Dictionary<TerrainMaterial, ImageBrush>();
            var colors = new Dictionary<TerrainMaterial, Color>();
            foreach (var mat in TerrainMaterial.All.Concat(new[] { TerrainMaterial.Default }))
            {
                var img = GetImage(config, mat, terrainMaterialLibrary);
                brushes.Add(mat, new ImageBrush(img));
                colors.Add(mat, new Color(img[2,2]));
            }

            using (var fakeSat = new Image<Rgba32>(area.ImageryWidth, area.ImageryHeight, Color.Black))
            {
                fakeSat.Mutate(i => i.Fill(brushes[TerrainMaterial.Default]));

                var report = new ProgressReport("Tex-Shapes", polygonsByCategory.Sum(p => p.List.Count));
                fakeSat.Mutate(d =>
                {
                    foreach (var category in polygonsByCategory.OrderByDescending(e => e.Category.GroundTexturePriority))
                    {
                        if (category.Category != OsmShapeCategory.WaterWay)
                        {
                            var brush = brushes[category.Category.TerrainMaterial];

                            var edgeBrush = new PatternBrush(colors[category.Category.TerrainMaterial],
                                Color.Transparent,
                                Generate(category.Category.GroundTexturePriority, 0.5f));

                            Draw(area, d, report, category, brush, edgeBrush);
                        }
                    }
                });
                report.TaskDone();

                var roads = data.Roads.Where(r => r.RoadType <= RoadType.SingleLaneDirtRoad).ToList();

                //chuncking = DrawHelper.SavePngChuncked(fakeSat, config.Target.GetTerrain("sat-fake.png"));

                var realSat = Image.Load<Rgb24>(Path.Combine(config.Target.InputCache, "sat-raw.png"));
                SatMapTiling(realSat, fakeSat, config, area, roads);
                
            }
            /*
            if (!config.GenerateSatTiles)
            {
                BlendRawAndFake(config, 4);
            }*/
        }

        private static IBrush GetBrush(RoadType roadType)
        {
            if (roadType >= RoadType.TwoLanesConcreteRoad)
            {
                return new SolidBrush(Color.ParseHex("5A4936"));
            }
            return new SolidBrush(Color.ParseHex("4D4D4D"));
        }

        private static void SatMapTiling(Image<Rgb24> realSat, Image<Rgba32> fakeSat, MapConfig config, MapInfos area, List<Road> roads)
        {
            Directory.CreateDirectory(LayersHelper.GetLocalPath(config));

            // Going through TerrainBuilder takes ~4 hours, with a lot of manual operations
            // Here, for exactly the same result, it takes 4 minutes, all automated ! (but will eat all your CPU)

            var step = config.TileSize - (config.RealTileOverlap * 2);
            var num = (int)Math.Ceiling((double)realSat.Width / (double)step);
            var report2 = new ProgressReport("Tiling", num * num);
            Parallel.For(0, num, x =>
            {
                using (var tile = new Image<Rgb24>(config.TileSize, config.TileSize, Color.Black))
                {
                    using (var fake = new Image<Rgb24>(config.TileSize, config.TileSize, Color.Transparent))
                    {
                        for (var y = 0; y < num; ++y)
                        {
                            var pos = new Point(-x * step + config.RealTileOverlap, -y * step + config.RealTileOverlap);
                            fake.Mutate(c => c.DrawImage(fakeSat, pos, 1.0f));
                            LayersHelper.FillEdges(realSat, x, num, fake, y, pos);
                            fake.Mutate(d => d.GaussianBlur(10f));

                            tile.Mutate(c => c.DrawImage(realSat, pos, 1.0f));
                            tile.Mutate(p => p.DrawImage(fake, 0.75f));

                            tile.Mutate(d =>
                            {
                                foreach (var road in roads.Where(r => r.SpecialSegment != RoadSpecialSegment.Bridge))
                                {
                                    foreach (var polygon in road.Polygons)
                                    {
                                        DrawHelper.DrawPolygon(d, polygon, GetBrush(road.RoadType), l => l.Select(p => area.TerrainToPixelsPoint(p) + pos));
                                    }
                                }
                            });

                            LayersHelper.FillEdges(realSat, x, num, tile, y, pos);
                            tile.Save(LayersHelper.GetLocalPath(config, $"S_{x:000}_{y:000}_lco.png"));
                            report2.ReportOneDone();
                        }
                    }
                }
            });
            report2.TaskDone();

            Arma3ToolsHelper.ImageToPAA(num, x => LayersHelper.GetLocalPath(config, $"S_{x:000}_*_lco.png"));
        }

        private static Image<Bgra32> GetImage(MapConfig config, TerrainMaterial mat, TerrainMaterialLibrary terrainMaterialLibrary)
        {
            return Image.Load(terrainMaterialLibrary.GetInfo(mat, config.Terrain).FakeSatPngImage, new PngDecoder()).CloneAs<Bgra32>();
        }

        private class Polygons
        {
            internal List<TerrainPolygon> List;
            internal OsmShapeCategory Category;

            public List<TerrainPolygon> MergeAttempt { get; internal set; }
        }

        internal static void GenerateTerrainImages(MapConfig config, MapInfos area, MapData data, List<OsmShape> toRender, TerrainMaterialLibrary terrainMaterialLibrary)
        {
            var polygonsByCategory = GetPolygonsByCategory(toRender);

            DrawFakeSat(config, area, data, polygonsByCategory, terrainMaterialLibrary);

            DrawIdMap(config, area, data, polygonsByCategory, terrainMaterialLibrary);
        }

        private static void DrawIdMap(MapConfig config, MapInfos area, MapData data, List<Polygons> polygonsByCategory, TerrainMaterialLibrary terrainMaterialLibrary)
        {
            using (var img = new Image<Rgba32>(area.ImageryWidth, area.ImageryHeight, TerrainMaterial.Default.GetColor(config.Terrain)))
            {
                var report = new ProgressReport("Tex-Shapes", polygonsByCategory.Sum(p => p.List.Count));
                img.Mutate(d =>
                {
                    d.Fill(GetBrush(data, OsmShapeCategory.Default));
                    foreach (var category in polygonsByCategory.OrderByDescending(e => e.Category.GroundTexturePriority))
                    {
                        if (category.Category != OsmShapeCategory.WaterWay)
                        {
                            Draw(area, d, report, category, GetBrush(data, category.Category), GetEdgeBrush(data, category.Category));
                        }
                    }
                });
                report.TaskDone();

                DrawHelper.SavePngChuncked(img, Path.Combine(config.Target.Terrain, "idmap", "idmap.png"));

                IdMapCompiler.Compile(area, config, img, terrainMaterialLibrary);
            }
        }

        private static IBrush GetBrush(MapData data, OsmShapeCategory category)
        {
            var color = category.TerrainMaterial.GetColor(data.Config.Terrain);
            if (category == OsmShapeCategory.Forest || category == OsmShapeCategory.Rocks)
            {
                return new PatternBrush(color, TerrainMaterial.Default.GetColor(data.Config.Terrain), Generate(category.GroundTexturePriority, 0.0025f, 512));
            }
            if (category == OsmShapeCategory.Scrub)
            {
                return new PatternBrush(color, TerrainMaterial.Forest.GetColor(data.Config.Terrain), Generate(category.GroundTexturePriority, 0.0025f, 512));
            }
            if (category == OsmShapeCategory.Default)
            {
                var img = new Image<Rgba32>(512, 512, color);
                img.Mutate(d =>
                {
                    d.Fill(new PatternBrush(Color.Transparent, TerrainMaterial.Forest.GetColor(data.Config.Terrain), Generate(category.GroundTexturePriority, 0.0025f, 512)));
                    d.Fill(new PatternBrush(Color.Transparent, TerrainMaterial.Rock.GetColor(data.Config.Terrain), Generate(category.GroundTexturePriority, 0.0005f, 512)));
                });
                return new ImageBrush(img);
            }
            return new SolidBrush(color);
        }

        private static IBrush GetEdgeBrush(MapData data, OsmShapeCategory category)
        {
            var color = category.TerrainMaterial.GetColor(data.Config.Terrain);
            return new PatternBrush(color, Color.Transparent, Generate(category.GroundTexturePriority, 0.6f));
        }

        private static void Draw(MapInfos area, IImageProcessingContext d, ProgressReport report, Polygons category, IBrush brush, IBrush edgeBrush)
        {
            var crown = category.Category.TerrainMaterial != TerrainMaterial.WetLand ? 18f : 8f;

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

        private static bool[,] Generate(int seed, float coef, int size = 64)
        {
            var rnd = new Random(seed);
            var matrix = new bool[size, size];
            for(var x = 0;x< size; ++x)
            {
                for (var y = 0; y < size; ++y)
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
