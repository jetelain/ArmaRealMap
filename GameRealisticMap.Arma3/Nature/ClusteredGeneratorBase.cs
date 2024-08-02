using GameRealisticMap.Algorithms;
using GameRealisticMap.Algorithms.Filling;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.Assets.Filling;
using GameRealisticMap.Conditions;
using GameRealisticMap.Geometries;
using GameRealisticMap.Nature;
using Pmad.ProgressTracking;

namespace GameRealisticMap.Arma3.Nature
{
    public abstract class ClusteredGeneratorBase<TData> : GeneratorBase<TData> where TData : class, IBasicTerrainData
    {
        public ClusteredGeneratorBase(IArma3RegionAssets assets)
            : base(assets)
        {

        }

        protected abstract ClusterCollectionId Id { get; }

        protected override void Generate(RadiusPlacedLayer<Composition> layer, List<TerrainPolygon> polygons, IConditionEvaluator evaluator, IProgressScope scope)
        {
            var main = new FillAreaLocalClusters<Composition>(scope, assets.GetClusterCollections(Id));
            main.FillPolygons(layer, polygons, evaluator);
        }
    }
}
