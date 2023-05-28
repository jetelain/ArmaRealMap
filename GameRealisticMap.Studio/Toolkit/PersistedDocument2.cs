using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Gemini.Framework;
using Gemini.Framework.Commands;
using Gemini.Modules.Shell.Commands;
using MapControl;

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
                var response = MessageBox.Show($"'{FileName}' may have some unsaved changes, do you want to save them?", "Confirm", MessageBoxButton.YesNoCancel);
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
