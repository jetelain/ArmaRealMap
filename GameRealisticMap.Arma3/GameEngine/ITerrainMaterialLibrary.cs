using SixLabors.ImageSharp.PixelFormats;

namespace GameRealisticMap.Arma3.GameEngine
{
    public interface ITerrainMaterialLibrary
    {
        ITerrainMaterial GetMaterialById(Rgb24 color);
        ITerrainMaterial Default { get; }
        ITerrainMaterial DefaultUrban { get; }
        ITerrainMaterial DefaultIndustrial { get; }
        ITerrainMaterial ForestGround { get; }
        ITerrainMaterial Sand { get; }
        ITerrainMaterial Grass { get; }
        ITerrainMaterial Meadow { get; }
        ITerrainMaterial FarmLand { get; }
        ITerrainMaterial RiverGround { get; }
        ITerrainMaterial LakeGround { get; }
    }
}