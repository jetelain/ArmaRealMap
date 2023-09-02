using System.Collections.Generic;
using System.Linq;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.Assets.Detection;
using GameRealisticMap.Studio.Modules.AssetConfigEditor.ViewModels.Fences;
using GameRealisticMap.Studio.Modules.AssetConfigEditor.ViewModels.Filling;
using GameRealisticMap.Studio.Modules.Explorer.ViewModels;

namespace GameRealisticMap.Studio.Modules.AssetConfigEditor.ViewModels.Roads
{
    internal class SidewalkViewModel : AssetProbabilityBase<SidewalkId, SidewalksDefinition>
    {
        private bool useAnySize;

        public FencesStraightViewModel Straights { get; }

        public FencesCornerOrEndViewModel LeftCorners { get; }

        public FencesCornerOrEndViewModel RightCorners { get; }

        public FencesCornerOrEndViewModel Ends { get; }

        public SidewalkViewModel(SidewalksDefinition? definition, AssetConfigEditorViewModel parent)
            : base(SidewalkId.Sidewalks, definition, parent)
        {
            label = definition?.Label ?? string.Empty;
            useAnySize = definition?.UseAnySize ?? false;
            Straights = new FencesStraightViewModel(definition?.Straights, this);
            LeftCorners = new FencesCornerOrEndViewModel(definition?.LeftCorners, Labels.FenceLeftCorners, this);
            RightCorners = new FencesCornerOrEndViewModel(definition?.RightCorners, Labels.FenceRightCorners, this);
            Ends = new FencesCornerOrEndViewModel(definition?.Ends, Labels.FenceEnds, this);
            Children = new IExplorerTreeItem[] { Straights, LeftCorners, RightCorners, Ends };
        }

        public override IEnumerable<IExplorerTreeItem> Children { get; }

        public IEnumerable<FencesCornerOrEndViewModel> CornersAndEnds => new[] { LeftCorners, RightCorners, Ends };

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

        public override SidewalksDefinition ToDefinition()
        {
            return new SidewalksDefinition(Probability, Straights.ToDefinition(), LeftCorners.ToDefinition(), RightCorners.ToDefinition(), Ends.ToDefinition(), useAnySize, Label);
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
