using GameRealisticMap.Geometries;

namespace GameRealisticMap.Conditions
{
    public interface IConditionContext<TGeometry> 
        where TGeometry : ITerrainEnvelope
    {
    }
}
