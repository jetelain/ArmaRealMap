using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Gemini.Framework;
using Gemini.Framework.Commands;
using Gemini.Modules.Shell.Commands;

namespace GameRealisticMap.Studio.Toolkit
{
    public abstract class PersistedDocument2 : PersistedDocument
    {
        public Task SaveDocument()
        {
            return ((ICommandHandler<SaveFileCommandDefinition>)this).Run(null);
        }

        public override async Task<bool> CanCloseAsync(CancellationToken cancellationToken)
        {
            if (IsDirty)
            {
                var response = MessageBox.Show(string.Format(Labels.UnsavedCloseConfirmText, FileName), Labels.UnsavedCloseConfirmTitle, MessageBoxButton.YesNoCancel);
                if (response == MessageBoxResult.Cancel)
                {
                    return false;
                }
                if (response == MessageBoxResult.Yes)
                {
                    await SaveDocument();
                    return !IsNew;
                }
            }
            return true;
        }
    }
}
