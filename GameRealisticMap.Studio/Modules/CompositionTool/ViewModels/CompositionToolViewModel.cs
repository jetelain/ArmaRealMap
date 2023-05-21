using System.ComponentModel.Composition;
using Gemini.Framework;
using Gemini.Framework.Services;

namespace GameRealisticMap.Studio.Modules.CompositionTool.ViewModels
{
    [Export(typeof(ICompositionTool))]
    internal class CompositionToolViewModel : Tool, ICompositionTool
    {
        public override PaneLocation PreferredLocation => PaneLocation.Right;

        public CompositionToolViewModel()
        {
            DisplayName = "Objects position";
        }

        private IWithComposition? _current;
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
    }
}
