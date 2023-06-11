using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.Assets.Filling;
using GameRealisticMap.Nature.Watercourses;
using GameRealisticMap.Reporting;

namespace GameRealisticMap.Arma3.Nature.Watercourses
{
    internal class WatercourseGenerator : ClusteredGeneratorBase<WatercoursesData>
    {
        public WatercourseGenerator(IProgressSystem progress, IArma3RegionAssets assets)
            : base(progress, assets)
        {
        }

        protected override ClusterCollectionId Id => ClusterCollectionId.Watercourse;
    }
}
