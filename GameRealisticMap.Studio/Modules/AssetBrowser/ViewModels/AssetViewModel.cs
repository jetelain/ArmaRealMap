using System;
using Caliburn.Micro;
using GameRealisticMap.Studio.Modules.AssetBrowser.Services;

namespace GameRealisticMap.Studio.Modules.AssetBrowser.ViewModels
{
    public class AssetViewModel : PropertyChangedBase
    {
        private readonly AssetCatalogItem item;

        internal AssetViewModel(AssetCatalogItem item, Uri preview)
        {
            this.item = item;
            this.preview = preview;
        }

        public string Name=> item.Name;

        public string Path => item.Path;

        private Uri preview;
        public Uri Preview { get { return preview; } set { preview = value; NotifyOfPropertyChange(); } }
    }
}