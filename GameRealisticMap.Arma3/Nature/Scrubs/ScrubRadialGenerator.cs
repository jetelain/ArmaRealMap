using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.Assets.Filling;
using GameRealisticMap.Nature.Scrubs;

namespace GameRealisticMap.Arma3.Nature.Scrubs
{
    internal class ScrubRadialGenerator : ClusteredGeneratorBase<ScrubRadialData>
    {
        public ScrubRadialGenerator(IArma3RegionAssets assets)
            : base(assets)
        {
        }

        protected override ClusterCollectionId Id => ClusterCollectionId.ScrubRadial;
    }
}
