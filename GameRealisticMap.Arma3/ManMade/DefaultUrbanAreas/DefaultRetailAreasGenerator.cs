﻿using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.Assets.Filling;
using GameRealisticMap.ManMade.DefaultUrbanAreas;

namespace GameRealisticMap.Arma3.Nature.DefaultUrbanAreas
{
    internal class DefaultRetailAreasGenerator : BasicGeneratorBase<DefaultRetailAreaData>
    {
        public DefaultRetailAreasGenerator(IArma3RegionAssets assets)
            : base(assets)
        {
        }

        protected override bool ShouldGenerate => assets.GetBasicCollections(Id).Count > 0;

        protected override BasicCollectionId Id => BasicCollectionId.DefaultRetailAreas;
    }
}
