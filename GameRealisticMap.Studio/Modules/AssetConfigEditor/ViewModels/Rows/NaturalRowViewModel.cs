using System.Collections.Generic;
using System.Threading.Tasks;
using GameRealisticMap.Algorithms.Rows;
using GameRealisticMap.Algorithms;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.Assets.Detection;
using GameRealisticMap.Arma3.Assets.Rows;
using GameRealisticMap.Studio.Modules.AssetConfigEditor.ViewModels.Fences;
using GameRealisticMap.Studio.Modules.AssetConfigEditor.ViewModels.Generic;
using GameRealisticMap.Studio.Modules.Explorer.ViewModels;
using System;
using GameRealisticMap.Algorithms.Following;
using System.Linq;
using GameRealisticMap.Geometries;
using GameRealisticMap.Studio.Modules.AssetConfigEditor.Controls;
using GameRealisticMap.Arma3.TerrainBuilder;

namespace GameRealisticMap.Studio.Modules.AssetConfigEditor.ViewModels.Rows
{
    internal class NaturalRowViewModel : PathObjectsViewModelBase<NaturalRowType, RowDefinition>
    {
        private double rowSpacing;

        public FencesStraightViewModel Segments { get; }

        public ObjectsViewModel Objects { get; }

        public PreviewViewModel PlacementPreview { get; } = new PreviewViewModel();

        public NaturalRowViewModel(NaturalRowType id, RowDefinition? definition, AssetConfigEditorViewModel parent)
            : base(id, definition, parent)
        {
            label = definition?.Label ?? string.Empty;
            rowSpacing = definition?.RowSpacing ?? 2;
            useObjects = (definition?.Straights?.Count ?? 0) == 0;
            Segments = new FencesStraightViewModel(definition?.Straights, this);
            Objects = new ObjectsViewModel(definition?.Objects, this);
            SegmentsItems = new() { Segments };
            ObjectsItems = new() { Objects };
            rowSpacing = definition?.RowSpacing ?? 2;
        }

        public override List<IExplorerTreeItem> SegmentsItems { get; }

        public override List<IExplorerTreeItem> ObjectsItems { get; }

        public bool IsEmpty { get { return UseObjects ? Objects.Items.Count == 0 : Segments.Items.Count == 0; } }

        public double RowSpacing
        {
            get { return rowSpacing; }
            set { rowSpacing = value; NotifyOfPropertyChange(); }
        }

        public bool UseRowSpacing => FillId != NaturalRowType.TreeRow;

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

        public Task GeneratePreview()
        {
            PlacementPreview.GeneratePreview(DoGeneratePreview);
            return Task.CompletedTask;
        }

        private List<TerrainBuilderObject> DoGeneratePreview()
        {
            var layer = new List<PlacedModel<Composition>>();
            var rnd = new Random(0);
            if (UseRowSpacing)
            {
                FillAreaWithRows.Fill(rnd, ToDefinition(), layer, TerrainPolygon.FromRectangle(new TerrainPoint(0, 0), new TerrainPoint((float)PreviewGrid.SizeInMeters, (float)PreviewGrid.SizeInMeters)));
            }
            else
            {
                var h = (float)PreviewGrid.SizeInMeters / 2;
                FollowPathWithObjects.PlaceOnPathNotFitted(rnd, ToDefinition(), layer, new[] { new TerrainPoint(h, 0), new TerrainPoint(h, (float)PreviewGrid.SizeInMeters) });
            }
            return layer.SelectMany(o => o.Model.ToTerrainBuilderObjects(o)).ToList();
        }
    }
}
