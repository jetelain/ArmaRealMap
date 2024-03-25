using System.Text.Json.Serialization;
using GameRealisticMap.Arma3.GameEngine.Materials;

namespace GameRealisticMap.Arma3.Assets
{
    public class TerrainMaterialDefinition
    {
        public TerrainMaterialDefinition(TerrainMaterial material, TerrainMaterialUsage[] usages, SurfaceConfig? surface = null, TerrainMaterialData? data = null)
        {
            Material = material;
            Usages = usages;
            Surface = surface;
            Data = data;

            if (surface != null && !surface.Match(Path.GetFileNameWithoutExtension(material.ColorTexture)))
            {
                throw new ArgumentException($"Pattern '{surface.Files}' does not matches material file '{material.ColorTexture}'");
            }
        }

        public TerrainMaterial Material { get; }

        public TerrainMaterialUsage[] Usages { get; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public SurfaceConfig? Surface { get; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public TerrainMaterialData? Data { get; }
    }
}