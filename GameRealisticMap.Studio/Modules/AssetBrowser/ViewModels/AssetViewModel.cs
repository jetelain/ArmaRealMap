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
        public Uri Preview 
        { 
            get { return preview; } 
            set { preview = value; NotifyOfPropertyChange(); } 
        }

        public string ModId => item.ModId;

        public AssetCatalogCategory Category 
        { 
            get { return item.Category; } 
            set { item.Category = value; NotifyOfPropertyChange(); }
        }

        public string GroundSizeText => $"{item.Size.X:0.0} x {item.Size.Z:0.0} m";

        public string HeightText => $"{item.Height:0.0} m";

        public AssetCatalogItem Item => item;
    }
}