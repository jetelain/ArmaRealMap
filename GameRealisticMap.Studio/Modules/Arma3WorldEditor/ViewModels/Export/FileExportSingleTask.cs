using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BIS.WRP;
using GameRealisticMap.Arma3.TerrainBuilder;
using GameRealisticMap.Arma3.TerrainBuilder.TmlFiles;
using GameRealisticMap.Studio.Modules.Reporting;

namespace GameRealisticMap.Studio.Modules.Arma3WorldEditor.ViewModels.Export
{
    internal sealed class FileExportSingleTask : SingleFileExportBase
    {
        private readonly EditableWrp? world;
        private readonly IModelInfoLibrary library;

        public FileExportSingleTask(string targetFile, EditableWrp? world, IModelInfoLibrary library)
            : base(targetFile)
        {
            this.world = world;
            this.library = library;
        }

        public override string Title => "Export objects to file";

        protected override async Task<bool> Export(IProgressTaskUI ui, string targetFile)
        {
            if (world == null)
            {
                return false;
            }

            var targetDirectory = Path.GetDirectoryName(targetFile)!;

            Directory.CreateDirectory(targetDirectory);

            await File.WriteAllLinesAsync(
                targetFile,
                world.GetNonDummyObjects().Select(w => new TerrainBuilderObject(w, library)).Select(f => f.ToTerrainBuilderCSV()));

            var models = world.Objects.Select(o => o.Model).Where(m => !string.IsNullOrEmpty(m)).Distinct(StringComparer.OrdinalIgnoreCase);

            new TmlGenerator().WriteLibrariesTo(models.Select(library.ResolveByPath), Path.Combine(targetDirectory, Path.GetFileNameWithoutExtension(targetFile)));

            return true;
        }
    }
}
