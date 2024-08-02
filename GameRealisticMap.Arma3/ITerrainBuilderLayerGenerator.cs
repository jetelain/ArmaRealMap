using GameRealisticMap.Arma3.TerrainBuilder;
using Pmad.ProgressTracking;

namespace GameRealisticMap.Arma3
{
    public interface ITerrainBuilderLayerGenerator
    {
        IEnumerable<TerrainBuilderObject> Generate(IArma3MapConfig config, IContext context, IProgressScope scope);
    }
}