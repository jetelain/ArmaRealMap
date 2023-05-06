using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.IO;
using GameRealisticMap.Reporting;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace GameRealisticMap.Arma3.GameEngine
{
    public class ImageryCompiler
    {
        private readonly ITerrainMaterialLibrary materialLibrary;
        private readonly IProgressSystem progress;
        private readonly IGameFileSystemWriter gameFileSystemWriter;

        private class TextureStats
        {
            public TextureStats(ITerrainMaterial terrainMaterial)
            {
                Material = terrainMaterial;
            }

            public ITerrainMaterial Material { get; }

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

        public ImageryCompiler(ITerrainMaterialLibrary materialLibrary, IProgressSystem progress, IGameFileSystemWriter gameFileSystemWriter)
        {
            this.materialLibrary = materialLibrary;
            this.progress = progress;
            this.gameFileSystemWriter = gameFileSystemWriter;
        }

        public ImageryTiler Compile(IArma3MapConfig config, IImagerySource source)
        {
            gameFileSystemWriter.CreateDirectory($"{config.PboPrefix}\\data\\layers");

            var tiler = new ImageryTiler(config.TileSize, config.Resolution, config.GetImagerySize());

            using (var idMap = source.CreateIdMap())
            {
                GenerateIdMapTilesAndRvMat(config, idMap, tiler);
            }

            using (var satMap = source.CreateSatMap())
            {
                GenerateSatMapTiles(config, satMap, tiler);
            }

            return tiler;
        }

        private void GenerateSatMapTiles(IArma3MapConfig config, Image satMap, ImageryTiler tiler)
        {
            using var report = progress.CreateStep("SatMapTiling", tiler.Segments.Length);
            Parallel.For(0, tiler.Segments.GetLength(0), x =>
            {
                using (var tile = new Image<Rgb24>(tiler.TileSize, tiler.TileSize, Color.Black.ToPixel<Rgb24>()))
                {
                    for (var y = 0; y < tiler.Segments.GetLength(1); ++y)
                    {
                        var seg = tiler.Segments[x, y];
                        var pos = -seg.ImageTopLeft;
                        tile.Mutate(c => c.DrawImage(satMap, pos, 1.0f));
                        ImageryTileHelper.FillEdges(tiler.FullImageSize, x, tiler.Segments.GetLength(0), tile, y, pos);
                        gameFileSystemWriter.WritePngImage($"{config.PboPrefix}\\data\\layers\\S_{x:000}_{y:000}_lco.png", tile);
                    }
                }
            });
        }

        private void GenerateIdMapTilesAndRvMat(IArma3MapConfig config, Image idmap, ImageryTiler tiler)
        {
            using var report = progress.CreateStep("IdMapTiling", tiler.Segments.Length);
            Parallel.For(0, tiler.Segments.GetLength(0), x =>
            {
                using (var sourceTile = new Image<Rgb24>(tiler.TileSize, tiler.TileSize, Color.Black.ToPixel<Rgb24>()))
                {
                    using (var targetTile = new Image<Rgba32>(tiler.TileSize, tiler.TileSize, Color.Black.ToPixel<Rgba32>()))
                    {
                        for (var y = 0; y < tiler.Segments.GetLength(1); ++y)
                        {
                            var seg = tiler.Segments[x, y];
                            var pos = -seg.ImageTopLeft;
                            sourceTile.Mutate(c => c.DrawImage(idmap, pos, 1.0f));
                            ImageryTileHelper.FillEdges(tiler.FullImageSize, x, tiler.Segments.GetLength(0), sourceTile, y, pos);
                            var tex = ReduceColors(sourceTile, targetTile);
                            var rvmat = MakeRvMat(seg, config, tex.Select(t => t.Material));
                            gameFileSystemWriter.WritePngImage($"{config.PboPrefix}\\data\\layers\\M_{x:000}_{y:000}_lca.png", targetTile);
                            gameFileSystemWriter.WriteTextFile($"{config.PboPrefix}\\data\\layers\\P_{x:000}-{y:000}.rvmat", rvmat);
                            report.ReportOneDone();
                        }
                    }
                }
            });
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

        public static string MakeRvMat(ImageryTile segment, IArma3MapConfig config, IEnumerable<ITerrainMaterial> textures)
        {
            var textureScale = config.GetTextureScale();
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
	texture=""{texture.NormalTexture}"";
	texGen=1;
}};
class Stage{stage+1}
{{
	texture=""{texture.ColorTexture}"";
	texGen=2;
}};"));
                stage += 2;
            }

            return sw.ToString();
        }

    }
}
