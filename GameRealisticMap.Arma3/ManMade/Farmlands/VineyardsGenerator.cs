using GameRealisticMap.Algorithms;
using GameRealisticMap.Algorithms.Following;
using GameRealisticMap.Algorithms.Rows;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.TerrainBuilder;
using GameRealisticMap.ManMade.Farmlands;
using GameRealisticMap.Reporting;

namespace GameRealisticMap.Arma3.ManMade.Farmlands
{
    internal class VineyardsGenerator : ITerrainBuilderLayerGenerator
    {
        private readonly IProgressSystem progress;
        private readonly IArma3RegionAssets assets;

        public VineyardsGenerator(IProgressSystem progress, IArma3RegionAssets assets)
        {
            this.progress = progress;
            this.assets = assets;
        }

        public IEnumerable<TerrainBuilderObject> Generate(IArma3MapConfig config, IContext context)
        {
            var layer = new List<PlacedModel<Composition>>();
            var lib = assets.GetNaturalRows(Assets.Rows.NaturalRowType.VineyardRow);
            if (lib.Count > 0)
            {
                var vineyards = context.GetData<VineyardData>().Polygons;
                foreach (var vineyard in vineyards.ProgressStep(progress, "Vineyards"))
                {
                    var rnd = RandomHelper.CreateRandom(vineyard.Centroid);
                    FillAreaWithRows.Fill(RandomHelper.CreateRandom(vineyard.Centroid), lib.GetRandom(rnd), layer, vineyard);
                }
            }
            return layer.SelectMany(o => o.Model.ToTerrainBuilderObjects(o));
        }
    }
}
