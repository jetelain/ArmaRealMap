using System.Threading.Tasks;
using GameRealisticMap.Arma3.Edit;
using GameRealisticMap.Studio.Modules.Reporting;

namespace GameRealisticMap.Studio.Modules.Arma3WorldEditor.ViewModels.MassEdit
{
    internal class MassEditTask : IProcessTask
    {
        private readonly WrpMassEditBatch batch;
        private readonly Arma3WorldEditorViewModel vm;

        public MassEditTask(WrpMassEditBatch batch, Arma3WorldEditorViewModel vm)
        {
            this.batch = batch;
            this.vm = vm;
        }

        public string Title => "Mass edit";

        public bool Prompt => false;

        public Task Run(IProgressTaskUI ui)
        {
            var world = vm.World;
            if (world != null)
            {
                var processor = new WrpMassEditProcessor(ui.Scope, vm.Library);

                processor.Process(world, batch);

                vm.PostEdit();
            }
            return Task.CompletedTask;
        }
    }
}
