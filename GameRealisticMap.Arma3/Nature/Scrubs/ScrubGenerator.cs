using GameRealisticMap.Algorithms;
using GameRealisticMap.Algorithms.Filling;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.Assets.Filling;
using GameRealisticMap.Conditions;
using GameRealisticMap.Geometries;
using GameRealisticMap.Nature.Scrubs;
using Pmad.ProgressTracking;

namespace GameRealisticMap.Arma3.Nature.Scrubs
{
    internal class ScrubGenerator : ClusteredGeneratorBase<ScrubData>
    {
        public ScrubGenerator(IArma3RegionAssets assets)
            : base(assets)
        {
        }

        protected override ClusterCollectionId Id => ClusterCollectionId.Scrub;

        protected override void Generate(RadiusPlacedLayer<Composition> layer, List<TerrainPolygon> polygons, IConditionEvaluator evaluator, IProgressScope scope)
        {
            base.Generate(layer, polygons, evaluator, scope);
            
            var additional = new FillAreaBasic<Composition>(scope, assets.GetBasicCollections(BasicCollectionId.ScrubAdditional));
            additional.FillPolygons(layer, polygons, evaluator);
        }
    }
}
