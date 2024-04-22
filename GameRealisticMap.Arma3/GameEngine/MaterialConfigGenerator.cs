using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.GameEngine.Materials;
using GameRealisticMap.Arma3.IO;

namespace GameRealisticMap.Arma3.GameEngine
{
    public static class MaterialConfigGenerator
    {
        public static void GenerateConfigFiles(IGameFileSystemWriter gameFileSystemWriter, IArma3MapConfig config, TerrainMaterialLibrary materials)
        {
            gameFileSystemWriter.WriteTextFile($"{config.PboPrefix}\\clutter.hpp", GenerateClutters(materials.Surfaces));

            gameFileSystemWriter.WriteTextFile($"{config.PboPrefix}\\surfaces.hpp", GenerateSurfaces(materials.Definitions));
        }

        internal static string GenerateSurfaces(List<TerrainMaterialDefinition> definitions)
        {
            var surfaces = definitions.Where(d => d.Data != null && d.Surface != null).Select(d => d.Surface!);
            if (!surfaces.Any())
            {
                return string.Empty;
            }
            var sw = new StringWriter();
            sw.WriteLine("class CfgSurfaces {");
            sw.WriteLine("class Default;");
            foreach (var surface in surfaces)
            {
                surface.WriteCfgSurfacesTo(sw);
            }
            sw.WriteLine("};");
            sw.WriteLine("class CfgSurfaceCharacters {");
            foreach (var surface in surfaces)
            {
                surface.WriteCfgSurfaceCharactersTo(sw);
            }
            sw.WriteLine("};");
            return sw.ToString();
        }

        internal static string GenerateClutters(IEnumerable<SurfaceConfig> surfaces)
        {
            var sw = new StringWriter();
            var done = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var clutter in surfaces.SelectMany(s => s.Character))
            {
                if (done.Add(clutter.Name))
                {
                    clutter.WriteTo(sw);
                }
            }
            return sw.ToString();
        }
    }
}
