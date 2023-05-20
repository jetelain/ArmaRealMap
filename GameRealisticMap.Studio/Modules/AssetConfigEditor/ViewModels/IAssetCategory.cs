using Gemini.Framework;

namespace GameRealisticMap.Studio.Modules.AssetConfigEditor.ViewModels
{
    internal interface IAssetCategory : IDocument
    {
        void Equilibrate();

        string IdText { get; }
    }
}