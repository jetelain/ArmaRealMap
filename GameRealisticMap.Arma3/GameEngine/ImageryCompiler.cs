using System.Collections.Concurrent;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.IO;
using HugeImages;
using HugeImages.Processing;
using Pmad.ProgressTracking;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace GameRealisticMap.Arma3.GameEngine
{
    public class ImageryCompiler
    {
        private readonly TerrainMaterialLibrary materialLibrary;
        private readonly IProgressScope progress;
        private readonly IGameFileSystemWriter gameFileSystemWriter;

        private class TextureStats
        {
            public TextureStats(TerrainMaterial terrainMaterial)
            {
                Material = terrainMaterial;
            }

            public TerrainMaterial Material { get; }

            public int Count { get; set; }

            public Rgba32 Target { get; set; }
        }

        private static Rgba32[] RuleColors = new[] 
        {
            new Rgba32(0, 0, 0, 255), // Stage3
            new Rgba32(255, 0, 0, 255), // Stage5
            new Rgba32(0, 255, 0, 255), // Stage7
            new Rgba32(0, 0, 255, 255), // Stage9
            new Rgba32(0, 0, 255, 128), // Stage11
            new Rgba32(0, 0, 255, 0) // Stage13
        };

        public ImageryCompiler(TerrainMaterialLibrary materialLibrary, IProgressScope progress, IGameFileSystemWriter gameFileSystemWriter)
        {
            this.materialLibrary = materialLibrary;
            this.progress = progress;
            this.gameFileSystemWriter = gameFileSystemWriter;
        }

        public ImageryTiler Compile(IArma3MapConfig config, IImagerySource source)
        {
            gameFileSystemWriter.CreateDirectory($"{config.PboPrefix}\\data\\layers");

            TerrainMaterialHelper.UnpackEmbeddedFiles(materialLibrary, progress, gameFileSystemWriter, config);

            var tiler = new ImageryTiler(config);

            using (var idMap = source.CreateIdMap())
            {
                // idMap.SaveAsPng("idmap.png");
                GenerateIdMapTilesAndRvMat(config, idMap, tiler);
            }

            CreateConfigCppImages(gameFileSystemWriter, config, source);

            using (var satMap = source.CreateSatMap())
            {
                // satMap.SaveAsPng("satmap.png");
                GenerateSatMapTiles(config, satMap, tiler);
            }

            return tiler;
        }

        public static void CreateConfigCppImages(IGameFileSystemWriter gameFileSystemWriter, IArma3MapConfig config, IImagerySource source)
        {
            var picturemapFile = $"{config.PboPrefix}\\data\\picturemap_ca.png";
            //if (!gameFileSystemWriter.FileExists(picturemapFile))
            {
                using (var satMapOut = source.CreatePictureMap())
                {
                    gameFileSystemWriter.WritePngImage(picturemapFile, satMapOut);
                }
            }

            var satoutFile = $"{config.PboPrefix}\\data\\satout_ca.png";
            //if (!gameFileSystemWriter.FileExists(satoutFile))
            {
                using (var satMapOut = source.CreateSatOut())
                {
                    gameFileSystemWriter.WritePngImage(satoutFile, satMapOut);
                }
            }
        }

        internal void GenerateSatMapTiles(IArma3MapConfig config, HugeImage<Rgba32> satMap, ImageryTiler tiler)
        {
            using var report = progress.CreateInteger("SatMapTiling", tiler.Segments.Length);
            Parallel.For(0, tiler.Segments.GetLength(0), x =>
            {
                using (var tile = new Image<Rgb24>(tiler.TileSize, tiler.TileSize, Color.Black.ToPixel<Rgb24>()))
                {
                    for (var y = 0; y < tiler.Segments.GetLength(1); ++y)
                    {
                        var seg = tiler.Segments[x, y];
                        var pos = seg.ImageTopLeft;
                        tile.Mutate(c => c.DrawHugeImage(satMap, pos, 1.0f));
                        ImageryTileHelper.FillEdges(tiler.FullImageSize, x, tiler.Segments.GetLength(0), tile, y, -pos);
                        gameFileSystemWriter.WritePngImage($"{config.PboPrefix}\\data\\layers\\S_{x:000}_{y:000}_lco.png", tile);
                    }
                }
            });
        }

        internal HashSet<TerrainMaterial> GenerateIdMapTilesAndRvMat(IArma3MapConfig config, HugeImage<Rgba32> idmap, ImageryTiler tiler)
        {
            using var report = progress.CreateInteger("IdMapTiling", tiler.Segments.Length);
            var textureScale = GetTextureScale(config, materialLibrary);
            var allUsed = new ConcurrentQueue<HashSet<TerrainMaterial>>();
            Parallel.For(0, tiler.Segments.GetLength(0), x =>
            {
                var usedMaterials = new HashSet<TerrainMaterial>();
                using (var sourceTile = new Image<Rgb24>(tiler.IdMapTileSize, tiler.IdMapTileSize, Color.Black.ToPixel<Rgb24>()))
                {
                    using (var targetTile = new Image<Rgba32>(tiler.IdMapTileSize, tiler.IdMapTileSize, Color.Black.ToPixel<Rgba32>()))
                    {
                        for (var y = 0; y < tiler.Segments.GetLength(1); ++y)
                        {
                            var seg = tiler.Segments[x, y];
                            var pos = seg.ImageTopLeft * tiler.IdMapMultiplier;
                            sourceTile.Mutate(c => c.DrawHugeImage(idmap, pos, 1.0f));
                            ImageryTileHelper.FillEdges(tiler.IdMapFullImageSize, x, tiler.Segments.GetLength(0), sourceTile, y, -pos);
                            var tex = ReduceColors(sourceTile, targetTile);
                            var rvmat = MakeRvMat(seg, config, tex.Select(t => t.Material), textureScale);
                            gameFileSystemWriter.WritePngImage($"{config.PboPrefix}\\data\\layers\\M_{x:000}_{y:000}_lca.png", targetTile);
                            gameFileSystemWriter.WriteTextFile($"{config.PboPrefix}\\data\\layers\\P_{x:000}-{y:000}.rvmat", rvmat);
                            report.ReportOneDone();
                            usedMaterials.UnionWith(tex.Select(t => t.Material));
                        }
                    }
                }
                allUsed.Enqueue(usedMaterials);
            });
            return new HashSet<TerrainMaterial>(allUsed.SelectMany(o => o));
        }

        public static double GetTextureScale(IArma3MapConfig config, TerrainMaterialLibrary materialLibrary)
        {
            return config.SizeInMeters / WrpCompiler.LandRange(config.SizeInMeters) / materialLibrary.TextureSizeInMeters;
        }

        private List<TextureStats> ReduceColors(Image<Rgb24> source, Image<Rgba32> target)
        {
            var dict = GetStats(source);
            var colors = dict.Values.Distinct().OrderByDescending(c => c.Count).ToList();
            foreach (var pair in colors.Zip(RuleColors))
            {
                pair.First.Target = pair.Second;
            }
            foreach (var texture in colors.Skip(RuleColors.Length)) // Ignore colors above 6, replace with dominant texture
            {
                texture.Target = RuleColors[0];
            }
            for (var x = 0; x < source.Width; x++)
            {
                for (var y = 0; y < source.Height; y++)
                {
                    target[x, y] = dict[source[x, y]].Target;
                }
            }
            if (colors.Count > 4)
            {
                // Apply alpha channel around pixel to reduce DXT artefacts
                for (var x = 0; x < source.Width; x++)
                {
                    for (var y = 0; y < source.Height; y++)
                    {
                        var pxColor = target[x, y];
                        if (pxColor == RuleColors[4] || pxColor == RuleColors[5])
                        {
                            ForceAlpha(pxColor.A, target, x, y, RuleColors[3]);
                        }
                    }
                }
            }
            return colors;
        }

        private Dictionary<Rgb24, TextureStats> GetStats(Image<Rgb24> source)
        {
            var dict = new Dictionary<Rgb24, TextureStats>();
            for (var x = 0; x < source.Width; x++)
            {
                for (var y = 0; y < source.Height; y++)
                {
                    GetMaterial(dict, source[x, y]).Count++;
                }
            }
            return dict;
        }

        private TextureStats GetMaterial(Dictionary<Rgb24, TextureStats> dict, Rgb24 color)
        {
            if (!dict.TryGetValue(color, out var info))
            {
                var material = materialLibrary.GetMaterialById(color);
                if (material.Id != color)
                {
                    // Approximate match : should reuse the stats of the original color
                    if (!dict.TryGetValue(color, out var canonicalInfo))
                    {
                        dict[material.Id] = canonicalInfo = new TextureStats(material);
                    }
                    dict[color] = info = canonicalInfo;
                }
                else
                {
                    dict[color] = info = new TextureStats(material);
                }
            }
            return info;
        }

        private static void ForceAlpha(byte alpha, Image<Rgba32> output, int xc, int yc, Rgba32 exceptFor)
        {
            var x1 = Math.Max(0, xc - 8);
            var x2 = Math.Min(output.Width, xc + 9);
            var y1 = Math.Max(0, yc - 8);
            var y2 = Math.Min(output.Height, yc + 9);
            for(var x = x1; x < x2; x++)
            {
                for (var y = y1; y < y2; y++)
                {
                    var c = output[x, y];
                    if (c != exceptFor && c.A == 255)
                    {
                        c.A = alpha;
                        output[x, y] = c;
                    }
                }
            }
        }

        public static string MakeRvMat(ImageryTile segment, IArma3MapConfig config, IEnumerable<TerrainMaterial> textures, double textureScale = 10)
        {
            var sw = new StringWriter();
            sw.WriteLine(FormattableString.Invariant($@"ambient[]={{1,1,1,1}};
diffuse[]={{1,1,1,1}};
forcedDiffuse[]={{0.02,0.02,0.02,1}};
emmisive[]={{0,0,0,0}};
specular[]={{0,0,0,0}};
specularPower=0;
class Stage0
{{
	texture=""{config.PboPrefix}\data\layers\s_{segment.X:000}_{segment.Y:000}_lco.paa"";
	texGen=3;
}};
class Stage1
{{
	texture=""{config.PboPrefix}\data\layers\m_{segment.X:000}_{segment.Y:000}_lca.paa"";
	texGen=4;
}};
class TexGen3
{{
	uvSource=""worldPos"";
	class uvTransform
	{{
		aside[]={{{segment.UA:F12},0,0}};
		up[]={{0,0,{segment.UA:F12}}};
		dir[]={{0,-{segment.UA:F12},0}};
		pos[]={{{segment.UB:F12},{segment.VB:F12},0}};
	}};
}};
class TexGen4
{{
	uvSource=""worldPos"";
	class uvTransform
	{{
		aside[]={{{segment.UA:F12},0,0}};
		up[]={{0,0,{segment.UA:F12}}};
		dir[]={{0,-{segment.UA:F12},0}};
		pos[]={{{segment.UB:F12},{segment.VB:F12},0}};
	}};
}};
class TexGen0
{{
	uvSource=""tex"";
	class uvTransform
	{{
		aside[]={{1,0,0}};
		up[]={{0,1,0}};
		dir[]={{0,0,1}};
		pos[]={{0,0,0}};
	}};
}};
class TexGen1
{{
	uvSource=""tex"";
	class uvTransform
	{{
		aside[]={{{textureScale:F12},0,0}};
		up[]={{0,{textureScale:F12},0}};
		dir[]={{0,0,{textureScale:F12}}};
		pos[]={{0,0,0}};
	}};
}};
class TexGen2
{{
	uvSource=""tex"";
	class uvTransform
	{{
		aside[]={{{textureScale:F12},0,0}};
		up[]={{0,{textureScale:F12},0}};
		dir[]={{0,0,{textureScale:F12}}};
		pos[]={{0,0,0}};
	}};
}};
PixelShaderID=""TerrainX"";
VertexShaderID=""Terrain"";
class Stage2
{{
	texture=""#(rgb,1,1,1)color(0.5,0.5,0.5,1,cdt)"";
	texGen=0;
}};"));
            /*
*/
            int stage = 3; 
            foreach(var texture in textures)
            {
                sw.WriteLine(FormattableString.Invariant($@"class Stage{stage}
{{
	texture=""{texture.GetNormalTexturePath(config)}"";
	texGen=1;
}};
class Stage{stage+1}
{{
	texture=""{texture.GetColorTexturePath(config)}"";
	texGen=2;
}};"));
                stage += 2;
            }

            return sw.ToString();
        }

    }
}
