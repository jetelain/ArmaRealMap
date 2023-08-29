using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using GameRealisticMap.Arma3.TerrainBuilder;
using GameRealisticMap.Studio.Modules.AssetBrowser.Services;

namespace GameRealisticMap.Studio.Modules.Arma3WorldEditor.ViewModels
{
    internal class ImportLibrary : IModelInfoLibrary
    {
        private readonly List<AssetCatalogItem> assetCatalogItems;
        private readonly IModelInfoLibrary library;

        public ImportLibrary(List<AssetCatalogItem> assetCatalogItems, IModelInfoLibrary library)
        {
            this.assetCatalogItems = assetCatalogItems;
            this.library = library;
        }

        public ModelInfo ResolveByName(string name)
        {
            if (TryResolveByName(name, out var model))
            {
                return model;
            }
            return library.ResolveByName(name);
        }

        public ModelInfo ResolveByPath(string path)
        {
            return library.ResolveByPath(path);
        }

        public bool TryResolveByName(string name, [MaybeNullWhen(false)] out ModelInfo model)
        {
            if (library.TryResolveByName(name, out model))
            {
                return true;
            }
            var searchName = name + ".p3d";
            var entry = assetCatalogItems.Where(i => string.Equals(i.Name, searchName, System.StringComparison.OrdinalIgnoreCase)).ToList();
            if (entry.Count == 1)
            {
                return TryResolveByPath(entry[0].Path, out model);
            }
            return false;
        }

        public bool TryResolveByPath(string path, [MaybeNullWhen(false)] out ModelInfo model)
        {
            return library.TryResolveByPath(path, out model);
        }
    }
}