using System.Windows.Input;

namespace GameRealisticMap.Studio.Modules.AssetConfigEditor.ViewModels
{
    internal interface ICommandWithLabel : ICommand
    {
        string Label { get; }
    }
}