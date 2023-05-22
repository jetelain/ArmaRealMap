using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.Assets.Detection;
using GameRealisticMap.Arma3.Assets.Filling;
using GameRealisticMap.Arma3.TerrainBuilder;
using GameRealisticMap.Studio.UndoRedo;
using Gemini.Framework;

namespace GameRealisticMap.Studio.Modules.AssetConfigEditor.ViewModels.Filling
{
    internal class FillingAssetClusterViewModel : AssetDensityBase<ClusterCollectionId, ClusterCollectionDefinition>
    {
        public FillingAssetClusterViewModel(ClusterCollectionId id, ClusterCollectionDefinition? definition, AssetConfigEditorViewModel shell)
            : base(id, definition, shell)
        {
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
        }


        public ObservableCollection<SeedItem> Items { get; }

        public RelayCommand RemoveSeed { get; }

        public RelayCommand AddEmptySeed { get; }

        public override ClusterCollectionDefinition ToDefinition()
        {
            DefinitionHelper.EquilibrateProbabilities(Items);
            return new ClusterCollectionDefinition(Items.Select(i => i.ToDefinition()).ToList(), Probability, MinDensity, MaxDensity);
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
    }
}
