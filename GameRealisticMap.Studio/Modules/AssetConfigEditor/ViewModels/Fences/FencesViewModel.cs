using System.Collections.Generic;
using System.Linq;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.Assets.Detection;
using GameRealisticMap.Arma3.Assets.Fences;
using GameRealisticMap.ManMade.Fences;
using GameRealisticMap.Studio.Modules.AssetConfigEditor.ViewModels.Filling;
using GameRealisticMap.Studio.Modules.Explorer.ViewModels;

namespace GameRealisticMap.Studio.Modules.AssetConfigEditor.ViewModels.Fences
{
    internal class FencesViewModel : AssetProbabilityBase<FenceTypeId, FenceDefinition>
    {
        private bool useAnySize;
        private bool useObjects;

        public FencesStraightViewModel Straights { get; }

        public FencesCornerOrEndViewModel LeftCorners { get; }

        public FencesCornerOrEndViewModel RightCorners { get; }

        public FencesCornerOrEndViewModel Ends { get; }

        public FenceObjectsViewModel Objects { get; }

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
            Objects = new FenceObjectsViewModel(definition?.Objects, this);
            SegmentsItems = new() { Straights, LeftCorners, RightCorners, Ends };
            ObjectsItems = new() { Objects };
        }

        public List<IExplorerTreeItem> SegmentsItems { get; }

        public List<IExplorerTreeItem> ObjectsItems { get; }

        public IExplorerTreeItem? MainChild => Children.FirstOrDefault(); // Used by "Count" column on main view

        public override IEnumerable<IExplorerTreeItem> Children => useObjects ? ObjectsItems : SegmentsItems;

        public IEnumerable<FencesCornerOrEndViewModel> CornersAndEnds => new[] { LeftCorners, RightCorners, Ends };

        public string DensityText => string.Empty; // To avoid binding error

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

        public bool UseObjects
        {
            get { return useObjects; }
            set 
            {
                if (useObjects != value)
                {
                    useObjects = value; 
                    NotifyOfPropertyChange(); 
                    NotifyOfPropertyChange(nameof(UseSegments));
                    NotifyOfPropertyChange(nameof(Children));
                    NotifyOfPropertyChange(nameof(MainChild));
                }
            }
        }

        public bool UseSegments
        {
            get { return !UseObjects; }
            set { UseObjects = !value; }
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
