using GameRealisticMap.Algorithms;
using GameRealisticMap.Algorithms.Following;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.TerrainBuilder;
using GameRealisticMap.Conditions;
using GameRealisticMap.ManMade.Roads;
using GameRealisticMap.Reporting;

namespace GameRealisticMap.Arma3.ManMade
{
    internal class SidewalksGenerator : ITerrainBuilderLayerGenerator
    {
        private readonly IProgressSystem progress;
        private readonly IArma3RegionAssets assets;

        public SidewalksGenerator(IProgressSystem progress, IArma3RegionAssets assets)
        {
            this.progress = progress;
            this.assets = assets;
        }

        public IEnumerable<TerrainBuilderObject> Generate(IArma3MapConfig config, IContext context)
        {
            var layer = new List<PlacedModel<Composition>>();

            if (assets.Sidewalks.Count != 0)
            {
                var evaluator = context.GetData<ConditionEvaluator>();
                var paths = context.GetData<SidewalksData>().Paths;
                foreach (var path in paths.ProgressStep(progress, "Sidewalks"))
                {
                    if (path.Points.Count > 1)
                    {
                        var random = RandomHelper.CreateRandom(path.FirstPoint);
                        var def = assets.Sidewalks.GetRandom(random, evaluator.GetPathContext(path));
                        if (def != null)
                        {
                            FollowPathWithObjects.PlaceOnPathRightAngle(random, def, layer, path.Points);
                        }
                    }
                }
            }
            return layer.SelectMany(o => o.Model.ToTerrainBuilderObjects(o));
        }
    }
}
