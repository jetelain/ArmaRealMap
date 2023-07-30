using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Caliburn.Micro;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.Assets.Detection;
using GameRealisticMap.Studio.Modules.CompositionTool.ViewModels;
using GameRealisticMap.Studio.Modules.Explorer.ViewModels;
using GameRealisticMap.Studio.UndoRedo;
using Gemini.Framework;

namespace GameRealisticMap.Studio.Modules.AssetConfigEditor.ViewModels.Fences
{
    internal class FencesStraightViewModel : PropertyChangedBase, IModelImporterTarget, IExplorerTreeItem, IExplorerTreeItemCounter
    {
        private readonly FencesViewModel _parent;

        public FencesStraightViewModel(FenceDefinition? definition, FencesViewModel parent)
        {
            _parent = parent;
            if (definition != null)
            {
                Items = new ObservableCollection<FenceItem>(definition.Straights.Select(d => new FenceItem(d)));
            }
            else
            {
                Items = new();
            }
            CompositionImporter = new CompositionImporter(this);
            RemoveItem = new RelayCommand(item => Items.RemoveUndoable(parent.UndoRedoManager, (FenceItem)item));
        }

        public ObservableCollection<FenceItem> Items { get; }

        public string Label => "Straigth segments";

        public CompositionImporter CompositionImporter { get; }

        public RelayCommand RemoveItem { get; }

        public string TreeName => Label;

        public string Icon => "pack://application:,,,/GameRealisticMap.Studio;component/Resources/Icons/Generic.png";

        public IEnumerable<IExplorerTreeItem> Children => Enumerable.Empty<IExplorerTreeItem>();

        public void AddComposition(Composition composition, ObjectPlacementDetectedInfos detected)
        {
            Items.AddUndoable(_parent.UndoRedoManager, new FenceItem(new FenceSegmentDefinition(composition.Translate(-detected.GeneralRadius.Center), detected.GeneralRadius.Radius)));
        }

        internal List<FenceSegmentDefinition> ToDefinition()
        {
            return Items.Select(i => i.ToDefinition()).ToList();
        }
        public IEnumerable<string> GetModels()
        {
            return Items.SelectMany(i => i.Composition.Items.Select(i => i.Model.Path));
        }
    }
}
