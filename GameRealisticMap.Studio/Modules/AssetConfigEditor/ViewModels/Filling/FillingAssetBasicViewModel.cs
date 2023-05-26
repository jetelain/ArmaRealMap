using System.Collections.ObjectModel;
using System.Linq;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.Assets.Detection;
using GameRealisticMap.Arma3.Assets.Filling;
using GameRealisticMap.Arma3.TerrainBuilder;
using GameRealisticMap.Studio.Modules.Explorer.ViewModels;
using GameRealisticMap.Studio.UndoRedo;
using Gemini.Framework;

namespace GameRealisticMap.Studio.Modules.AssetConfigEditor.ViewModels.Filling
{
    internal class FillingAssetBasicViewModel : AssetDensityBase<BasicCollectionId, BasicCollectionDefinition>, IExplorerTreeItemCounter
    {
        public FillingAssetBasicViewModel(BasicCollectionId id, BasicCollectionDefinition? definition, AssetConfigEditorViewModel shell)
            : base(id, definition, shell)
        {
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

        public ObservableCollection<FillingItem> Items { get; }
        public RelayCommand RemoveItem { get; }

        public override BasicCollectionDefinition ToDefinition()
        {
            DefinitionHelper.EquilibrateProbabilities(Items);
            return new BasicCollectionDefinition(Items.Select(i => i.ToDefinition()).ToList(), Probability, MinDensity, MaxDensity);
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
    }
}
