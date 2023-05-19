using Caliburn.Micro;

namespace GameRealisticMap.Studio.Modules.AssetConfigEditor.ViewModels
{
    internal interface IWithEditableProbability : INotifyPropertyChangedEx
    {
        double Probability { get; set; }
    }
}