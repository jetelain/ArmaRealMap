using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using GameRealisticMap.Arma3.Assets.Filling;
using GameRealisticMap.ManMade.Fences;
using GameRealisticMap.Studio.Modules.AssetConfigEditor.ViewModels.Filling;
using GameRealisticMap.Studio.UndoRedo;

namespace GameRealisticMap.Studio.Modules.AssetConfigEditor.ViewModels
{
    public class AdditionalFilling : ICommand
    {
        private readonly Func<IFillAssetCategory> create;
        private readonly AssetConfigEditorViewModel target;

        internal AdditionalFilling(string idText, AssetConfigEditorViewModel target, Func<IFillAssetCategory> create)
        {
            Label = Labels.ResourceManager.GetString("Asset" + idText) ?? idText;
            this.create = create;
            this.target = target;
        }

        public string Label { get; }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter)
        {
            return true;
        }

        public void Execute(object? parameter)
        {
            var newItem = create();
            var others = target.Filling.Where(f => f.IsSameFillId(newItem.IdObj)).ToList();
            newItem.Probability = DefinitionHelper.GetNewItemProbility(others);
            newItem.Label = "Additional #" + others.Count;
            target.Filling.AddUndoable(target.UndoRedoManager, newItem);
            others.Add(newItem);
            DefinitionHelper.EquilibrateProbabilities(others);
        }

        internal static List<AdditionalFilling> Create(AssetConfigEditorViewModel target)
        {
            var list = new List<AdditionalFilling>();
            foreach (var id in Enum.GetValues<BasicCollectionId>())
            {
                list.Add(new AdditionalFilling(id.ToString(), target, () => new FillingAssetBasicViewModel(id, null, target)));
            }
            foreach (var id in Enum.GetValues<ClusterCollectionId>())
            {
                list.Add(new AdditionalFilling(id.ToString(), target, () => new FillingAssetClusterViewModel(id, null, target)));
            }
            foreach (var id in Enum.GetValues<FenceTypeId>())
            {
                list.Add(new AdditionalFilling(id.ToString(), target, () => new FencesViewModel(id, null, target)));
            }
            list.Sort((a, b) => a.Label.CompareTo(b.Label));
            return list;
        }
    }
}
