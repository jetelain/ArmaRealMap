using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameRealisticMap.Studio.Modules.AssetBrowser.Services;

namespace GameRealisticMap.Studio.Modules.AssetBrowser.ViewModels
{
    internal class CategoryOption
    {
        public CategoryOption(string name, AssetCatalogCategory? category)
        {
            Name = name;
            Category = category;
        }

        public string Name { get; }

        public AssetCatalogCategory? Category { get; }
    }
}
