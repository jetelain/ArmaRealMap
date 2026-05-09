using System.Collections.Generic;
using GameRealisticMap.Studio.Modules.Explorer.ViewModels;
using Gemini.Framework;

namespace GameRealisticMap.Studio.Modules.AssetConfigEditor.ViewModels
{
    /// <summary>
    /// A typed category page inside the asset config editor (e.g. Buildings, Forests, Roads).
    /// Implements <see cref="IExplorerTreeItem"/> so the category appears as a tree node,
    /// and <see cref="Gemini.Framework.IDocument"/> so it opens as a tab.
    /// </summary>
    internal interface IAssetCategory : IDocument, IExplorerTreeItem
    {
        void Equilibrate();

        string IdText { get; }

        string PageTitle { get; }

        IEnumerable<string> GetModels();
    }
}