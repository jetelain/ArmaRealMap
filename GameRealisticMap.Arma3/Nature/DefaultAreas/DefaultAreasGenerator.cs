using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.Assets.Filling;
using GameRealisticMap.Nature.DefaultAreas;

namespace GameRealisticMap.Arma3.Nature.DefaultAreas
{
    internal class DefaultAreasGenerator : BasicGeneratorBase<DefaultAreasData>
    {
        public DefaultAreasGenerator(IArma3RegionAssets assets)
            : base(assets)
        {
        }

        protected override bool ShouldGenerate => assets.GetBasicCollections(Id).Count > 0;

        protected override BasicCollectionId Id => BasicCollectionId.DefaultAreas;
    }
}
