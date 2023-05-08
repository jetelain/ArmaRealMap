using GameRealisticMap.Algorithms;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.TerrainBuilder;
using GameRealisticMap.ManMade.Objects;
using GameRealisticMap.Reporting;

namespace GameRealisticMap.Arma3.ManMade
{
    internal class OrientedObjectsGenerator : ITerrainBuilderLayerGenerator
    {
        private readonly IProgressSystem progress;
        private readonly IArma3RegionAssets assets;

        public OrientedObjectsGenerator(IProgressSystem progress, IArma3RegionAssets assets)
        {
            this.progress = progress;
            this.assets = assets;
        }

        public IEnumerable<TerrainBuilderObject> Generate(IArma3MapConfig config, IContext context)
        {
            var objects = context.GetData<OrientedObjectData>().Objects;
            var result = new List<TerrainBuilderObject>();
            foreach(var obj in objects.ProgressStep(progress,"OrientedObjects"))
            {
                var candidates = assets.GetObjects(obj.TypeId);
                if (candidates.Count >  0)
                {
                    var random = new Random((int)Math.Truncate(obj.Point.X + obj.Point.Y));
                    var definition = candidates.GetRandom(random);
                    result.AddRange(definition.Composition.ToTerrainBuilderObjects(new ModelPosition(obj.Point, obj.Angle)));
                }
            }
            return result;
        }
    }
}
