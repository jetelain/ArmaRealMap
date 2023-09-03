using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.Assets.Filling;
using GameRealisticMap.ManMade.DefaultUrbanAreas;
using GameRealisticMap.Reporting;

namespace GameRealisticMap.Arma3.Nature.DefaultUrbanAreas
{
    internal class DefaultResidentialAreasGenerator : BasicGeneratorBase<DefaultCommercialAreaData>
    {
        public DefaultResidentialAreasGenerator(IProgressSystem progress, IArma3RegionAssets assets)
            : base(progress, assets)
        {
        }

        protected override bool ShouldGenerate => assets.GetBasicCollections(Id).Count > 0;

        protected override BasicCollectionId Id => BasicCollectionId.DefaultResidentialAreas;
    }
}
