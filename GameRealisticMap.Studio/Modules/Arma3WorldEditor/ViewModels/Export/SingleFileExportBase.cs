using System.IO;
using System.Threading.Tasks;
using GameRealisticMap.Studio.Modules.Reporting;
using GameRealisticMap.Studio.Toolkit;

namespace GameRealisticMap.Studio.Modules.Arma3WorldEditor.ViewModels.Export
{
    internal abstract class SingleFileExportBase : IProcessTask
    {
        private readonly string targetFile;

        public SingleFileExportBase(string targetFile)
        {
            this.targetFile = targetFile;
        }

        public abstract string Title { get; }

        public bool Prompt => true;

        public async Task Run(IProgressTaskUI ui)
        {
            if (await Export(ui, targetFile))
            {
                ui.AddSuccessAction(() => ShellHelper.OpenUri(targetFile), Labels.OpenResult);
                ui.AddSuccessAction(() => ShellHelper.OpenUri(Path.GetDirectoryName(targetFile)!), Labels.OpenFolder);
            }
        }

        protected abstract Task<bool> Export(IProgressTaskUI ui, string targetFile);
    }
}
