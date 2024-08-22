using GameRealisticMap.ElevationModel;
using GameRealisticMap.ManMade.Places;
using GameRealisticMap.ManMade.Roads;
using GameRealisticMap.ManMade;
using GameRealisticMap.Nature.Ocean;
using Pmad.ProgressTracking;

namespace GameRealisticMap.Conditions
{
    internal class ConditionEvaluatorBuilder : IDataBuilderAsync<ConditionEvaluator>
    {
        public IEnumerable<IDataDependency> Dependencies => [
            new DataDependency<CategoryAreaData>(),
            new DataDependency<CitiesData>(),
            new DataDependency<ElevationData>(),
            new DataDependency<OceanData>(),
            new DataDependency<RoadsData>()
            ];

        public Task<ConditionEvaluator> BuildAsync(IBuildContext context, IProgressScope scope)
        {
            return ConditionEvaluator.CreateAsync(context);
        }
    }
}
