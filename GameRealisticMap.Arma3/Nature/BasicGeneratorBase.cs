using GameRealisticMap.Algorithms;
using GameRealisticMap.Algorithms.Filling;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.Assets.Filling;
using GameRealisticMap.Conditions;
using GameRealisticMap.Geometries;
using GameRealisticMap.Nature;
using GameRealisticMap.Reporting;

namespace GameRealisticMap.Arma3.Nature
{
    internal abstract class BasicGeneratorBase<TData> : GeneratorBase<TData> where TData : class, IPolygonTerrainData
    {
        public BasicGeneratorBase(IProgressSystem progress, IArma3RegionAssets assets)
            : base(progress, assets)
        {

        }

        protected abstract BasicCollectionId Id { get; }

        protected override void Generate(RadiusPlacedLayer<Composition> layer, List<TerrainPolygon> polygons, IConditionEvaluator evaluator)
        {
            var main = new FillAreaBasic<Composition>(progress, assets.GetBasicCollections(Id));
            main.FillPolygons(layer, polygons, evaluator);
        }
    }
}
