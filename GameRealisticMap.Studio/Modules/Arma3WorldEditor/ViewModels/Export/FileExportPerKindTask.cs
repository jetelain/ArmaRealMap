using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BIS.WRP;
using Caliburn.Micro;
using GameRealisticMap.Arma3.TerrainBuilder;
using GameRealisticMap.Arma3.TerrainBuilder.TmlFiles;
using GameRealisticMap.Studio.Modules.AssetBrowser.Services;
using GameRealisticMap.Studio.Modules.Reporting;
using GameRealisticMap.Studio.Toolkit;

namespace GameRealisticMap.Studio.Modules.Arma3WorldEditor.ViewModels.Export
{
    internal sealed class FileExportPerKindTask : IProcessTask
    {
        private readonly string targetDirectory;
        private readonly EditableWrp? world;
        private readonly IModelInfoLibrary library;

        public FileExportPerKindTask(string targetDirectory, EditableWrp? world, IModelInfoLibrary library)
        {
            this.targetDirectory = targetDirectory;
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
            Directory.CreateDirectory(targetDirectory);
            var models = world.Objects.Select(o => o.Model).Where(m => !string.IsNullOrEmpty(m)).Distinct(StringComparer.OrdinalIgnoreCase);
            var itemsByPath = await IoC.Get<IAssetsCatalogService>().GetItems(models).ConfigureAwait(false);
            foreach (var category in itemsByPath.Values.GroupBy(v => v.Category))
            {
                var modelsPaths = category.Select(s => s.Path).ToHashSet(StringComparer.OrdinalIgnoreCase);
                var matches = world.GetNonDummyObjects().Where(w => modelsPaths.Contains(w.Model)).ToList();
                if (matches.Count > 0)
                {
                    await File.WriteAllLinesAsync(
                        Path.Combine(targetDirectory, $"{category.Key}.txt"),
                        matches.Select(w => new TerrainBuilderObject(w, library)).Select(f => f.ToTerrainBuilderCSV()));
                }
            }

            new TmlGenerator().WriteLibrariesTo(models.Select(library.ResolveByPath), targetDirectory);

            ui.AddSuccessAction(() => ShellHelper.OpenUri(targetDirectory), Labels.OpenFolder);
        }
    }
}
