using System.Threading.Tasks;
using GameRealisticMap.Arma3.Edit;
using GameRealisticMap.Arma3.GameEngine;
using GameRealisticMap.ElevationModel;
using GameRealisticMap.Studio.Modules.Reporting;

namespace GameRealisticMap.Studio.Modules.Arma3WorldEditor.ViewModels.Import
{
    internal class ImportElevationTask : IProcessTask
    {
        private readonly Arma3WorldEditorViewModel parent;
        private readonly ElevationGrid grid;
        private readonly bool isUpdateElevation;

        public ImportElevationTask(Arma3WorldEditorViewModel parent, ElevationGrid grid, bool isUpdateElevation)
        {
            this.parent = parent;
            this.grid = grid;
            this.isUpdateElevation = isUpdateElevation;
        }

        public string Title => "Import elevation";

        public bool Prompt => false;

        public Task Run(IProgressTaskUI ui)
        {
            var world = parent.World;
            if (world == null)
            {
                return Task.CompletedTask;
            }
            var worker = new WrpEditProcessor(ui.Scope);
            if (isUpdateElevation)
            {
                worker.UpdateElevationGrid(world, grid);
            }
            else
            {
                worker.UpdateElevationGridAbsolute(world, grid);
            }
            parent.PostEdit();
            return Task.CompletedTask;
        }
    }
}