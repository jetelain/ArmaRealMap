using GameRealisticMap.Algorithms;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.TerrainBuilder;
using GameRealisticMap.ManMade.Objects;
using GameRealisticMap.Nature.Trees;
using GameRealisticMap.Reporting;

namespace GameRealisticMap.Arma3.Nature.Trees
{
    internal class TreesGenerator : ITerrainBuilderLayerGenerator
    {
        private readonly IProgressSystem progress;
        private readonly IArma3RegionAssets assets;

        public TreesGenerator(IProgressSystem progress, IArma3RegionAssets assets)
        {
            this.progress = progress;
            this.assets = assets;
        }

        public IEnumerable<TerrainBuilderObject> Generate(IArma3MapConfig config, IContext context)
        {
            var result = new List<TerrainBuilderObject>();
            var candidates = assets.GetObjects(ObjectTypeId.Tree);
            if (candidates.Count > 0)
            {
                var points = context.GetData<TreesData>().Points;
                foreach (var point in points.ProgressStep(progress, "Trees"))
                {
                    var random = new Random((int)Math.Truncate(point.X + point.Y));
                    var definition = candidates.GetRandom(random);
                    result.AddRange(definition.Composition.ToTerrainBuilderObjects(new ModelPosition(point, (float)(random.NextDouble() * 360))));
                }
            }
            return result;
        }
    }
}
