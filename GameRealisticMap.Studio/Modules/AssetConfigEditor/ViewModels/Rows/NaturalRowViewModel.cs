using System.Collections.Generic;
using System.Linq;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.Assets.Detection;
using GameRealisticMap.Arma3.Assets.Rows;
using GameRealisticMap.Studio.Modules.AssetConfigEditor.ViewModels.Fences;
using GameRealisticMap.Studio.Modules.AssetConfigEditor.ViewModels.Filling;
using GameRealisticMap.Studio.Modules.AssetConfigEditor.ViewModels.Generic;
using GameRealisticMap.Studio.Modules.Explorer.ViewModels;

namespace GameRealisticMap.Studio.Modules.AssetConfigEditor.ViewModels.Rows
{
    internal class NaturalRowViewModel : AssetProbabilityBase<NaturalRowType, RowDefinition>
    {
        private double rowSpacing;
        private bool useObjects;

        public FencesStraightViewModel Segments { get; }

        public ObjectsViewModel Objects { get; }

        public NaturalRowViewModel(NaturalRowType id, RowDefinition? definition, AssetConfigEditorViewModel parent)
            : base(id, definition, parent)
        {
            label = definition?.Label ?? string.Empty;
            rowSpacing = definition?.RowSpacing ?? 2;
            useObjects = (definition?.Segments?.Count ?? 0) == 0;
            Segments = new FencesStraightViewModel(definition?.Segments, this);
            Objects = new ObjectsViewModel(definition?.Objects, this);
            SegmentsItems = new() { Segments };
            ObjectsItems = new() { Objects };
            rowSpacing = definition?.RowSpacing ?? 2;
        }

        public List<IExplorerTreeItem> SegmentsItems { get; }

        public List<IExplorerTreeItem> ObjectsItems { get; }

        public IExplorerTreeItem? MainChild => Children.FirstOrDefault(); // Used by "Count" column on main view

        public override IEnumerable<IExplorerTreeItem> Children => useObjects ? ObjectsItems : SegmentsItems;

        public string DensityText => string.Empty; // To avoid binding error

        public bool IsEmpty { get { return UseObjects ? Objects.Items.Count == 0 : Segments.Items.Count == 0; } }

        public double RowSpacing
        {
            get { return rowSpacing; }
            set { rowSpacing = value; NotifyOfPropertyChange(); }
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

        public override RowDefinition ToDefinition()
        {
            if (UseObjects)
            {
                return new RowDefinition(Probability, RowSpacing, null, Objects.ToDefinition(), Label);
            }
            return new RowDefinition(Probability, RowSpacing, Segments.ToDefinition(), null, Label);
        }

        public override void AddComposition(Composition composition, ObjectPlacementDetectedInfos detected)
        {
            if (UseObjects)
            {
                Objects.AddComposition(composition, detected);
            }
            else
            {
                Segments.AddComposition(composition, detected);
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
            return Segments.GetModels();
        }
    }
}
