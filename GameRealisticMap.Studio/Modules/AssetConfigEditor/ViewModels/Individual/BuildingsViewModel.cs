using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.Assets.Detection;
using GameRealisticMap.ManMade.Buildings;
using GameRealisticMap.Studio.UndoRedo;
using Gemini.Framework;

namespace GameRealisticMap.Studio.Modules.AssetConfigEditor.ViewModels.Individual
{
    internal class BuildingsViewModel : AssetBase<BuildingTypeId, List<BuildingDefinition>>
    {
        public BuildingsViewModel(BuildingTypeId id, IReadOnlyCollection<BuildingDefinition> definitions, AssetConfigEditorViewModel parent)
            : base(id, parent)
        {
            Items = new ObservableCollection<BuildingItem>(definitions.Select(d => new BuildingItem(d)));
            RemoveItem = new RelayCommand(item => Items.RemoveUndoable(UndoRedoManager,(BuildingItem)item));
        }

        public ObservableCollection<BuildingItem> Items { get; }

        public RelayCommand RemoveItem { get; }

        public override List<BuildingDefinition> ToDefinition()
        {
            return Items.Select(i => i.ToDefinition()).ToList();
        }

        public override void AddComposition(Composition composition, ObjectPlacementDetectedInfos detected)
        {
            var use = detected.UpperRectangle;
            if (detected.GeneralRectangle.Surface > use.Surface * 3)
            {
                use = detected.GeneralRectangle;
            }
            Items.AddUndoable(UndoRedoManager,new BuildingItem(new BuildingDefinition(use.Size, composition.Translate(-use.Center))));
        }

        public override void Equilibrate()
        {

        }
    }
}