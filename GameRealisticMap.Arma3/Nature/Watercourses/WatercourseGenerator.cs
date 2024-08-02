using GameRealisticMap.Algorithms;
using GameRealisticMap.Algorithms.Filling;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.Assets.Filling;
using GameRealisticMap.Conditions;
using GameRealisticMap.Geometries;
using GameRealisticMap.Nature.Watercourses;
using Pmad.ProgressTracking;

namespace GameRealisticMap.Arma3.Nature.Watercourses
{
    internal class WatercourseGenerator : ClusteredGeneratorBase<WatercoursesData>
    {
        public WatercourseGenerator(IArma3RegionAssets assets)
            : base(assets)
        {
        }

        protected override ClusterCollectionId Id => ClusterCollectionId.Watercourse;

        protected override void Generate(RadiusPlacedLayer<Composition> layer, List<TerrainPolygon> polygons, IConditionEvaluator evaluator, IProgressScope scope)
        {
            base.Generate(layer, polygons, evaluator, scope);

            var additional = new FillAreaBasic<Composition>(scope, assets.GetBasicCollections(BasicCollectionId.WatercourseAdditional));
            additional.FillPolygons(layer, polygons, evaluator);
        }
    }
}
