using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ArmaRealMap.Core.ObjectLibraries;
using ArmaRealMap.GroundTextureDetails;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace ArmaRealMap.TerrainData.GroundDetailTextures
{
    internal static class IdMapCompiler
    {
        class TextureInfo
        {
            public TerrainMaterial Material { get; internal set; }
            public int Count { get; internal set; }
            public Rgba32 Target { get; internal set; }
        }

        private static Rgba32[] RuleColors = new[] {
            new Rgba32(0, 0, 0, 255), // Stage3
            new Rgba32(255, 0, 0, 255),  // Stage5
            new Rgba32(0, 255, 0, 255),  // Stage7
            new Rgba32(0, 0, 255, 255), // Stage9
            new Rgba32(0, 0, 255, 128), // Stage11 (need additional processing)
            // new Rgba32(???, ???, ???, 0) // Stage13 (need additional processing)
        };

        public static void Compile(MapInfos area, MapConfig config, TerrainMaterialLibrary terrainMaterialLibrary)
        {
            using (var idmap = Image.Load<Rgb24>(Path.Combine(config.Target.Terrain, "idmap", "idmap.png")))
            {
                Compile(area, config, idmap, terrainMaterialLibrary);
            }
        }

        public static void Compile(MapInfos area, MapConfig config, Image idmap, TerrainMaterialLibrary terrainMaterialLibrary)
        {
            Directory.CreateDirectory(LayersHelper.GetLocalPath(config));
            var tiler = new TerrainTiler(area, config);
            var report2 = new ProgressReport("Tiling", tiler.Segments.Length);
            Parallel.For(0, tiler.Segments.GetLength(0), x =>
            {
                using (var tile = new Image<Rgb24>(config.TileSize, config.TileSize, Color.Black))
                {
                    using (var output = new Image<Rgba32>(config.TileSize, config.TileSize, Color.Black))
                    {
                        for (var y = 0; y < tiler.Segments.GetLength(1); ++y)
                        {
                            var seg = tiler.Segments[x, y];
                            var pos = -seg.ImageTopLeft;
                            tile.Mutate(c => c.DrawImage(idmap, pos, 1.0f));
                            LayersHelper.FillEdges(idmap, x, tiler.Segments.GetLength(0), tile, y, pos);

                            var tex = ReduceColors(tile, output, config.Terrain);
                            var rvmat = MakeRvMat(seg, 40, tex, config.Terrain, config, terrainMaterialLibrary);


                            output.Save(LayersHelper.GetLocalPath(config, $"M_{x:000}_{y:000}_lca.png"));
                            //tile.Save(LayersHelper.GetPath(config,$"M_{x:000}-{y:000}_ori.png"));
                            File.WriteAllText(LayersHelper.GetLocalPath(config, $"P_{x:000}-{y:000}.rvmat"), rvmat);
                            report2.ReportOneDone();
                        }
                    }
                }
            });
            report2.TaskDone();

            LayersHelper.ImageToPAA(tiler.Segments.GetLength(0), x => LayersHelper.GetLocalPath(config, $"M_{x:000}_*_lca.png"));
        }

        private static List<TextureInfo> ReduceColors(Image<Rgb24> tile, Image<Rgba32> output, TerrainRegion region)
        {
            var dict = new Dictionary<Rgb24, TextureInfo>();
            for (var x = 0; x < tile.Width; x++)
            {
                for (var y = 0; y < tile.Height; y++)
                {
                    var color = tile[x, y];
                    if (!dict.TryGetValue(color, out var info))
                    {
                        dict[color] = info = new TextureInfo() { Material = TerrainMaterial.All.FirstOrDefault(m => m.GetColor(region).ToPixel<Rgb24>() == color) ?? TerrainMaterial.Default.GetMaterial(region) };
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
            for (var x = 0; x < tile.Width; x++)
            {
                for (var y = 0; y < tile.Height; y++)
                {
                    output[x, y] = dict[tile[x, y]].Target;
                }
            }
            if (colors.Count > 4)
            {
                // TODO: Extra Alpha Processing : Protect against texture interpolation 
                for (var x = 0; x < tile.Width; x++)
                {
                    for (var y = 0; y < tile.Height; y++)
                    {
                        if (output[x, y] == RuleColors[4])
                        {
                            // ? Force Alpha 128 at 8px range exept for RuleColors[3]
                        }
                    }
                }
            }
            return colors;
        }

        private static string MakeRvMat(LandSegment segment, double textureScale, List<TextureInfo> textures, TerrainRegion terrain, MapConfig config, TerrainMaterialLibrary terrainMaterialLibrary)
        {
            var lco = $"s_{segment.X:000}_{segment.Y:000}_lco.png";
            var lca = $"m_{segment.X:000}_{segment.Y:000}_lca.png";

            var sw = new StringWriter();
            sw.WriteLine(FormattableString.Invariant($@"ambient[]={{1,1,1,1}};
diffuse[]={{1,1,1,1}};
forcedDiffuse[]={{0.02,0.02,0.02,1}};
emmisive[]={{0,0,0,0}};
specular[]={{0,0,0,0}};
specularPower=0;
class Stage0
{{
	texture=""{LayersHelper.GetLogicalPath(config, lco)}"";
	texGen=3;
}};
class Stage1
{{
	texture=""{LayersHelper.GetLogicalPath(config, lca)}"";
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
                var material = terrainMaterialLibrary.GetInfo(texture.Material, terrain);

                sw.WriteLine(FormattableString.Invariant($@"class Stage{stage}
{{
	texture=""{material.NormalTexture}"";
	texGen=1;
}};
class Stage{stage+1}
{{
	texture=""{material.ColorTexture}"";
	texGen=2;
}};"));
                stage += 2;
            }

            return sw.ToString();
        }

    }
}
