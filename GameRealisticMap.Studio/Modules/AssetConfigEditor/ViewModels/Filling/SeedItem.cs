using System.Collections.ObjectModel;
using System.Linq;
using Caliburn.Micro;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.Assets.Detection;
using GameRealisticMap.Arma3.Assets.Filling;
using GameRealisticMap.Arma3.TerrainBuilder;
using GameRealisticMap.Studio.Modules.CompositionTool.ViewModels;
using Gemini.Framework;

namespace GameRealisticMap.Studio.Modules.AssetConfigEditor.ViewModels.Filling
{
    internal class SeedItem : PropertyChangedBase, IWithEditableProbability, IModelImporterTarget
    {
        public SeedItem(ClusterDefinition c, int index, FillingAssetClusterViewModel parent)
        {
            Items = new ObservableCollection<FillingItem>(c.Models.Select(m => new FillingItem(m)));
            _probability = c.Probability;
            Label = $"Seed #{index + 1}";
            CompositionImporter = new CompositionImporter(this);
            RemoveItem = new RelayCommand(item => Items.Remove((FillingItem)item));
        }

        public ObservableCollection<FillingItem> Items { get; }

        private double _probability;
        public double Probability
        {
            get { return _probability; }
            set { _probability = value; NotifyOfPropertyChange(); }
        }

        public string Label { get; set; }

        public CompositionImporter CompositionImporter { get; }

        public RelayCommand RemoveItem { get; }

        public void AddComposition(Composition composition, ObjectPlacementDetectedInfos detected)
        {
            Items.Add(new FillingItem(new ClusterItemDefinition(
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

        internal ClusterDefinition ToDefinition()
        {
            DefinitionHelper.EquilibrateProbabilities(Items);
            return new ClusterDefinition(Items.Select(i => i.ToDefinition()).ToList(), Probability);
        }
    }
}