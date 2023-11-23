using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BIS.WRP;
using GameRealisticMap.Arma3.TerrainBuilder;
using GameRealisticMap.Arma3.TerrainBuilder.TmlFiles;
using GameRealisticMap.Studio.Modules.Reporting;
using GameRealisticMap.Studio.Toolkit;

namespace GameRealisticMap.Studio.Modules.Arma3WorldEditor.ViewModels.Export
{
    internal sealed class FileExportSingleTask : IProcessTask
    {
        private readonly string targetFile;
        private readonly EditableWrp? world;
        private readonly IModelInfoLibrary library;

        public FileExportSingleTask(string targetFile, EditableWrp? world, IModelInfoLibrary library)
        {
            this.targetFile = targetFile;
            this.world = world;
            this.library = library;
        }

        public string Title => "Export objects to file";

        public bool Prompt => true;

        public async Task Run(IProgressTaskUI ui)
        {
            if (world == null)
            {
                return;
            }

            var targetDirectory = Path.GetDirectoryName(targetFile)!;

            Directory.CreateDirectory(targetDirectory);

            await File.WriteAllLinesAsync(
                targetFile,
                world.GetNonDummyObjects().Select(w => new TerrainBuilderObject(w, library)).Select(f => f.ToTerrainBuilderCSV()));

            var models = world.Objects.Select(o => o.Model).Where(m => !string.IsNullOrEmpty(m)).Distinct(StringComparer.OrdinalIgnoreCase);

            new TmlGenerator().WriteLibrariesTo(models.Select(library.ResolveByPath), Path.Combine(targetDirectory,Path.GetFileNameWithoutExtension(targetFile)));

            ui.AddSuccessAction(() => ShellHelper.OpenUri(targetFile), Labels.OpenResult);
            ui.AddSuccessAction(() => ShellHelper.OpenUri(Path.GetDirectoryName(targetFile)!), Labels.OpenFolder);
        }
    }
}
