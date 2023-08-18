using GameRealisticMap.Algorithms;
using GameRealisticMap.Algorithms.Following;
using GameRealisticMap.Algorithms.Rows;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.TerrainBuilder;
using GameRealisticMap.ManMade.Farmlands;
using GameRealisticMap.Reporting;

namespace GameRealisticMap.Arma3.ManMade.Farmlands
{
    internal class OrchardGenerator : ITerrainBuilderLayerGenerator
    {
        private readonly IProgressSystem progress;
        private readonly IArma3RegionAssets assets;

        public OrchardGenerator(IProgressSystem progress, IArma3RegionAssets assets)
        {
            this.progress = progress;
            this.assets = assets;
        }

        public IEnumerable<TerrainBuilderObject> Generate(IArma3MapConfig config, IContext context)
        {
            var layer = new List<PlacedModel<Composition>>();
            var lib = assets.GetNaturalRows(Assets.Rows.NaturalRowType.OrchardRow);
            if (lib.Count > 0)
            {
                var orchards = context.GetData<OrchardData>().Polygons;
                foreach (var orchard in orchards.ProgressStep(progress, "Orchards"))
                {
                    var rnd = RandomHelper.CreateRandom(orchard.Centroid);
                    FillAreaWithRows.Fill(RandomHelper.CreateRandom(orchard.Centroid), lib.GetRandom(rnd), layer, orchard);
                }
            }
            return layer.SelectMany(o => o.Model.ToTerrainBuilderObjects(o));
        }
    }
}
