using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.Assets.Detection;
using GameRealisticMap.Arma3.TerrainBuilder;
using GameRealisticMap.ManMade.Objects;
using Gemini.Framework;

namespace GameRealisticMap.Studio.Modules.AssetConfigEditor.ViewModels.Individual
{
    internal class ObjectsViewModel : AssetBase<ObjectTypeId, List<ObjectDefinition>>
    {
        public ObjectsViewModel(ObjectTypeId id, IReadOnlyCollection<ObjectDefinition> definitions, AssetConfigEditorViewModel parent)
            : base(id, parent)
        {
            Items = new ObservableCollection<ObjectItem>(definitions.Select(d => new ObjectItem(d)));
            RemoveItem = new RelayCommand(item => Items.Remove((ObjectItem)item));
        }

        public ObservableCollection<ObjectItem> Items { get; }
        public RelayCommand RemoveItem { get; }

        public override List<ObjectDefinition> ToDefinition()
        {
            DefinitionHelper.EquilibrateProbabilities(Items);
            return Items.Select(i => i.ToDefinition()).ToList();
        }

        public override void AddComposition(Composition composition, ObjectPlacementDetectedInfos detected)
        {
            Items.Add(new ObjectItem(new ObjectDefinition(composition.Translate(-detected.TrunkRadius.Center), DefinitionHelper.GetNewItemProbility(Items))));
            DefinitionHelper.EquilibrateProbabilities(Items);
        }
    }
}