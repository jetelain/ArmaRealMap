using GameRealisticMap.Algorithms.Following;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.TerrainBuilder;
using GameRealisticMap.ManMade.Railways;
using GameRealisticMap.Reporting;

namespace GameRealisticMap.Arma3.ManMade
{
    internal class RailwayGenerator : ITerrainBuilderLayerGenerator
    {
        private readonly IProgressSystem progress;
        private readonly RailwaysDefinition? assets;

        public RailwayGenerator(IProgressSystem progress, IArma3RegionAssets assets)
        {
            this.progress = progress;
            this.assets = assets.Railways;
        }

        public IEnumerable<TerrainBuilderObject> Generate(IArma3MapConfig config, IContext context)
        {
            if (assets == null || assets.Straights.Count == 0)
            {
                return Enumerable.Empty<TerrainBuilderObject>();
            }
            var railways = context.GetData<RailwaysData>().Railways;
            var layer = new List<PlacedModel<Composition>>();
            foreach (var fence in railways.ProgressStep(progress, "Railways"))
            {
                FollowPathWithObjects.PlaceOnPath(assets.Straights, layer, fence.Path.Points);
            }
            return layer.SelectMany(o => o.Model.ToTerrainBuilderObjects(o));
        }
    }
}
