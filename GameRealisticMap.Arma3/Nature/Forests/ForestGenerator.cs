using GameRealisticMap.Algorithms;
using GameRealisticMap.Algorithms.Filling;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.Assets.Filling;
using GameRealisticMap.Conditions;
using GameRealisticMap.Geometries;
using GameRealisticMap.Nature.Forests;
using GameRealisticMap.Reporting;

namespace GameRealisticMap.Arma3.Nature.Forests
{
    public class ForestGenerator : ClusteredGeneratorBase<ForestData>
    {
        public ForestGenerator(IProgressSystem progress, IArma3RegionAssets assets)
            : base(progress,assets)
        {
        }

        protected override ClusterCollectionId Id => ClusterCollectionId.Forest;

        protected override void Generate(RadiusPlacedLayer<Composition> layer, List<TerrainPolygon> polygons, IConditionEvaluator evaluator)
        {
            base.Generate(layer, polygons, evaluator);

            var additional = new FillAreaBasic<Composition>(progress, assets.GetBasicCollections(BasicCollectionId.ForestAdditional));
            additional.FillPolygons(layer, polygons, evaluator);
        }
    }
}
