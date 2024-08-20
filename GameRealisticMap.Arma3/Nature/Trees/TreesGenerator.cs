using GameRealisticMap.Algorithms;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.TerrainBuilder;
using GameRealisticMap.Conditions;
using GameRealisticMap.ManMade.Objects;
using GameRealisticMap.Nature.Trees;
using Pmad.ProgressTracking;

namespace GameRealisticMap.Arma3.Nature.Trees
{
    internal class TreesGenerator : ITerrainBuilderLayerGenerator
    {
        private readonly IArma3RegionAssets assets;

        public TreesGenerator(IArma3RegionAssets assets)
        {
            this.assets = assets;
        }

        public IEnumerable<TerrainBuilderObject> Generate(IArma3MapConfig config, IContext context, IProgressScope scope)
        {
            var evaluator = context.GetData<ConditionEvaluator>();

            var result = new List<TerrainBuilderObject>();
            var candidates = assets.GetObjects(ObjectTypeId.Tree);
            if (candidates.Count > 0)
            {
                var points = context.GetData<TreesData>().Points;
                foreach (var point in points.WithProgress(scope, "Trees"))
                {
                    var random = RandomHelper.CreateRandom(point);
                    var definition = candidates.GetRandom(random, evaluator.GetPointContext(point));
                    if (definition != null)
                    {
                        result.AddRange(definition.Composition.ToTerrainBuilderObjects(new ModelPosition(point, (float)(random.NextDouble() * 360))));
                    }
                }
            }
            return result;
        }
    }
}
