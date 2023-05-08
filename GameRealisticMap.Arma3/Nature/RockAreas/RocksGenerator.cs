using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Nature.RockAreas;
using GameRealisticMap.Reporting;

namespace GameRealisticMap.Arma3.Nature.RockAreas
{
    internal class RocksGenerator : BasicGeneratorBase<RocksData>
    {
        public RocksGenerator(IProgressSystem progress, IArma3RegionAssets assets)
            : base(progress, assets)
        {
        }

        protected override BasicCollectionId Id => BasicCollectionId.Rocks;
    }
}
