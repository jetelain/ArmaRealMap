using GameRealisticMap.Algorithms;
using GameRealisticMap.Algorithms.Filling;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.Assets.Filling;
using GameRealisticMap.Conditions;
using GameRealisticMap.Geometries;
using GameRealisticMap.Nature.Forests;
using Pmad.ProgressTracking;

namespace GameRealisticMap.Arma3.Nature.Forests
{
    public class ForestGenerator : ClusteredGeneratorBase<ForestData>
    {
        public ForestGenerator(IArma3RegionAssets assets)
            : base(assets)
        {
        }

        protected override ClusterCollectionId Id => ClusterCollectionId.Forest;

        protected override void Generate(RadiusPlacedLayer<Composition> layer, List<TerrainPolygon> polygons, IConditionEvaluator evaluator, IProgressScope scope)
        {
            base.Generate(layer, polygons, evaluator, scope);

            var additional = new FillAreaBasic<Composition>(scope, assets.GetBasicCollections(BasicCollectionId.ForestAdditional));
            additional.FillPolygons(layer, polygons, evaluator);
        }
    }
}
