using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.Assets.Filling;
using GameRealisticMap.Nature.RockAreas;
using GameRealisticMap.Reporting;

namespace GameRealisticMap.Arma3.Nature.RockAreas
{
    internal class ScreeGenerator : BasicGeneratorBase<ScreeData>
    {
        public ScreeGenerator(IArma3RegionAssets assets)
            : base(assets)
        {
        }

        protected override BasicCollectionId Id => BasicCollectionId.Scree;
    }
}
