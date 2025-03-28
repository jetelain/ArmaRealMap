﻿using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.Assets.Filling;
using GameRealisticMap.Nature.Forests;

namespace GameRealisticMap.Arma3.Nature.Forests
{
    internal class ForestRadialGenerator : ClusteredGeneratorBase<ForestRadialData>
    {
        public ForestRadialGenerator(IArma3RegionAssets assets)
            : base(assets)
        {
        }

        protected override ClusterCollectionId Id => ClusterCollectionId.ForestRadial;
    }
}
