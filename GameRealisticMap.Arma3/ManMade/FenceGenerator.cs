using GameRealisticMap.Algorithms;
using GameRealisticMap.Algorithms.Following;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.TerrainBuilder;
using GameRealisticMap.ManMade.Fences;
using GameRealisticMap.Reporting;

namespace GameRealisticMap.Arma3.ManMade
{
    internal class FenceGenerator : ITerrainBuilderLayerGenerator
    {
        private readonly IProgressSystem progress;
        private readonly IArma3RegionAssets assets;

        public FenceGenerator(IProgressSystem progress, IArma3RegionAssets assets)
        {
            this.progress = progress;
            this.assets = assets;
        }

        public IEnumerable<TerrainBuilderObject> Generate(IArma3MapConfig config, IContext context)
        {
            var fences = context.GetData<FencesData>().Fences;
            var layer = new List<PlacedModel<Composition>>();
            foreach (var fence in fences.ProgressStep(progress, "Fences"))
            {
                var lib = assets.GetFences(fence.TypeId);
                if (lib.Count != 0 && fence.Path.Points.Count > 1)
                {
                    var random = RandomHelper.CreateRandom(fence.Path.FirstPoint);
                    var def = lib.GetRandom(random);
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
            return layer.SelectMany(o => o.Model.ToTerrainBuilderObjects(o));
        }
    }
}
