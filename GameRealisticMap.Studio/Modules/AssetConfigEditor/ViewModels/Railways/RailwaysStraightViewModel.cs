using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.Assets.Detection;
using GameRealisticMap.Studio.Modules.Explorer.ViewModels;
using GameRealisticMap.Studio.UndoRedo;
using Gemini.Framework;

namespace GameRealisticMap.Studio.Modules.AssetConfigEditor.ViewModels.Railways
{
    internal class RailwaysStraightViewModel : AssetBase<List<StraightSegmentDefinition>>, IExplorerTreeItemCounter
    {
        public RailwaysStraightViewModel(List<StraightSegmentDefinition>? definition, AssetConfigEditorViewModel parent)
            : base(parent, "RailwaysStraight")
        {
            if (definition != null)
            {
                Items = new ObservableCollection<RailwayStraightItem>(definition.Select(d => new RailwayStraightItem(d)));
            }
            else
            {
                Items = new();
            }
            RemoveItem = new RelayCommand(item => Items.RemoveUndoable(UndoRedoManager, (RailwayStraightItem)item));
        }

        public ObservableCollection<RailwayStraightItem> Items { get; }

        public string DensityText => string.Empty; // To avoid binding error

        public RelayCommand RemoveItem { get; }

        public bool IsEmpty { get { return Items.Count == 0; } }

        public override List<StraightSegmentDefinition> ToDefinition()
        {
            return Items.Select(i => i.ToDefinition()).ToList();
        }

        public override void AddComposition(Composition composition, ObjectPlacementDetectedInfos detected)
        {
            Items.AddUndoable(UndoRedoManager, new RailwayStraightItem(new StraightSegmentDefinition(composition, detected.GeneralRadius.Radius)));
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
