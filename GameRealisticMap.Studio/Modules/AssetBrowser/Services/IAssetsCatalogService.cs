﻿using System.Collections.Generic;
using System.Threading.Tasks;

namespace GameRealisticMap.Studio.Modules.AssetBrowser.Services
{
    internal interface IAssetsCatalogService
    {
        Task<List<AssetCatalogItem>> LoadFrom(string fileName);

        Task SaveTo(string fileName, List<AssetCatalogItem> items);

        Task<List<AssetCatalogItem>> ImportItems(IEnumerable<string> paths, string modId = "");
    }
}