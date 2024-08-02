using GameRealisticMap.Algorithms;
using GameRealisticMap.Algorithms.Filling;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.Assets.Filling;
using GameRealisticMap.Conditions;
using GameRealisticMap.Geometries;
using GameRealisticMap.Nature.RockAreas;
using Pmad.ProgressTracking;

namespace GameRealisticMap.Arma3.Nature.RockAreas
{
    internal class RocksGenerator : BasicGeneratorBase<RocksData>
    {
        public RocksGenerator(IArma3RegionAssets assets)
            : base(assets)
        {
        }

        protected override BasicCollectionId Id => BasicCollectionId.Rocks;

        protected override void Generate(RadiusPlacedLayer<Composition> layer, List<TerrainPolygon> polygons, IConditionEvaluator evaluator, IProgressScope scope)
        {
            base.Generate(layer, polygons, evaluator, scope);

            var additional = new FillAreaBasic<Composition>(scope, assets.GetBasicCollections(BasicCollectionId.RocksAdditional));
            additional.FillPolygons(layer, polygons, evaluator);
        }
    }
}
