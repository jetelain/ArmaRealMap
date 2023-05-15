using System.Numerics;
using GameRealisticMap.Algorithms;
using GameRealisticMap.Algorithms.Filling;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.TerrainBuilder;
using GameRealisticMap.Geometries;
using GameRealisticMap.Nature;
using GameRealisticMap.Reporting;

namespace GameRealisticMap.Arma3.Nature
{
    public abstract class GeneratorBase<TData> : ITerrainBuilderLayerGenerator 
        where TData : class, IBasicTerrainData
    {
        protected readonly IProgressSystem progress;
        protected readonly IArma3RegionAssets assets;

        public GeneratorBase(IProgressSystem progress, IArma3RegionAssets assets)
        {
            this.progress = progress;
            this.assets = assets;
        }

        public IEnumerable<TerrainBuilderObject> Generate(IArma3MapConfig config, IContext context)
        {
            using var scope = progress.CreateScope(GetType().Name.Replace("Generator",""));

            var polygons = context.GetData<TData>().Polygons;

            var layer = new RadiusPlacedLayer<Composition>(new Vector2(config.SizeInMeters));

            Generate(layer, polygons);

            return layer.SelectMany(item => item.Model.ToTerrainBuilderObjects(item));
        }

        protected abstract void Generate(RadiusPlacedLayer<Composition> layer, List<TerrainPolygon> polygons);
    }
}
