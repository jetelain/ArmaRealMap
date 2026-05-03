using Caliburn.Micro;

namespace GameRealisticMap.Studio.Modules.AssetConfigEditor.ViewModels
{
    /// <summary>
    /// Exposes an editable <c>Probability</c> property (0.0–1.0) for asset entries
    /// that support weighted random selection (object definitions, compositions, etc.).
    /// </summary>
    internal interface IWithEditableProbability : INotifyPropertyChangedEx
    {
        double Probability { get; set; }
    }
}