using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.Assets.Filling;
using GameRealisticMap.Nature.Scrubs;
using GameRealisticMap.Reporting;

namespace GameRealisticMap.Arma3.Nature.Scrubs
{
    internal class ScrubRadialGenerator : ClusteredGeneratorBase<ScrubRadialData>
    {
        public ScrubRadialGenerator(IProgressSystem progress, IArma3RegionAssets assets)
            : base(progress, assets)
        {
        }

        protected override ClusterCollectionId Id => ClusterCollectionId.ScrubRadial;
    }
}
