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
                FollowPathWithObjects.PlaceOnPathRightAngle(assets.GetFences(fence.TypeId), layer, fence.Path.Points);
            }
            return layer.SelectMany(o => o.Model.ToTerrainBuilderObjects(o));
        }
    }
}
