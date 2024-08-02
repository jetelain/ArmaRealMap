using GameRealisticMap.Algorithms;
using GameRealisticMap.Algorithms.Following;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.Assets.Rows;
using GameRealisticMap.Arma3.TerrainBuilder;
using GameRealisticMap.Nature.Trees;
using Pmad.ProgressTracking;

namespace GameRealisticMap.Arma3.Nature.Trees
{
    internal class TreeRowsGenerator : ITerrainBuilderLayerGenerator
    {
        private readonly IArma3RegionAssets assets;

        public TreeRowsGenerator(IArma3RegionAssets assets)
        {
            this.assets = assets;
        }

        public IEnumerable<TerrainBuilderObject> Generate(IArma3MapConfig config, IContext context, IProgressScope scope)
        {
            var layer = new List<PlacedModel<Composition>>();
            var definitions = assets.GetNaturalRows(NaturalRowType.TreeRow);
            if (definitions.Count > 0)
            {
                var rows = context.GetData<TreeRowsData>().Rows;
                foreach (var row in rows.WithProgress(scope, "TreeRows"))
                {
                    var random = RandomHelper.CreateRandom(row.FirstPoint);
                    FollowPathWithObjects.PlaceOnPathNotFitted(random, definitions.GetRandom(random), layer, row.Points);
                }
            }
            return layer.SelectMany(o => o.Model.ToTerrainBuilderObjects(o));
        }
    }
}
