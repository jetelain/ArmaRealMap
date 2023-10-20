using GameRealisticMap.Algorithms;
using GameRealisticMap.Algorithms.Filling;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.Assets.Filling;
using GameRealisticMap.Conditions;
using GameRealisticMap.Geometries;
using GameRealisticMap.Nature.Scrubs;
using GameRealisticMap.Reporting;

namespace GameRealisticMap.Arma3.Nature.Scrubs
{
    internal class ScrubGenerator : ClusteredGeneratorBase<ScrubData>
    {
        public ScrubGenerator(IProgressSystem progress, IArma3RegionAssets assets)
            : base(progress, assets)
        {
        }

        protected override ClusterCollectionId Id => ClusterCollectionId.Scrub;

        protected override void Generate(RadiusPlacedLayer<Composition> layer, List<TerrainPolygon> polygons, IConditionEvaluator evaluator)
        {
            base.Generate(layer, polygons, evaluator);
            
            var additional = new FillAreaBasic<Composition>(progress, assets.GetBasicCollections(BasicCollectionId.ScrubAdditional));
            additional.FillPolygons(layer, polygons, evaluator);
        }
    }
}
