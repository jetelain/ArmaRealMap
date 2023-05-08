using GameRealisticMap.Algorithms.Filling;
using GameRealisticMap.Algorithms;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.Assets.Filling;
using GameRealisticMap.Geometries;
using GameRealisticMap.Nature.RockAreas;
using GameRealisticMap.Reporting;

namespace GameRealisticMap.Arma3.Nature.RockAreas
{
    internal class RocksGenerator : BasicGeneratorBase<RocksData>
    {
        public RocksGenerator(IProgressSystem progress, IArma3RegionAssets assets)
            : base(progress, assets)
        {
        }

        protected override BasicCollectionId Id => BasicCollectionId.Rocks;

        protected override void Generate(RadiusPlacedLayer<Composition> layer, List<TerrainPolygon> polygons)
        {
            base.Generate(layer, polygons);

            var additional = new FillAreaBasic<Composition>(progress, assets.GetBasicCollections(BasicCollectionId.RocksAdditional));
            additional.FillPolygons(layer, polygons);
        }
    }
}
