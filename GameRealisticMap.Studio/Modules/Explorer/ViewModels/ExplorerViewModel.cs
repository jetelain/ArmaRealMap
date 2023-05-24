using System.ComponentModel.Composition;
using Caliburn.Micro;
using GameRealisticMap.Studio.Toolkit;
using Gemini.Framework;
using Gemini.Framework.Services;

namespace GameRealisticMap.Studio.Modules.Explorer.ViewModels
{
    [Export(typeof(IExplorerTool))]
    public class ExplorerViewModel : Tool, IExplorerTool
    {
        private readonly IShell _shell;
        public IObservableCollection<IExplorerRootTreeItem> Items { get; }

        public override PaneLocation PreferredLocation => PaneLocation.Right;

        [ImportingConstructor]
        public ExplorerViewModel(IShell shell) 
        {
            _shell = shell;
            Items = new ObservableCollectionOfType<IExplorerRootTreeItem, IDocument>(_shell.Documents);
            DisplayName = "Explorer";
        }
    }
}
