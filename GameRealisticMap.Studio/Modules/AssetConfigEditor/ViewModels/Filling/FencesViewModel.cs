using System.Collections.ObjectModel;
using System.Linq;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.Assets.Detection;
using GameRealisticMap.Arma3.TerrainBuilder;
using GameRealisticMap.ManMade.Fences;
using Gemini.Framework;

namespace GameRealisticMap.Studio.Modules.AssetConfigEditor.ViewModels.Filling
{
    internal class FencesViewModel : AssetProbabilityBase<FenceTypeId, FenceDefinition>
    {
        public FencesViewModel(FenceTypeId id, FenceDefinition? definition, AssetConfigEditorViewModel parent)
            : base(id, definition, parent)
        {
            if (definition != null)
            {
                Items = new ObservableCollection<FenceItem>(definition.Straights.Select(d => new FenceItem(d)));
            }
            else
            {
                Items = new();
            }
            RemoveItem = new RelayCommand(item => Items.Remove((FenceItem)item));
        }

        public ObservableCollection<FenceItem> Items { get; }

        public string DensityText => string.Empty; // To avoid binding error

        public RelayCommand RemoveItem { get; }

        public override FenceDefinition ToDefinition()
        {
            return new FenceDefinition(Probability, Items.Select(i => i.ToDefinition()).ToList());
        }

        public override void AddComposition(Composition composition, ObjectPlacementDetectedInfos detected)
        {
            Items.Add(new FenceItem(new StraightSegmentDefinition(composition.Translate(-detected.GeneralRadius.Center), detected.GeneralRadius.Radius)));
        }
    }
}
