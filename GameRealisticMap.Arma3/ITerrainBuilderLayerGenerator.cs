using GameRealisticMap.Arma3.TerrainBuilder;

namespace GameRealisticMap.Arma3
{
    public interface ITerrainBuilderLayerGenerator
    {
        IEnumerable<TerrainBuilderObject> Generate(IArma3MapConfig config, IContext context);
    }
}