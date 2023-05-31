using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using Caliburn.Micro;
using CoordinateSharp.Debuggers;
using Gemini.Framework;
using Gemini.Framework.Services;
using Gemini.Modules.Output;

namespace GameRealisticMap.Studio.Modules.Reporting.ViewModels
{
    [Export(typeof(IProgressTool))]
    internal class ProgressToolViewModel : Tool, IProgressTool
    {
        private TaskState state =  TaskState.None;
        private ProgressTask? current = null;
        private readonly IOutput output;
        private readonly IShell shell;

        public override PaneLocation PreferredLocation => PaneLocation.Right;

        public BindableCollection<ProgressStep> Items { get; set; } = new BindableCollection<ProgressStep>();

        [ImportingConstructor]
        public ProgressToolViewModel(IOutput output, IShell shell)
        {
            DisplayName = "Task Progress";
            this.output = output;
            this.shell = shell;
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
            output.Clear();
            Items.Clear();

            Percent = 0;
            NotifyOfPropertyChange(nameof(Percent));

            TaskName = name;
            NotifyOfPropertyChange(nameof(TaskName));

            State = TaskState.Running;

            return current = new ProgressTask(this, output);
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
            shell.ShowTool(output);
            return Task.CompletedTask;
        }

        internal void WriteLine(string message)
        {
            OnUIThread(() => output.AppendLine(message ?? string.Empty));
        }
    }
}
