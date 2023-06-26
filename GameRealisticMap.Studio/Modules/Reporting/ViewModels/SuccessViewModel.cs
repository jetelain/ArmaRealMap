using System.Collections.Generic;
using System.Threading.Tasks;
using Gemini.Framework;

namespace GameRealisticMap.Studio.Modules.Reporting.ViewModels
{
    public sealed class SuccessViewModel : WindowBase
    {
        private readonly ProgressTask task;

        internal SuccessViewModel(ProgressTask task)
        {
            this.task = task;
        }

        public string TaskName => task.TaskName;

        public string Status => string.Format(Labels.DoneSuccessfullyInSeconds, task.ElapsedSeconds);

        public List<SuccessAction> Actions => task.SuccessActions;

        public Task Close() => TryCloseAsync();
    }
}
