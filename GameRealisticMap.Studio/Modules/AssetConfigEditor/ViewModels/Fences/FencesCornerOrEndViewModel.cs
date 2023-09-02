using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.Assets.Detection;
using GameRealisticMap.Arma3.Assets.Fences;
using GameRealisticMap.Studio.Modules.CompositionTool.ViewModels;
using GameRealisticMap.Studio.Modules.Explorer.ViewModels;
using GameRealisticMap.Studio.UndoRedo;
using Gemini.Framework;

namespace GameRealisticMap.Studio.Modules.AssetConfigEditor.ViewModels.Fences
{
    internal class FencesCornerOrEndViewModel : PropertyChangedBase, IModelImporterTarget, IExplorerTreeItem, IExplorerTreeItemCounter
    {
        private readonly IDocument _parent;

        public FencesCornerOrEndViewModel(List<FenceCornerOrEndDefinition>? definition, string label, IDocument parent)
        {
            _parent = parent;
            if (definition != null)
            {
                Items = new ObservableCollection<FenceCornerOrEndItem>(definition.Select(d => new FenceCornerOrEndItem(d)));
            }
            else
            {
                Items = new();
            }
            Label = label;
            CompositionImporter = new CompositionImporter(this);
            RemoveItem = new RelayCommand(item => Items.RemoveUndoable(parent.UndoRedoManager, (FenceCornerOrEndItem)item));
        }

        public ObservableCollection<FenceCornerOrEndItem> Items { get; }

        public string Label { get; }

        public CompositionImporter CompositionImporter { get; }

        public RelayCommand RemoveItem { get; }

        public string TreeName => Label;

        public string Icon => "pack://application:,,,/GameRealisticMap.Studio;component/Resources/Icons/Generic.png";

        public IEnumerable<IExplorerTreeItem> Children => Enumerable.Empty<IExplorerTreeItem>();

        public void AddComposition(Composition composition, ObjectPlacementDetectedInfos detected)
        {
            Items.AddUndoable(_parent.UndoRedoManager, new FenceCornerOrEndItem(new FenceCornerOrEndDefinition(composition.Translate(-detected.GeneralRadius.Center), DefinitionHelper.GetNewItemProbility(Items))));
            DefinitionHelper.EquilibrateProbabilities(Items);
        }

        internal List<FenceCornerOrEndDefinition> ToDefinition()
        {
            return Items.Select(i => i.ToDefinition()).ToList();
        }

        public IEnumerable<string> GetModels()
        {
            return Items.SelectMany(i => i.Composition.Items.Select(i => i.Model.Path));
        }
        public Task MakeItemsEquiprobable()
        {
            DefinitionHelper.Equiprobable(Items, _parent.UndoRedoManager);
            return Task.CompletedTask;
        }
    }
}
