using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Data;
using Caliburn.Micro;
using GameRealisticMap.Studio.Modules.AssetBrowser.ViewModels;
using GameRealisticMap.Studio.Modules.Reporting;
using Gemini.Framework;

namespace GameRealisticMap.Studio.Modules.Arma3WorldEditor.ViewModels.MassEdit
{
    internal class ReplaceMaterialViewModel : WindowBase
    {
        private readonly MaterialItem materialItem;
        private GdtDetailViewModel? _libraryItem;
        private readonly Arma3WorldEditorViewModel parent;

        public CollectionViewSource LibraryItemsViewSource { get; }

        public ReplaceMaterialViewModel(MaterialItem materialItem, Arma3WorldEditorViewModel parent)
        {
            this.materialItem = materialItem;
            this._libraryItem = materialItem.LibraryItem;
            this.parent = parent;

            LibraryItemsViewSource = new CollectionViewSource();
            LibraryItemsViewSource.Source = IoC.Get<GdtBrowserViewModel>().AllItems;
            LibraryItemsViewSource.SortDescriptions.Add(new SortDescription(nameof(GdtDetailViewModel.DisplayName), ListSortDirection.Ascending));
        }

        public GdtDetailViewModel? LibraryItem
        {
            get { return _libraryItem; }
            set
            {
                Set(ref _libraryItem, value);
            }
        }

        public string CurrentColorTexture => materialItem.ColorTexture;

        public Task Cancel() => TryCloseAsync(false);

        public Task Process()
        {
            if (_libraryItem != null)
            {
                var config = parent.GetConfig();
                var material = _libraryItem.ToMaterial();
                if (ProgressToolHelper.Start(new ReplaceMaterialTask(parent.World!, parent.ProjectDrive, config, parent.Materials, CurrentColorTexture, material)))
                {
                    materialItem.LibraryItem = _libraryItem;
                    materialItem.ColorTexture = material.GetColorTexturePath(config);
                    return TryCloseAsync(false);
                }
            }
            return Task.CompletedTask;
        }

    }
}
