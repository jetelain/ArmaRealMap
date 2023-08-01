using System.Collections.Generic;
using System.Linq;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.Assets.Detection;
using GameRealisticMap.ManMade.Fences;
using GameRealisticMap.Studio.Modules.AssetConfigEditor.ViewModels.Filling;
using GameRealisticMap.Studio.Modules.Explorer.ViewModels;

namespace GameRealisticMap.Studio.Modules.AssetConfigEditor.ViewModels.Fences
{
    internal class FencesViewModel : AssetProbabilityBase<FenceTypeId, FenceDefinition>
    {
        private bool useAnySize;

        public FencesStraightViewModel Straights { get; }

        public FencesCornerOrEndViewModel LeftCorners { get; }

        public FencesCornerOrEndViewModel RightCorners { get; }

        public FencesCornerOrEndViewModel Ends { get; }

        public FencesViewModel(FenceTypeId id, FenceDefinition? definition, AssetConfigEditorViewModel parent)
            : base(id, definition, parent)
        {
            label = definition?.Label ?? string.Empty;
            useAnySize = definition?.UseAnySize ?? false;
            Straights = new FencesStraightViewModel(definition?.Straights, this);
            LeftCorners = new FencesCornerOrEndViewModel(definition?.LeftCorners, "Left corners", this);
            RightCorners = new FencesCornerOrEndViewModel(definition?.RightCorners, "Right corners", this);
            Ends = new FencesCornerOrEndViewModel(definition?.Ends, "Ends", this);
            Items = new() { Straights, LeftCorners, RightCorners, Ends };
        }

        public List<IExplorerTreeItem> Items { get; }

        public override IEnumerable<IExplorerTreeItem> Children => Items;

        public IEnumerable<FencesCornerOrEndViewModel> CornersAndEnds => new[] { LeftCorners, RightCorners, Ends };

        public string DensityText => string.Empty; // To avoid binding error

        public bool IsEmpty { get { return Straights.Items.Count == 0; } }

        public bool UseAnySize
        {
            get { return useAnySize; }
            set { useAnySize = value; NotifyOfPropertyChange(); }
        }

        public bool UseLargestFirst
        {
            get { return !useAnySize; }
            set { useAnySize = !value; }
        }

        public override FenceDefinition ToDefinition()
        {
            return new FenceDefinition(Probability, Straights.ToDefinition(), LeftCorners.ToDefinition(), RightCorners.ToDefinition(), Ends.ToDefinition(), useAnySize, Label);
        }

        public override void AddComposition(Composition composition, ObjectPlacementDetectedInfos detected)
        {
            Straights.AddComposition(composition, detected);
        }

        public override void Equilibrate()
        {

        }

        public override IEnumerable<string> GetModels()
        {
            return Straights.GetModels()
                .Concat(LeftCorners.GetModels())
                .Concat(RightCorners.GetModels())
                .Concat(Ends.GetModels());
        }
    }
}
