using System.Diagnostics;
using System.IO;
using SixLabors.ImageSharp.PixelFormats;

namespace ArmaRealMap.GroundTextureDetails
{
    internal static class GDTConfigBuilder
    {

        internal static void PrepareGDT(Config config)
        {
            if (!Directory.Exists(config.Target.GroundDetailTextures))
            {
                Directory.CreateDirectory(config.Target.GroundDetailTextures);

            }

            GenerateLayersConfig(config);

            PrepareTexturesFromLibrary(config);
        }

        private static void PrepareTexturesFromLibrary(Config config)
        {
            File.Copy(Path.Combine(config.Libraries, "gdt", "CfgSurfaces.hpp"), Path.Combine(config.Target.GroundDetailTextures, "CfgSurfaces.hpp"), true);
            File.Copy(Path.Combine(config.Libraries, "gdt", "Clutter.hpp"), Path.Combine(config.Target.GroundDetailTextures, "Clutter.hpp"), true);

            foreach (var gdt in TerrainMaterial.All)
            {
                var rvmat = Path.Combine(config.Libraries, "gdt", gdt.RvMat(config.Terrain));
                if (File.Exists(rvmat))
                {
                    CopyRvMat(config, gdt, rvmat);
                }
                else
                {
                    var generic = Path.Combine(config.Libraries, "gdt", gdt.RvMatGeneric);
                    if (File.Exists(generic))
                    {
                        CopyRvMat(config, gdt, generic);
                    }
                    else
                    {
                        Trace.TraceWarning("Missing rvmat: {0} or {1}", rvmat, generic);
                    }
                }
            }
        }

        private static void CopyRvMat(Config config, TerrainMaterial gdt, string sourceRvMat)
        {
            File.WriteAllText(
                Path.Combine(config.Target.GroundDetailTextures, gdt.RvMat(config.Terrain)),
                File.ReadAllText(sourceRvMat).Replace("$PBOPREFIX$", config.Target.PboPrefix)
                );

            foreach (var texture in Directory.GetFiles(Path.Combine(config.Libraries, "gdt"), Path.GetFileNameWithoutExtension(sourceRvMat) + "*.paa"))
            {
                File.Copy(texture, Path.Combine(config.Target.GroundDetailTextures, Path.GetFileName(texture)), true);
            }
        }

        internal static void GenerateLayersConfig(Config config)
        {
            using (var writer = new StreamWriter(new FileStream(Path.Combine(config.Target.GroundDetailTextures, "layers.cfg"), FileMode.Create, FileAccess.Write)))
            {
                writer.WriteLine("class Layers");
                writer.WriteLine("{");
                foreach (var gdt in TerrainMaterial.All)
                {
                    writer.WriteLine($"  class gtd_{gdt.ClassName(config.Terrain)}");
                    writer.WriteLine("  {");
                    writer.WriteLine($@"    material=""{Path.Combine(config.Target.PboPrefix, "data", "gdt", gdt.RvMat(config.Terrain))}"";");
                    writer.WriteLine("  };");
                }
                writer.WriteLine("};");
                writer.WriteLine("class Legend");
                writer.WriteLine("{");
                writer.WriteLine(@"  picture=""mapLegend.png"";");
                writer.WriteLine("  class Colors");
                writer.WriteLine("  {");
                foreach (var gtd in TerrainMaterial.All)
                {
                    var rgb = gtd.DefaultColor.ToPixel<Rgb24>();
                    writer.WriteLine($@"    gtd_{gtd.ClassName(config.Terrain)}[]={{{{{rgb.R},{rgb.G},{rgb.B}}}}};");
                }
                writer.WriteLine("  };");
                writer.WriteLine("};");
            }
        }
    }
}
