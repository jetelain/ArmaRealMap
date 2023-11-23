using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;
using GameRealisticMap.Arma3.Edit;
using GameRealisticMap.Studio.Modules.AssetBrowser.Services;
using GameRealisticMap.Studio.Modules.Reporting;
using Gemini.Framework;

namespace GameRealisticMap.Studio.Modules.Arma3WorldEditor.ViewModels.MassEdit
{
    internal class ReplaceViewModel : WindowBase
    {
        private readonly Arma3WorldEditorViewModel worldEditor;

        public ReplaceViewModel(Arma3WorldEditorViewModel worldEditor)
        {
            this.worldEditor = worldEditor;
            ReplaceItems.CollectionChanged += ReplaceItems_CollectionChanged;

            Models = worldEditor.ObjectStatsItems.ToList();
            Models.Sort((a,b) => a.Model.CompareTo(b.Model));

            Task.Run(LoadAllModels);
        }

        private void ReplaceItems_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (ReplaceItem add in e.NewItems)
                {
                    add.Attach(this);
                }
            }
        }

        private async Task LoadAllModels()
        {
            var models = await IoC.Get<IAssetsCatalogService>().GetOrLoad();
            models.Sort((a, b) => a.Path.CompareTo(b.Path));
            AllModels = models;
            NotifyOfPropertyChange(nameof(AllModels));
        }

        public Task Cancel() => TryCloseAsync(false);

        public Task Process()
        {
            var batch = new WrpMassEditBatch();
            batch.Replace.AddRange(ReplaceItems.Select(i => i.ToOperation()));
            if (ProgressToolHelper.Start(new MassEditTask(batch, worldEditor)))
            {
                return TryCloseAsync(false);
            }
            return Task.CompletedTask;
        }

        public Arma3WorldEditorViewModel ParentEditor => worldEditor;

        public ObservableCollection<ReplaceItem> ReplaceItems { get; } = new ObservableCollection<ReplaceItem>();

        public List<ObjectStatsItem> Models { get; }

        public List<AssetCatalogItem> AllModels { get; private set; } = new List<AssetCatalogItem>();
    }
}
