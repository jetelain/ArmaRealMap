using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.Assets.Detection;
using GameRealisticMap.Arma3.TerrainBuilder;
using GameRealisticMap.ManMade.Buildings;
using Gemini.Framework;

namespace GameRealisticMap.Studio.Modules.AssetConfigEditor.ViewModels.Individual
{
    internal class BuildingsViewModel : AssetBase<BuildingTypeId, List<BuildingDefinition>>
    {
        public BuildingsViewModel(BuildingTypeId id, IReadOnlyCollection<BuildingDefinition> definitions, AssetConfigEditorViewModel parent)
            : base(id, parent)
        {
            Items = new ObservableCollection<BuildingItem>(definitions.Select(d => new BuildingItem(d)));
            RemoveItem = new RelayCommand(item => Items.Remove((BuildingItem)item));
        }

        public ObservableCollection<BuildingItem> Items { get; }
        public RelayCommand RemoveItem { get; }

        public override List<BuildingDefinition> ToDefinition()
        {
            return Items.Select(i => i.ToDefinition()).ToList();
        }

        public override void AddSingleObject(ModelInfo model, ObjectPlacementDetectedInfos detected)
        {
            Items.Add(new BuildingItem(new BuildingDefinition(detected.Rectangle.Size, Composition.CreateSingleFrom(model, -detected.Rectangle.Center))));
        }
    }
}