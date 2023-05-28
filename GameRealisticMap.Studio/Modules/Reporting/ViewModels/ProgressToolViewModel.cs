using System.ComponentModel.Composition;
using System.Threading.Tasks;
using Caliburn.Micro;
using Gemini.Framework;
using Gemini.Framework.Services;

namespace GameRealisticMap.Studio.Modules.Reporting.ViewModels
{
    [Export(typeof(IProgressTool))]
    internal class ProgressToolViewModel : Tool, IProgressTool
    {
        private TaskState state =  TaskState.None;
        private ProgressTask? current = null;

        public override PaneLocation PreferredLocation => PaneLocation.Right;

        public BindableCollection<ProgressStep> Items { get; set; } = new BindableCollection<ProgressStep>();

        public ProgressToolViewModel()
        {
            DisplayName = "Task Progress";
        }

        public TaskState State
        {
            get { return state; }
            set
            {
                state = value;
                NotifyOfPropertyChange();
            }
        }

        public double Percent { get; set; }

        public string TaskName { get; set; } = string.Empty;

        public bool IsRunning => state == TaskState.Running || state == TaskState.Canceling;

        public IProgressTaskUI StartTask(string name)
        {
            Items.Clear();

            Percent = 0;
            NotifyOfPropertyChange(nameof(Percent));

            TaskName = name;
            NotifyOfPropertyChange(nameof(TaskName));

            State = TaskState.Running;

            return current = new ProgressTask(this);
        }

        public Task CancelTask()
        {
            if (current != null)
            {
                current.Cancel();
            }
            return Task.CompletedTask;
        }

        public Task ShowTaskResult()
        {
            if (current != null)
            {
                current.DisplayResult?.Invoke();
            }
            return Task.CompletedTask;
        }

        public Task ShowTaskError()
        {

            return Task.CompletedTask;
        }

    }
}
