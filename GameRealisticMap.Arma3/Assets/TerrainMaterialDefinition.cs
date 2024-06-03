using System.Text.Json.Serialization;
using GameRealisticMap.Arma3.GameEngine.Materials;

namespace GameRealisticMap.Arma3.Assets
{
    public class TerrainMaterialDefinition
    {
        public TerrainMaterialDefinition(TerrainMaterial material, TerrainMaterialUsage[] usages, SurfaceConfig? surface = null, TerrainMaterialData? data = null, string? title = null)
        {
            Material = material;
            Usages = usages;
            Surface = surface;
            Data = data;
            Title = title;
            if (surface != null && !surface.Match(Path.GetFileNameWithoutExtension(material.ColorTexture)))
            {
#if DEBUG
                throw new ArgumentException($"Pattern '{surface.Files}' does not matches material file '{material.ColorTexture}'");
#else
                Surface = null;
                Data = null;
#endif
            }
        }

        public TerrainMaterial Material { get; }

        public TerrainMaterialUsage[] Usages { get; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public SurfaceConfig? Surface { get; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public TerrainMaterialData? Data { get; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Title { get; }
    }
}