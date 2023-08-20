using System;
using System.Linq;
using Caliburn.Micro;
using GameRealisticMap.Studio.Modules.AssetConfigEditor.ViewModels.Filling;
using GameRealisticMap.Studio.UndoRedo;
using Gemini.Modules.UndoRedo;

namespace GameRealisticMap.Studio.Modules.AssetConfigEditor.ViewModels
{
    internal class AdditionalFilling<T> : ICommandWithLabel
        where T : class, IFillAssetCategory
    {
        private readonly Func<T> create;
        private readonly BindableCollection<T> target;
        private readonly IUndoRedoManager undoRedoManager;

        internal AdditionalFilling(string idText, Func<T> create, BindableCollection<T> target, IUndoRedoManager undoRedoManager)
        {
            Label = Labels.ResourceManager.GetString("Asset" + idText) ?? idText;
            this.create = create;
            this.target = target;
            this.undoRedoManager = undoRedoManager;
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
            var others = target.Where(f => f.IsSameFillId(newItem.IdObj)).ToList();
            newItem.Probability = DefinitionHelper.GetNewItemProbility(others);
            newItem.Label = "Additional #" + others.Count;
            target.AddUndoable(undoRedoManager, newItem);
            others.Add(newItem);
            DefinitionHelper.EquilibrateProbabilities(others);
        }
    }
}
