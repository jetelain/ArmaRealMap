using System.Windows.Input;

namespace GameRealisticMap.Studio.Modules.AssetConfigEditor.ViewModels
{
    /// <summary>
    /// An <see cref="System.Windows.Input.ICommand"/> that also carries a display label,
    /// used to populate action buttons in the success dialog and other dynamic UI.
    /// </summary>
    internal interface ICommandWithLabel : ICommand
    {
        string Label { get; }
    }
}