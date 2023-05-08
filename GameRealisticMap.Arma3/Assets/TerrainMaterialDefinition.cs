namespace GameRealisticMap.Arma3.Assets
{
    public class TerrainMaterialDefinition
    {
        public TerrainMaterialDefinition(TerrainMaterial material, TerrainMaterialUsage[] usages)
        {
            Material = material;
            Usages = usages;
        }

        public TerrainMaterial Material { get; }

        public TerrainMaterialUsage[] Usages { get; }
    }
}