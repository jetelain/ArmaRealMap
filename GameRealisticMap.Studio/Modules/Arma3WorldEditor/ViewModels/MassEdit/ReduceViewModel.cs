using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Gemini.Framework;

namespace GameRealisticMap.Studio.Modules.Arma3WorldEditor.ViewModels.MassEdit
{
    internal class ReduceViewModel : WindowBase
    {
        private readonly Arma3WorldEditorViewModel worldEditor;

        public ReduceViewModel(Arma3WorldEditorViewModel worldEditor)
        {
            this.worldEditor = worldEditor;
            ReduceItems.CollectionChanged += ReplaceItems_CollectionChanged;
        }

        private void ReplaceItems_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (ReduceItem add in e.NewItems)
                {
                    add.Attach(this);
                }
            }
        }

        public Task Cancel() => TryCloseAsync(false);

        public Task Process()
        {
            return Task.CompletedTask;
        }

        public Arma3WorldEditorViewModel ParentEditor => worldEditor;

        public ObservableCollection<ReduceItem> ReduceItems { get; } = new ObservableCollection<ReduceItem>();

        public List<ObjectStatsItem> Models => worldEditor.ObjectStatsItems;
    }
}
