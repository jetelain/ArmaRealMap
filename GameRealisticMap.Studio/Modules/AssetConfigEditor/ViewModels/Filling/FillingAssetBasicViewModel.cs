using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.Assets.Detection;
using GameRealisticMap.Arma3.Assets.Filling;
using GameRealisticMap.Arma3.TerrainBuilder;
using Gemini.Framework;
using Gemini.Framework.Services;

namespace GameRealisticMap.Studio.Modules.AssetConfigEditor.ViewModels.Filling
{
    internal class FillingAssetBasicViewModel : AssetDensityBase<BasicCollectionId, BasicCollectionDefinition>
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
            RemoveItem = new RelayCommand(item => Items.Remove((FillingItem)item));
        }

        public ObservableCollection<FillingItem> Items { get; }
        public RelayCommand RemoveItem { get; }

        public override BasicCollectionDefinition ToDefinition()
        {
            DefinitionHelper.EquilibrateProbabilities(Items);
            return new BasicCollectionDefinition(Items.Select(i => i.ToDefinition()).ToList(), Probability, MinDensity, MaxDensity);
        }

        public override void AddSingleObject(ModelInfo model, ObjectPlacementDetectedInfos detected)
        {
            Items.Add(new FillingItem(new ClusterItemDefinition(
                detected.GeneralRadius.Radius,
                detected.GeneralRadius.Radius,
                Composition.CreateSingleFrom(model, -detected.GeneralRadius.Center),
                null,
                null,
                DefinitionHelper.GetNewItemProbility(Items),
                null,
                null)));

            DefinitionHelper.EquilibrateProbabilities(Items);
        }
    }
}
