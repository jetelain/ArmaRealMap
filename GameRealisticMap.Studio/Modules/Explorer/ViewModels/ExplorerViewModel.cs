using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Windows.Data;
using Gemini.Framework;
using Gemini.Framework.Services;

namespace GameRealisticMap.Studio.Modules.Explorer.ViewModels
{
    [Export(typeof(IExplorerTool))]
    public class ExplorerViewModel : Tool, IExplorerTool
    {
        private readonly IShell _shell;
        public ICollectionView Items { get; }

        public override PaneLocation PreferredLocation => PaneLocation.Right;

        [ImportingConstructor]
        public ExplorerViewModel(IShell shell) 
        {
            _shell = shell;
            DisplayName = Labels.Explorer;
            Items = CollectionViewSource.GetDefaultView(_shell.Documents);
            Items.Filter = o => o is IExplorerRootTreeItem;

        }
    }
}
