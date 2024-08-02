using GameRealisticMap.Algorithms;
using GameRealisticMap.Algorithms.Filling;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.Assets.Filling;
using GameRealisticMap.Conditions;
using GameRealisticMap.Geometries;
using GameRealisticMap.Nature;
using GameRealisticMap.Reporting;
using Pmad.ProgressTracking;

namespace GameRealisticMap.Arma3.Nature
{
    internal abstract class BasicGeneratorBase<TData> : GeneratorBase<TData> where TData : class, IPolygonTerrainData
    {
        public BasicGeneratorBase(IArma3RegionAssets assets)
            : base(assets)
        {

        }

        protected abstract BasicCollectionId Id { get; }

        protected override void Generate(RadiusPlacedLayer<Composition> layer, List<TerrainPolygon> polygons, IConditionEvaluator evaluator, IProgressScope scope)
        {
            var main = new FillAreaBasic<Composition>(scope, assets.GetBasicCollections(Id));
            main.FillPolygons(layer, polygons, evaluator);
        }
    }
}
