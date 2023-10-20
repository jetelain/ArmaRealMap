using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using GameRealisticMap.Algorithms;
using GameRealisticMap.Algorithms.Filling;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.Assets.Detection;
using GameRealisticMap.Arma3.Assets.Filling;
using GameRealisticMap.Arma3.TerrainBuilder;
using GameRealisticMap.Reporting;
using GameRealisticMap.Studio.Modules.Explorer.ViewModels;
using GameRealisticMap.Studio.UndoRedo;
using Gemini.Framework;

namespace GameRealisticMap.Studio.Modules.AssetConfigEditor.ViewModels.Filling
{
    internal class FillingAssetBasicViewModel : AssetDensityBase<BasicCollectionId, BasicCollectionDefinition, FillingItem>, IExplorerTreeItemCounter
    {
        public FillingAssetBasicViewModel(BasicCollectionId id, BasicCollectionDefinition? definition, AssetConfigEditorViewModel shell)
            : base(id, definition, shell)
        {
            label = definition?.Label ?? string.Empty;
            if (definition != null)
            {
                Items = new ObservableCollection<FillingItem>(definition.Models.Select(d => new FillingItem(d)));
            }
            else
            {
                Items = new();
            }
            RemoveItem = new RelayCommand(item => Items.RemoveUndoable(UndoRedoManager, (FillingItem)item));
        }

        public override ObservableCollection<FillingItem> Items { get; }

        public RelayCommand RemoveItem { get; }



        public override BasicCollectionDefinition ToDefinition()
        {
            DefinitionHelper.EquilibrateProbabilities(Items);
            return new BasicCollectionDefinition(Items.Select(i => i.ToDefinition()).ToList(), Probability, MinDensity, MaxDensity, Label, Condition.ToDefinition());
        }

        public override void AddComposition(Composition composition, ObjectPlacementDetectedInfos detected)
        {
            Items.AddUndoable(UndoRedoManager, new FillingItem(new ClusterItemDefinition(
                detected.GeneralRadius.Radius,
                detected.GeneralRadius.Radius,
                composition.Translate(-detected.GeneralRadius.Center),
                null,
                null,
                DefinitionHelper.GetNewItemProbility(Items),
                null,
                null)));

            DefinitionHelper.EquilibrateProbabilities(Items);
        }

        public override void Equilibrate()
        {
            DefinitionHelper.EquilibrateProbabilities(Items);
        }

        protected override double GetMaxDensity()
        {
            return DensityHelper.GetMaxDensity(ToDefinition().Models);
        }

        public override FillAreaBase<Composition> CreatePreviewGenerator()
        {
            if (IsEmpty)
            {
                return new FillAreaBasic<Composition>(new NoProgressSystem(), new List<BasicCollectionDefinition>());
            }
            return new FillAreaBasic<Composition>(new NoProgressSystem(), new[] { ToDefinition() });
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
            if (FullPreviewGenerator.IsRocks(this))
            {
                return FullPreviewGenerator.Rocks(this);
            }
            return base.GenerateFullPreviewItems();
        }

        public override IEnumerable<string> GetModels()
        {
            return Items.SelectMany(i => i.Composition.Items.Select(i => i.Model.Path));
        }
    }
}
