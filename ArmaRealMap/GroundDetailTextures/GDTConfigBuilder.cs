using System.Diagnostics;
using System.IO;
using SixLabors.ImageSharp.PixelFormats;

namespace ArmaRealMap.GroundTextureDetails
{
    internal static class GDTConfigBuilder
    {

        internal static void PrepareGDT(Config config)
        {
            GenerateLayersConfig(config);

            PrepareTexturesFromLibrary(config);
        }

        private static void PrepareTexturesFromLibrary(Config config)
        {
            Directory.CreateDirectory(config.Target.GroundDetailTextures);

            File.Copy(Path.Combine(config.Libraries, "gdt", "CfgSurfaces.hpp"), Path.Combine(config.Target.GroundDetailTextures, "CfgSurfaces.hpp"), true);
            File.Copy(Path.Combine(config.Libraries, "gdt", "Clutter.hpp"), Path.Combine(config.Target.GroundDetailTextures, "Clutter.hpp"), true);

            foreach (var gdt in TerrainMaterial.All)
            {
                var rvmat = Path.Combine(config.Libraries, "gdt", gdt.RvMat);
                if (File.Exists(rvmat))
                {
                    File.WriteAllText(
                        Path.Combine(config.Target.GroundDetailTextures, gdt.RvMat),
                        File.ReadAllText(rvmat).Replace("$PBOPREFIX$", config.Target.PboPrefix)
                        );

                    foreach (var texture in Directory.GetFiles(Path.Combine(config.Libraries, "gdt"), Path.GetFileNameWithoutExtension(gdt.RvMat) + "*.paa"))
                    {
                        File.Copy(texture, Path.Combine(config.Target.GroundDetailTextures, Path.GetFileName(texture)), true);
                    }
                }
                else
                {
                    Trace.TraceWarning("Missing rvmat: {0}", rvmat);
                }
            }
        }

        internal static void GenerateLayersConfig(Config config)
        {
            using (var writer = new StreamWriter(new FileStream(Path.Combine(config.Target?.Terrain ?? string.Empty, "layers.cfg"), FileMode.Create, FileAccess.Write)))
            {
                writer.WriteLine("class Layers");
                writer.WriteLine("{");
                foreach (var gdt in TerrainMaterial.All)
                {
                    writer.WriteLine($"  class gtd_{gdt.ClassName}");
                    writer.WriteLine("  {");
                    writer.WriteLine($@"    material=""{Path.Combine(config.Target.PboPrefix, "data", "gdt", gdt.RvMat)}"";");
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
                    var rgb = gtd.Color.ToPixel<Rgb24>();
                    writer.WriteLine($@"    gtd_{gtd.ClassName}[]={{{{{rgb.R},{rgb.G},{rgb.B}}}}};");
                }
                writer.WriteLine("  };");
                writer.WriteLine("};");
            }
        }
    }
}
