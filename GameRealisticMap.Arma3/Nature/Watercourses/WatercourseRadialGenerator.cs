using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.Assets.Filling;
using GameRealisticMap.Nature.Watercourses;

namespace GameRealisticMap.Arma3.Nature.Watercourses
{
    internal class WatercourseRadialGenerator : ClusteredGeneratorBase<WatercourseRadialData>
    {
        public WatercourseRadialGenerator(IArma3RegionAssets assets)
            : base(assets)
        {
        }

        protected override ClusterCollectionId Id => ClusterCollectionId.WatercourseRadial;
    }
}
