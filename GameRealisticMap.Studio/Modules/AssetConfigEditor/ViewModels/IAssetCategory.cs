using GameRealisticMap.Studio.Modules.Explorer.ViewModels;
using Gemini.Framework;

namespace GameRealisticMap.Studio.Modules.AssetConfigEditor.ViewModels
{
    internal interface IAssetCategory : IDocument, IExplorerTreeItem
    {
        void Equilibrate();

        string IdText { get; }
    }
}