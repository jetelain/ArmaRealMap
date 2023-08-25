using GameRealisticMap.Algorithms;
using GameRealisticMap.Algorithms.Filling;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.Assets.Filling;
using GameRealisticMap.Geometries;
using GameRealisticMap.Nature;
using GameRealisticMap.Reporting;

namespace GameRealisticMap.Arma3.Nature
{
    public abstract class ClusteredGeneratorBase<TData> : GeneratorBase<TData> where TData : class, IBasicTerrainData
    {
        public ClusteredGeneratorBase(IProgressSystem progress, IArma3RegionAssets assets)
            : base(progress, assets)
        {

        }

        protected abstract ClusterCollectionId Id { get; }

        protected override void Generate(RadiusPlacedLayer<Composition> layer, List<TerrainPolygon> polygons)
        {
            var main = new FillAreaLocalClusters<Composition>(progress, assets.GetClusterCollections(Id));
            main.FillPolygons(layer, polygons);
        }
    }
}
