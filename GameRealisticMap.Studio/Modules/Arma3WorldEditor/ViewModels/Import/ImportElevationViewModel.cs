using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using GameRealisticMap.ElevationModel;
using GameRealisticMap.Studio.Modules.Arma3WorldEditor.ViewModels.Export;
using GameRealisticMap.Studio.Modules.Reporting;
using Pmad.Cartography.DataCells;
using Pmad.Cartography.DataCells.FileFormats;

namespace GameRealisticMap.Studio.Modules.Arma3WorldEditor.ViewModels.Import
{
    internal class ImportElevationViewModel : ModalProgressBase
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetLogger("FileImporter");

        private readonly Arma3WorldEditorViewModel parent;
        private readonly string fileName;
        private ElevationGrid? grid;
        private bool isUpdateElevation = true;

        public ImportElevationViewModel(Arma3WorldEditorViewModel parent, string fileName)
        {
            this.parent = parent;
            this.fileName = fileName;
        }

        public string FileName => fileName;

        public bool IsUpdateElevation
        {
            get { return isUpdateElevation; }
            set { if (isUpdateElevation != value) { isUpdateElevation = value; NotifyOfPropertyChange(); NotifyOfPropertyChange(nameof(IsKeepElevation)); } }
        }

        public bool IsKeepElevation { get { return !IsUpdateElevation; } set { IsUpdateElevation = !value; } }

        protected override Task OnActivateAsync(CancellationToken cancellationToken)
        {
            IsWorking = true;
            _ = Task.Run(DoReadFile);
            return Task.CompletedTask;
        }

        private void DoReadFile()
        {
            Error = string.Empty;
            try
            {
                DemDataCellBase<float> cell;
                using (var reader = File.OpenText(fileName))
                {
                    cell = EsriAsciiHelper.LoadDataCell(reader, this);
                }
                grid = new ElevationGrid((DemDataCellPixelIsPoint<float>)cell);
                if (grid.Size != parent.World!.TerrainRangeX)
                {
                    Error = string.Format("Size mismatch, terrain is {0} but imported file is {1}", parent.World!.TerrainRangeX, grid.Size);
                }
                if (Math.Abs(grid.CellSize.X - parent.CellSize!.Value) > 0.01)
                {
                    Error = string.Format("Cell size mismatch, terrain is {0} but imported file is {1}", grid.CellSize.X, parent.CellSize!.Value);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Error = ex.Message;
            }
            IsWorking = false;
        }

        public async Task Import()
        {
            if (grid != null && ProgressToolHelper.Start(new ImportElevationTask(parent, grid, isUpdateElevation)))
            {
                await TryCloseAsync(false);
            }
        }
    }
}
