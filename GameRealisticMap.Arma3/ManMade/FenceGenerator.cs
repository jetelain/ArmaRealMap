using GameRealisticMap.Algorithms;
using GameRealisticMap.Algorithms.Following;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.TerrainBuilder;
using GameRealisticMap.Conditions;
using GameRealisticMap.ManMade.Fences;
using Pmad.ProgressTracking;

namespace GameRealisticMap.Arma3.ManMade
{
    internal class FenceGenerator : ITerrainBuilderLayerGenerator
    {
        private readonly IArma3RegionAssets assets;

        public FenceGenerator(IArma3RegionAssets assets)
        {
            this.assets = assets;
        }

        public async Task<IEnumerable<TerrainBuilderObject>> Generate(IArma3MapConfig config, IContext context, IProgressScope scope)
        {
            var evaluator = await context.GetDataAsync<ConditionEvaluator>();
            var fences = (await context.GetDataAsync<FencesData>()).Fences;
            var layer = new List<PlacedModel<Composition>>();
            foreach (var fence in fences.WithProgress(scope, "Fences"))
            {
                var lib = assets.GetFences(fence.TypeId);
                if (lib.Count != 0 && fence.Path.Points.Count > 1)
                {
                    var random = RandomHelper.CreateRandom(fence.Path.FirstPoint);
                    var def = lib.GetRandom(random, evaluator.GetPathContext(fence.Path));
                    if (def != null)
                    {
                        if (def.Straights.Count > 0)
                        {
                            FollowPathWithObjects.PlaceOnPathRightAngle(random, def, layer, fence.Path.Points);
                        }
                        else if (def.Objects.Count > 0)
                        {
                            FollowPathWithObjects.PlaceObjectsOnPath(random, def.Objects, layer, fence.Path.Points);
                        }
                    }
                }

            }
            return layer.SelectMany(o => o.Model.ToTerrainBuilderObjects(o));
        }
    }
}
