using System.ComponentModel.Composition;
using Caliburn.Micro;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.Assets.Detection;
using GameRealisticMap.Studio.Modules.Arma3Data;
using Gemini.Framework;
using Gemini.Framework.Services;
using Gemini.Modules.UndoRedo;
using Gemini.Modules.UndoRedo.Services;

namespace GameRealisticMap.Studio.Modules.CompositionTool.ViewModels
{
    [Export(typeof(ICompositionTool))]
    internal class CompositionToolViewModel : Tool, ICompositionTool, IModelImporterTarget
    {
        public override double PreferredWidth => 485;

        public override PaneLocation PreferredLocation => PaneLocation.Right;

        [ImportingConstructor]
        public CompositionToolViewModel(IArma3DataModule arma3DataModule, IWindowManager windowManager)
        {
            Importer = new CompositionImporter(this, arma3DataModule, windowManager);
            DisplayName = "Objects position";
            RemoveItem = new RelayCommand(i => Current!.Composition.RemoveItem((CompositionItem)i, UndoRedoManager!));
        }

        private IWithComposition? _current;
        private IUndoRedoManager? _undoRedoManager;

        public IWithComposition? Current
        {
            get { return _current; }
            set
            {
                _current = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(Composition));
                NotifyOfPropertyChange(nameof(HasRectangle));
                NotifyOfPropertyChange(nameof(Rectangle));
                NotifyOfPropertyChange(nameof(HasRadius));
                NotifyOfPropertyChange(nameof(Radius));
            }
        }

        public IWithCompositionRectangle? Rectangle => _current as IWithCompositionRectangle;

        public bool HasRectangle => _current is IWithCompositionRectangle;

        public IWithCompositionRadius? Radius => _current as IWithCompositionRadius;

        public bool HasRadius => _current is IWithCompositionRadius;

        public CompositionViewModel? Composition
        {
            get { return _current?.Composition; }
        }

        public IUndoRedoManager? UndoRedoManager
        {
            get { return _undoRedoManager; }
            set { _undoRedoManager = value; NotifyOfPropertyChange(); }
        }

        public CompositionImporter Importer { get; }

        public RelayCommand RemoveItem { get; }

        public void AddComposition(Composition composition, ObjectPlacementDetectedInfos detected)
        {
            if (Current != null)
            {
                Current.Composition.AddRange(composition.Objects, UndoRedoManager!);
            }
        }
    }
}
