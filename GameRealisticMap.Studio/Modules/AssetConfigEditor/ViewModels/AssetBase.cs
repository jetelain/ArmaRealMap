using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.Assets.Detection;
using GameRealisticMap.Studio.Modules.CompositionTool.ViewModels;
using GameRealisticMap.Studio.Modules.Explorer.ViewModels;
using Gemini.Framework;
using Gemini.Framework.Commands;
using Gemini.Modules.Shell.Commands;
using Gemini.Modules.ToolBars;
using Gemini.Modules.UndoRedo;
using Gemini.Modules.UndoRedo.Commands;

namespace GameRealisticMap.Studio.Modules.AssetConfigEditor.ViewModels
{
    // IDocument : ILayoutItem, IScreen, IHaveDisplayName, IActivate, IDeactivate, IGuardClose, IClose, INotifyPropertyChangedEx, INotifyPropertyChanged

    internal abstract class AssetBase<TDefinition> : LayoutItemBase, IDocument, ICommandHandler<UndoCommandDefinition>, ICommandHandler, ICommandHandler<RedoCommandDefinition>, ICommandHandler<SaveFileCommandDefinition>, ICommandHandler<SaveFileAsCommandDefinition>, IModelImporterTarget, IAssetCategory
        where TDefinition : class
    {
        private AsyncCommand? _closeCommand;

        protected AssetBase(AssetConfigEditorViewModel parent, string idText)
        {
            IdText = idText;
            PageTitle = Labels.ResourceManager.GetString("Asset" + IdText) ?? IdText;
            DisplayName = parent.FileName + ": " + IdText;
            ParentEditor = parent;
            Edit = new AsyncCommand(() => parent.EditAssetCategory(this));
            EditComposition = new RelayCommand(c => parent.EditComposition((IWithComposition)c));
            Back = new AsyncCommand(() => parent.EditAssetCategory(parent));
            CompositionImporter = new CompositionImporter(this, parent.Arma3DataModule);
        }

        public string IdText { get; }

        public virtual string Icon => $"pack://application:,,,/GameRealisticMap.Studio;component/Resources/Icons/{IdText}.png";

        public string PageTitle { get; }

        public virtual string TreeName => PageTitle;

        public AssetConfigEditorViewModel ParentEditor { get; }

        public AsyncCommand Edit { get; }

        public RelayCommand EditComposition { get; }

        public AsyncCommand Back { get; }

        public CompositionImporter CompositionImporter { get; }

        public virtual IEnumerable<IExplorerTreeItem> Children => Enumerable.Empty<IExplorerTreeItem>();

        public IUndoRedoManager UndoRedoManager => ParentEditor.UndoRedoManager;

        public abstract void AddComposition(Composition model, ObjectPlacementDetectedInfos detected);

        public abstract void Equilibrate();

        public abstract TDefinition ToDefinition();

        public abstract IEnumerable<string> GetModels();

        public override ICommand CloseCommand => _closeCommand ?? (_closeCommand = new AsyncCommand(() => TryCloseAsync()));

        // Replicate commands to Parent editor (as everything is global)

        public IToolBar ToolBar => ParentEditor.ToolBar;

        void ICommandHandler<UndoCommandDefinition>.Update(Command command)
        {
            ((ICommandHandler<UndoCommandDefinition>)ParentEditor).Update(command);
        }

        Task ICommandHandler<UndoCommandDefinition>.Run(Command command)
        {
            return ((ICommandHandler<UndoCommandDefinition>)ParentEditor).Run(command);
        }

        void ICommandHandler<RedoCommandDefinition>.Update(Command command)
        {
            ((ICommandHandler<RedoCommandDefinition>)ParentEditor).Update(command);
        }

        Task ICommandHandler<RedoCommandDefinition>.Run(Command command)
        {
            return ((ICommandHandler<RedoCommandDefinition>)ParentEditor).Run(command);
        }

        void ICommandHandler<SaveFileCommandDefinition>.Update(Command command)
        {
            ((ICommandHandler<SaveFileCommandDefinition>)ParentEditor).Update(command);
        }

        Task ICommandHandler<SaveFileCommandDefinition>.Run(Command command)
        {
            return ((ICommandHandler<SaveFileCommandDefinition>)ParentEditor).Run(command);
        }

        void ICommandHandler<SaveFileAsCommandDefinition>.Update(Command command)
        {
            ((ICommandHandler<SaveFileAsCommandDefinition>)ParentEditor).Update(command);
        }

        Task ICommandHandler<SaveFileAsCommandDefinition>.Run(Command command)
        {
            return ((ICommandHandler<SaveFileAsCommandDefinition>)ParentEditor).Run(command);
        }
    }
}
