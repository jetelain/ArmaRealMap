using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.GameEngine;
using GameRealisticMap.Arma3.IO;
using GameRealisticMap.Reporting;

namespace GameRealisticMap.Arma3.TerrainBuilder
{
    internal class LayersCfgGenerator
    {
        private readonly TerrainMaterialLibrary materials;
        private readonly IGameFileSystemWriter fileSystemWriter;
        private readonly IProgressSystem progress;

        public LayersCfgGenerator(TerrainMaterialLibrary materials, IProgressSystem progress, IGameFileSystemWriter fileSystemWriter)
        {
            this.materials = materials;
            this.fileSystemWriter = fileSystemWriter;
            this.progress = progress;
        }

        public void WriteLayersCfg(IArma3MapConfig config)
        {
            TerrainMaterialHelper.UnpackEmbeddedFiles(materials, progress, fileSystemWriter, config);

            fileSystemWriter.CreateDirectory($"{config.PboPrefix}\\data\\gdt");

            var textureScale = ImageryCompiler.GetTextureScale(config, materials);
            using var cfg = new StringWriter();
            cfg.WriteLine("class Layers {");
            foreach (var def in materials.Definitions)
            {
                var name = def.Usages.First().ToString().ToLowerInvariant();
                var rvmat = $"{config.PboPrefix}\\data\\gdt\\arm_{name}.rvmat";
                cfg.WriteLine($"    class arm_{name} {{");
                cfg.WriteLine($"        material = \"{rvmat}\";");
                cfg.WriteLine("    };");
                fileSystemWriter.WriteTextFile(rvmat, CreateRvMat(def.Material, textureScale, config));
            }
            cfg.WriteLine("};");
            cfg.WriteLine("class Legend {");
            cfg.WriteLine("    picture = \"legend.png\";");
            cfg.WriteLine("    class Colors {");
            foreach (var def in materials.Definitions)
            {
                var name = def.Usages.First().ToString().ToLowerInvariant();
                var color = def.Material.Id;
                cfg.WriteLine(FormattableString.Invariant($"        arm_{name}[] = {{{{ {color.R}, {color.G}, {color.B} }}}};"));
            }
            cfg.WriteLine("    };");
            cfg.WriteLine("};");
            fileSystemWriter.WriteTextFile(config.PboPrefix + "\\data\\gdt\\layers.cfg", cfg.ToString());
        }

        private static string CreateRvMat(TerrainMaterial material, double textureScale, IArma3MapConfig config)
        {
            return
$@"ambient[]={{0.9,0.9,0.9,1}};
diffuse[]={{0.9,0.9,0.9,1}};
forcedDiffuse[]={{0.02,0.02,0.02,1}};
emmisive[]={{0,0,0,0}};
specular[]={{0,0,0,0}};
specularPower=1;
PixelShaderID=""NormalMapDiffuse"";
VertexShaderID=""NormalMapDiffuseAlpha"";
class Stage1
{{
	texture=""{material.GetNormalTexturePath(config)}"";
	uvSource=""tex"";
	class uvTransform
	{{
		aside[]={{{textureScale},0,0}};
		up[]={{0,{textureScale},0}};
		dir[]={{0,0,{textureScale}}};
		pos[]={{0,0,0}};
	}};
}};
class Stage2
{{
	texture=""{material.GetColorTexturePath(config)}"";
	uvSource=""tex"";
	class uvTransform
	{{
		aside[]={{{textureScale},0,0}};
		up[]={{0,{textureScale},0}};
		dir[]={{0,0,{textureScale}}};
		pos[]={{0,0,0}};
	}};
}};";
        }
    }
}
