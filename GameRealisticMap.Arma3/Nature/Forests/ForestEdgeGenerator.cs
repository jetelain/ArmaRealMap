using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.Assets.Filling;
using GameRealisticMap.Nature.Forests;
using GameRealisticMap.Reporting;

namespace GameRealisticMap.Arma3.Nature.Forests
{
    internal class ForestEdgeGenerator : ClusteredGeneratorBase<ForestEdgeData>
    {
        public ForestEdgeGenerator(IProgressSystem progress, IArma3RegionAssets assets)
            : base(progress, assets)
        {
        }

        protected override ClusterCollectionId Id => ClusterCollectionId.ForestEdge;
    }
}
