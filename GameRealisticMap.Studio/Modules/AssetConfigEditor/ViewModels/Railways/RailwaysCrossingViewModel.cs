using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.Assets.Detection;
using GameRealisticMap.Studio.Modules.AssetConfigEditor.ViewModels.Filling;
using GameRealisticMap.Studio.Modules.Explorer.ViewModels;
using GameRealisticMap.Studio.UndoRedo;
using Gemini.Framework;

namespace GameRealisticMap.Studio.Modules.AssetConfigEditor.ViewModels.Railways
{
    internal class RailwaysCrossingViewModel : AssetBase<List<RailwayCrossingDefinition>>, IExplorerTreeItemCounter
    {
        public RailwaysCrossingViewModel(List<RailwayCrossingDefinition>? definition, AssetConfigEditorViewModel parent)
            : base(parent, "RailwaysCrossing")
        {
            if (definition != null)
            {
                Items = new ObservableCollection<RailwayCrossingItem>(definition.Select(d => new RailwayCrossingItem(d)));
            }
            else
            {
                Items = new();
            }
            RemoveItem = new RelayCommand(item => Items.RemoveUndoable(UndoRedoManager, (RailwayCrossingItem)item));
        }

        public ObservableCollection<RailwayCrossingItem> Items { get; }

        public string DensityText => string.Empty; // To avoid binding error

        public RelayCommand RemoveItem { get; }

        public bool IsEmpty { get { return Items.Count == 0; } }

        public override List<RailwayCrossingDefinition> ToDefinition()
        {
            return Items.Select(i => i.ToDefinition()).ToList();
        }

        public override void AddComposition(Composition composition, ObjectPlacementDetectedInfos detected)
        {
            Items.AddUndoable(UndoRedoManager, new RailwayCrossingItem(new RailwayCrossingDefinition(composition, detected.GeneralRadius.Radius, detected.GeneralRadius.Radius/2)));
        }

        public override void Equilibrate()
        {

        }

        public override IEnumerable<string> GetModels()
        {
            return Items.SelectMany(i => i.Composition.Items.Select(i => i.Model.Path));
        }
    }
}
