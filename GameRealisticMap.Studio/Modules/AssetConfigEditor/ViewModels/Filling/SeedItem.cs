using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.Assets.Detection;
using GameRealisticMap.Arma3.Assets.Filling;
using GameRealisticMap.Studio.Modules.CompositionTool.ViewModels;
using GameRealisticMap.Studio.Modules.ConditionTool.ViewModels;
using GameRealisticMap.Studio.Modules.Explorer.ViewModels;
using GameRealisticMap.Studio.UndoRedo;
using Gemini.Framework;

namespace GameRealisticMap.Studio.Modules.AssetConfigEditor.ViewModels.Filling
{
    internal class SeedItem : PropertyChangedBase, IWithEditableProbability, IModelImporterTarget, IExplorerTreeItem, IExplorerTreeItemCounter
    {
        private readonly string ordinalName;

        public SeedItem(ClusterDefinition c, int index, FillingAssetClusterViewModel parent)
        {
            _parent = parent;
            Items = new ObservableCollection<FillingItem>(c.Models.Select(m => new FillingItem(m)));
            _probability = c.Probability;
            ordinalName = string.Format(Labels.SeedNumber, index + 1);
            CompositionImporter = new CompositionImporter(this, parent.ParentEditor.Arma3DataModule);
            RemoveItem = new RelayCommand(item => Items.RemoveUndoable(parent.UndoRedoManager,(FillingItem)item));
            Items.CollectionChanged += Items_CollectionChanged;
            Label = GetDistinctName();
            Condition = new PointConditionVM(c.Condition);
        }

        private void Items_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            var newName = GetDistinctName();
            if (newName!= Label)
            {
                Label = newName;
                NotifyOfPropertyChange(nameof(Label));
                NotifyOfPropertyChange(nameof(TreeName));
            }
        }

        private string GetDistinctName()
        {
            if (Items.Count == 0)
            {
                return ordinalName;
            }
            var str = NameHelper.LargestCommon(Items.SelectMany(i => i.Composition.Names).Distinct().Select(StripPrefix).ToList());
            if (string.IsNullOrEmpty(str))
            {
                return ordinalName;
            }
            return ordinalName + " : " + str;
        }

        private string StripPrefix(string name)
        {
            if (name.StartsWith("t_", StringComparison.OrdinalIgnoreCase) || name.StartsWith("b_", StringComparison.OrdinalIgnoreCase))
            {
                return name.Substring(2);
            }
            return name;
        }

        private readonly FillingAssetClusterViewModel _parent;

        public ObservableCollection<FillingItem> Items { get; }

        private double _probability;
        public double Probability
        {
            get { return _probability; }
            set { _probability = value; NotifyOfPropertyChange(); }
        }

        public string Label { get; set; }
        public PointConditionVM Condition { get; }
        public CompositionImporter CompositionImporter { get; }

        public RelayCommand RemoveItem { get; }

        public string TreeName => Label;

        public string Icon => "pack://application:,,,/GameRealisticMap.Studio;component/Resources/Icons/Generic.png";

        public IEnumerable<IExplorerTreeItem> Children => Enumerable.Empty<IExplorerTreeItem>();

        public bool IsEmpty => Items.Count == 0;

        public void AddComposition(Composition composition, ObjectPlacementDetectedInfos detected)
        {
            Items.AddUndoable(_parent.UndoRedoManager,new FillingItem(new ClusterItemDefinition(
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
            return new ClusterDefinition(Items.Select(i => i.ToDefinition()).ToList(), Probability, Condition.ToDefinition());
        }

        public Task MakeItemsEquiprobable()
        {
            DefinitionHelper.Equiprobable(Items, _parent.UndoRedoManager);
            return Task.CompletedTask;
        }
    }
}