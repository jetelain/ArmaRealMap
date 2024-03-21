using GameRealisticMap.Arma3.GameEngine.Materials;

namespace GameRealisticMap.Arma3.Assets
{
    public class TerrainMaterialDefinition
    {
        public TerrainMaterialDefinition(TerrainMaterial material, TerrainMaterialUsage[] usages, SurfaceConfig? surface)
        {
            Material = material;
            Usages = usages;
            Surface = surface;

            if (surface != null /*&& !string.IsNullOrEmpty(surface.Files)*/ && !surface.Match(Path.GetFileNameWithoutExtension(material.ColorTexture)))
            {
                throw new ArgumentException($"Pattern '{surface.Files}' does not matches material file '{material.ColorTexture}'");
            }
        }

        public TerrainMaterial Material { get; }

        public TerrainMaterialUsage[] Usages { get; }

        public SurfaceConfig? Surface { get; }
    }
}