using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using GameRealisticMap.Algorithms;
using GameRealisticMap.Algorithms.Filling;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.Assets.Detection;
using GameRealisticMap.Arma3.Assets.Filling;
using GameRealisticMap.Arma3.TerrainBuilder;
using GameRealisticMap.Nature.Forests;
using GameRealisticMap.Nature.Scrubs;
using GameRealisticMap.Nature.Watercourses;
using GameRealisticMap.Reporting;
using GameRealisticMap.Studio.Modules.AssetConfigEditor.Controls;
using GameRealisticMap.Studio.Modules.Explorer.ViewModels;
using GameRealisticMap.Studio.UndoRedo;
using Gemini.Framework;

namespace GameRealisticMap.Studio.Modules.AssetConfigEditor.ViewModels.Filling
{
    internal class FillingAssetClusterViewModel : AssetDensityBase<ClusterCollectionId, ClusterCollectionDefinition, SeedItem>
    {
        public FillingAssetClusterViewModel(ClusterCollectionId id, ClusterCollectionDefinition? definition, AssetConfigEditorViewModel shell)
            : base(id, definition, shell)
        {
            label = definition?.Label ?? string.Empty;
            if (definition != null)
            {
                Items = new ObservableCollection<SeedItem>(definition.Clusters.Select((c, i) => new SeedItem(c, i, this)));
            }
            else
            {
                Items = new ObservableCollection<SeedItem>();
            }

            RemoveSeed = new RelayCommand(item => Items.RemoveUndoable(UndoRedoManager, (SeedItem)item));

            AddEmptySeed = new RelayCommand(_ => {
                Items.AddUndoable(UndoRedoManager,new SeedItem(new ClusterDefinition(new List<ClusterItemDefinition>(), DefinitionHelper.GetNewItemProbility(Items)), Items.Count, this));
                DefinitionHelper.EquilibrateProbabilities(Items);
            }); 

            PreviewBoxWidthPixels = GetPreviewWidth() * PreviewGrid.Scale;
        }

        public override IEnumerable<IExplorerTreeItem> Children => Items;

        public override ObservableCollection<SeedItem> Items { get; }

        public RelayCommand RemoveSeed { get; }

        public RelayCommand AddEmptySeed { get; }

        public double PreviewBoxWidthPixels { get; }

        public override bool IsEmpty => Items.All(i => i.IsEmpty);

        public override ClusterCollectionDefinition ToDefinition()
        {
            return new ClusterCollectionDefinition(
                Items.Where(i => !i.IsEmpty).EquilibrateProbabilities().Select(i => i.ToDefinition()).ToList(), 
                Probability, Density.MinDensity, Density.MaxDensity, Label, Condition.ToDefinition(), Density.ToDefinition());
        }

        public override void AddComposition(Composition composition, ObjectPlacementDetectedInfos detected)
        {
            Items.AddUndoable(UndoRedoManager, new SeedItem(new ClusterDefinition(new ClusterItemDefinition(
                detected.GeneralRadius.Radius,
                detected.GeneralRadius.Radius,
                composition.Translate(-detected.GeneralRadius.Center),
                null,
                null,
                1,
                null,
                null), DefinitionHelper.GetNewItemProbility(Items)), Items.Count, this));
            DefinitionHelper.EquilibrateProbabilities(Items);
        }

        public override void Equilibrate()
        {
            DefinitionHelper.EquilibrateProbabilities(Items);

            foreach(var item in Items)
            {
                DefinitionHelper.EquilibrateProbabilities(item.Items);
            }
        }

        protected override double GetMaxDensity()
        {
            return DensityHelper.GetMaxDensity(ToDefinition().Clusters);
        }

        public override FillAreaBase<Composition> CreatePreviewGenerator()
        {
            if (IsEmpty)
            {
                return new FillAreaLocalClusters<Composition>(new NoProgressSystem(), new List<ClusterCollectionDefinition>());
            }
            return new FillAreaLocalClusters<Composition>(new NoProgressSystem(), new[] { ToDefinition() });
        }

        public override double GetPreviewWidth()
        {
            if (FillId == ClusterCollectionId.ForestEdge)
            {
                return ForestEdgeData.Width;
            }
            if (FillId == ClusterCollectionId.WatercourseRadial)
            {
                return WatercourseRadialData.Width;
            }
            if (FillId == ClusterCollectionId.ScrubRadial)
            {
                return ScrubRadialData.Width;
            }
            if (FillId == ClusterCollectionId.ForestRadial)
            {
                return ForestRadialData.Width;
            }
            return base.GetPreviewWidth();
        }

        protected override List<TerrainBuilderObject> GenerateFullPreviewItems()
        {
            if (FullPreviewGenerator.IsForest(this))
            {
                return FullPreviewGenerator.Forest(this);
            }
            if (FullPreviewGenerator.IsScrub(this))
            {
                return FullPreviewGenerator.Scrub(this);
            }
            return base.GenerateFullPreviewItems();
        }

        public override IEnumerable<string> GetModels()
        {
            return Items.SelectMany(i => i.Items.SelectMany(i => i.Composition.Items.Select(i => i.Model.Path)));
        }
    }
}
