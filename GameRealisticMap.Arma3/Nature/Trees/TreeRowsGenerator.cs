using GameRealisticMap.Algorithms;
using GameRealisticMap.Algorithms.Following;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.Assets.Rows;
using GameRealisticMap.Arma3.TerrainBuilder;
using GameRealisticMap.Nature.Trees;
using GameRealisticMap.Reporting;

namespace GameRealisticMap.Arma3.Nature.Trees
{
    internal class TreeRowsGenerator : ITerrainBuilderLayerGenerator
    {
        private readonly IProgressSystem progress;
        private readonly IArma3RegionAssets assets;

        public TreeRowsGenerator(IProgressSystem progress, IArma3RegionAssets assets)
        {
            this.progress = progress;
            this.assets = assets;
        }

        public IEnumerable<TerrainBuilderObject> Generate(IArma3MapConfig config, IContext context)
        {
            var result = new List<TerrainBuilderObject>();
            var definitions = assets.GetNaturalRows(NaturalRowType.TreeRow);
            if (definitions.Count > 0)
            {
                var layer = new List<PlacedModel<Composition>>();
                var rows = context.GetData<TreeRowsData>().Rows;
                foreach (var row in rows.ProgressStep(progress, "TreeRows"))
                {
                    var random = RandomHelper.CreateRandom(row.FirstPoint);
                    FollowPathWithObjects.PlaceOnPathNotFitted(random, definitions.GetRandom(random), layer, row.Points);
                }
            }
            return result;
        }
    }
}
