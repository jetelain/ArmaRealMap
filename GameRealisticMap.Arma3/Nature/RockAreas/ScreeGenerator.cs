using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.Assets.Filling;
using GameRealisticMap.Nature.RockAreas;
using GameRealisticMap.Reporting;

namespace GameRealisticMap.Arma3.Nature.RockAreas
{
    internal class ScreeGenerator : BasicGeneratorBase<ScreeData>
    {
        public ScreeGenerator(IProgressSystem progress, IArma3RegionAssets assets)
            : base(progress, assets)
        {
        }

        protected override BasicCollectionId Id => BasicCollectionId.Scree;
    }
}
