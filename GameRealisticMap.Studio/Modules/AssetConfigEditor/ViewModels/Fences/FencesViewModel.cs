using System.Collections.Generic;
using System.Linq;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.Assets.Detection;
using GameRealisticMap.Arma3.Assets.Fences;
using GameRealisticMap.ManMade.Fences;
using GameRealisticMap.Studio.Modules.AssetConfigEditor.ViewModels.Generic;
using GameRealisticMap.Studio.Modules.Explorer.ViewModels;

namespace GameRealisticMap.Studio.Modules.AssetConfigEditor.ViewModels.Fences
{
    internal class FencesViewModel : PathObjectsViewModelBase<FenceTypeId, FenceDefinition>
    {
        private bool useAnySize;

        public FencesStraightViewModel Straights { get; }

        public FencesCornerOrEndViewModel LeftCorners { get; }

        public FencesCornerOrEndViewModel RightCorners { get; }

        public FencesCornerOrEndViewModel Ends { get; }

        public ObjectsViewModel Objects { get; }

        public FencesViewModel(FenceTypeId id, FenceDefinition? definition, AssetConfigEditorViewModel parent)
            : base(id, definition, parent)
        {
            label = definition?.Label ?? string.Empty;
            useAnySize = definition?.UseAnySize ?? false;
            useObjects = (definition?.Objects?.Count ?? 0) != 0;
            Straights = new FencesStraightViewModel(definition?.Straights, this);
            LeftCorners = new FencesCornerOrEndViewModel(definition?.LeftCorners, GameRealisticMap.Studio.Labels.FenceLeftCorners, this);
            RightCorners = new FencesCornerOrEndViewModel(definition?.RightCorners, GameRealisticMap.Studio.Labels.FenceRightCorners, this);
            Ends = new FencesCornerOrEndViewModel(definition?.Ends, GameRealisticMap.Studio.Labels.FenceEnds, this);
            Objects = new ObjectsViewModel(definition?.Objects, this);
            SegmentsItems = new() { Straights, LeftCorners, RightCorners, Ends };
            ObjectsItems = new() { Objects };
        }

        public override List<IExplorerTreeItem> SegmentsItems { get; }

        public override List<IExplorerTreeItem> ObjectsItems { get; }

        public IEnumerable<FencesCornerOrEndViewModel> CornersAndEnds => new[] { LeftCorners, RightCorners, Ends };

        public bool IsEmpty { get { return UseObjects ? Objects.Items.Count == 0 : Straights.Items.Count == 0; } }

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
            if (UseObjects)
            {
                return new FenceDefinition(Probability, null, null, null, null, Objects.ToDefinition(), useAnySize, Label);
            }
            return new FenceDefinition(Probability, Straights.ToDefinition(), LeftCorners.ToDefinition(), RightCorners.ToDefinition(), Ends.ToDefinition(), null, useAnySize, Label);
        }

        public override void AddComposition(Composition composition, ObjectPlacementDetectedInfos detected)
        {
            if (UseObjects)
            {
                Objects.AddComposition(composition, detected);
            }
            else
            {
                Straights.AddComposition(composition, detected);
            }
        }

        public override void Equilibrate()
        {
            if (UseObjects)
            {
                DefinitionHelper.EquilibrateProbabilities(Objects.Items);
            }
        }

        public override IEnumerable<string> GetModels()
        {
            if (UseObjects)
            {
                return Objects.GetModels();
            }
            return Straights.GetModels()
                .Concat(LeftCorners.GetModels())
                .Concat(RightCorners.GetModels())
                .Concat(Ends.GetModels());
        }
    }
}
