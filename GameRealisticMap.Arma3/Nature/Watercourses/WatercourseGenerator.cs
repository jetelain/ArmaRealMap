using GameRealisticMap.Algorithms.Filling;
using GameRealisticMap.Algorithms;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.Assets.Filling;
using GameRealisticMap.Geometries;
using GameRealisticMap.Nature.Watercourses;
using GameRealisticMap.Reporting;
using GameRealisticMap.Conditions;

namespace GameRealisticMap.Arma3.Nature.Watercourses
{
    internal class WatercourseGenerator : ClusteredGeneratorBase<WatercoursesData>
    {
        public WatercourseGenerator(IProgressSystem progress, IArma3RegionAssets assets)
            : base(progress, assets)
        {
        }

        protected override ClusterCollectionId Id => ClusterCollectionId.Watercourse;

        protected override void Generate(RadiusPlacedLayer<Composition> layer, List<TerrainPolygon> polygons, IConditionEvaluator evaluator)
        {
            base.Generate(layer, polygons, evaluator);

            var additional = new FillAreaBasic<Composition>(progress, assets.GetBasicCollections(BasicCollectionId.WatercourseAdditional));
            additional.FillPolygons(layer, polygons, evaluator);
        }
    }
}
