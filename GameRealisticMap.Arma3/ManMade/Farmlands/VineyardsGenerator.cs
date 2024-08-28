using GameRealisticMap.Algorithms;
using GameRealisticMap.Algorithms.Following;
using GameRealisticMap.Algorithms.Rows;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.TerrainBuilder;
using GameRealisticMap.ManMade.Farmlands;
using Pmad.ProgressTracking;

namespace GameRealisticMap.Arma3.ManMade.Farmlands
{
    internal class VineyardsGenerator : ITerrainBuilderLayerGenerator
    {
        private readonly IArma3RegionAssets assets;

        public VineyardsGenerator(IArma3RegionAssets assets)
        {
            this.assets = assets;
        }

        public async Task<IEnumerable<TerrainBuilderObject>> Generate(IArma3MapConfig config, IContext context, IProgressScope scope)
        {
            var layer = new List<PlacedModel<Composition>>();
            var lib = assets.GetNaturalRows(Assets.Rows.NaturalRowType.VineyardRow);
            if (lib.Count > 0)
            {
                var vineyards = (await context.GetDataAsync<VineyardData>()).Polygons;
                foreach (var vineyard in vineyards.WithProgress(scope, "Vineyards"))
                {
                    var rnd = RandomHelper.CreateRandom(vineyard.Centroid);
                    FillAreaWithRows.Fill(rnd, lib.GetRandom(rnd), layer, vineyard);
                }
            }
            return layer.SelectMany(o => o.Model.ToTerrainBuilderObjects(o));
        }
    }
}
