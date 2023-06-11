namespace GameRealisticMap.Studio.Modules.AssetConfigEditor.ViewModels.Filling
{
    internal interface IFillAssetCategory : IAssetCategory, IWithEditableProbability
    {
        string Label { get; set; }

        bool IsSameFillId(object fillId);

        object IdObj { get; }
    }
}
