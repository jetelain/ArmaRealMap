using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            }
        }

    }
}
