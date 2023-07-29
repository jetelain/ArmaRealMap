using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.Assets.Detection;
using GameRealisticMap.ManMade.Fences;
using GameRealisticMap.Studio.Modules.Explorer.ViewModels;
using GameRealisticMap.Studio.UndoRedo;
using Gemini.Framework;

namespace GameRealisticMap.Studio.Modules.AssetConfigEditor.ViewModels.Filling
{
    internal class FencesViewModel : AssetProbabilityBase<FenceTypeId, FenceDefinition>, IExplorerTreeItemCounter
    {
        private bool isProportionForFullList;

        public FencesViewModel(FenceTypeId id, FenceDefinition? definition, AssetConfigEditorViewModel parent)
            : base(id, definition, parent)
        {
            label = definition?.Label ?? string.Empty;
            isProportionForFullList = definition?.IsProportionForFullList ?? false;
            if (definition != null)
            {
                Items = new ObservableCollection<FenceItem>(definition.Straights.Select(d => new FenceItem(d)));
            }
            else
            {
                Items = new();
            }
            RemoveItem = new RelayCommand(item => Items.RemoveUndoable(UndoRedoManager, (FenceItem)item));
        }

        public ObservableCollection<FenceItem> Items { get; }

        public string DensityText => string.Empty; // To avoid binding error

        public RelayCommand RemoveItem { get; }

        public bool IsEmpty { get { return Items.Count == 0; } }

        public bool IsProportionForFullList
        {
            get { return isProportionForFullList; }
            set { isProportionForFullList = value; NotifyOfPropertyChange(); }
        }

        public bool IsProportionPerSize
        {
            get { return !isProportionForFullList; }
            set { isProportionForFullList = !value; }
        }

        public override FenceDefinition ToDefinition()
        {
            return new FenceDefinition(Probability, Items.Select(i => i.ToDefinition()).ToList(), isProportionForFullList, Label);
        }

        public override void AddComposition(Composition composition, ObjectPlacementDetectedInfos detected)
        {
            Items.AddUndoable(UndoRedoManager, new FenceItem(new FenceSegmentDefinition(composition.Translate(-detected.GeneralRadius.Center), detected.GeneralRadius.Radius)));
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
