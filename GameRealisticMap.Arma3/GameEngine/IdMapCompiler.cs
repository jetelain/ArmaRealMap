using GameRealisticMap.Reporting;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace GameRealisticMap.Arma3.GameEngine
{
    public class IdMapCompiler
    {
        private readonly ITerrainMaterialLibrary materialLibrary;
        private readonly IProgressSystem progress;

        class TextureInfo
        {
            public TextureInfo(ITerrainMaterial terrainMaterial)
            {
                Material = terrainMaterial;
            }
            public ITerrainMaterial Material { get; }
            public int Count { get; internal set; }
            public Rgba32 Target { get; internal set; }
        }

        private static Rgba32[] RuleColors = new[] {
            new Rgba32(0, 0, 0, 255), // Stage3
            new Rgba32(255, 0, 0, 255),  // Stage5
            new Rgba32(0, 255, 0, 255),  // Stage7
            new Rgba32(0, 0, 255, 255), // Stage9
            new Rgba32(0, 0, 255, 128), // Stage11 (need additional processing)
            new Rgba32(0, 0, 255, 0) // Stage13 (need additional processing)
        };

        public IdMapCompiler(ITerrainMaterialLibrary materialLibrary, IProgressSystem progress)
        {
            this.materialLibrary = materialLibrary;
            this.progress = progress;
        }

        public void Compile(IArma3MapConfig config, Image idmap, string targetPath)
        {
            Directory.CreateDirectory(targetPath);

            var tiler = new ImageryTiler(config.TileSize, config.Resolution, config.GetImagerySize());

            GenerateSourceFiles(config, idmap, targetPath, tiler);

            //Arma3ToolsHelper.ImageToPAA(tiler.Segments.GetLength(0), x => Path.Combine(targetPath, $"M_{x:000}_*_lca.png"));
        }

        private void GenerateSourceFiles(IArma3MapConfig config, Image idmap, string targetPath, ImageryTiler tiler)
        {
            using var report = progress.CreateStep("Tiling", tiler.Segments.Length);
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

                            targetTile.Save(Path.Combine(targetPath, $"M_{x:000}_{y:000}_lca.png"));
                            File.WriteAllText(Path.Combine(targetPath, $"P_{x:000}-{y:000}.rvmat"), rvmat);
                            report.ReportOneDone();
                        }
                    }
                }
            });
        }

        private List<TextureInfo> ReduceColors(Image<Rgb24> source, Image<Rgba32> target)
        {
            var dict = new Dictionary<Rgb24, TextureInfo>();
            for (var x = 0; x < source.Width; x++)
            {
                for (var y = 0; y < source.Height; y++)
                {
                    var color = source[x, y];
                    if (!dict.TryGetValue(color, out var info))
                    {
                        dict[color] = info = new TextureInfo(materialLibrary.GetMaterialById(color));
                    }
                    info.Count++;
                }
            }
            var colors = dict.Values.OrderByDescending(c => c.Count).ToList();
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
