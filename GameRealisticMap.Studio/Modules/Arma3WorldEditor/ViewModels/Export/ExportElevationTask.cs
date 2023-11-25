using System.IO;
using System.Threading.Tasks;
using BIS.WRP;
using GameRealisticMap.Arma3.GameEngine;
using GameRealisticMap.Studio.Modules.Reporting;
using MapToolkit.DataCells.FileFormats;

namespace GameRealisticMap.Studio.Modules.Arma3WorldEditor.ViewModels.Export
{
    internal class ExportElevationTask : SingleFileExportBase
    {
        private readonly EditableWrp world;

        public ExportElevationTask(EditableWrp world, string targetFile)
            : base(targetFile)
        {
            this.world = world;
        }

        public override string Title => "Export elevation to file";

        protected override Task<bool> Export(IProgressTaskUI ui, string targetFile)
        {
            using (var writer = File.CreateText(targetFile))
            {
                using var report = ui.CreateStepPercent("Elevation.AscFile");
                EsriAsciiHelper.SaveDataCell(writer, world.ToElevationGrid().ToDataCell(), "-9999", report);
            }
            return Task.FromResult(true);
        }
    }
}
