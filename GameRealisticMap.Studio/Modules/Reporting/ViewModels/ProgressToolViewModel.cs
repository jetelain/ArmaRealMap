using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Threading;
using Caliburn.Micro;
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
        private readonly PerformanceCounter cpuCounter;
        private readonly DispatcherTimer timer;

        public override PaneLocation PreferredLocation => PaneLocation.Right;

        public BindableCollection<ProgressStep> Items { get; set; } = new BindableCollection<ProgressStep>();

        [ImportingConstructor]
        public ProgressToolViewModel(IOutput output, IShell shell)
        {
            DisplayName = "Task Progress";
            this.output = output;
            this.shell = shell;

            cpuCounter = new PerformanceCounter("Process", "% Processor Time",  Assembly.GetEntryAssembly()?.GetAssemblyName() ?? "_Total", true);
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(500);
            timer.Tick += Timer_Tick;
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            try
            {
                CpuUsage = cpuCounter.NextValue() / Environment.ProcessorCount;
                NotifyOfPropertyChange(nameof(CpuUsage));
            }
            catch (InvalidOperationException)
            {
            }
        }

        public TaskState State
        {
            get { return state; }
            set
            {
                if (value != state)
                {
                    state = value;
                    NotifyOfPropertyChange();
                    if (state == TaskState.Running)
                    {
                        if (!timer.IsEnabled)
                        {
                            timer.Start();
                        }
                    }
                    else if ( timer.IsEnabled )
                    {
                        timer.Stop();
                    }
                }
            }
        }

        public double Percent { get; set; }

        public string TaskName { get; set; } = string.Empty;

        public bool IsRunning => state == TaskState.Running || state == TaskState.Canceling;

        public float CpuUsage { get; private set; }

        public IProgressTaskUI StartTask(string name)
        {
            output.Clear();
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
            shell.ShowTool(output);
            return Task.CompletedTask;
        }

        internal void WriteLine(string message)
        {
            OnUIThread(() => output.AppendLine(message ?? string.Empty));
        }

        public void RunTask(string name, Func<IProgressTaskUI, Task> run)
        {
            if (!IsRunning)
            {
                shell.ShowTool(this);
                _ = Task.Run(() => DoRunTask(name, run));
            }
        }

        private async Task DoRunTask(string name, Func<IProgressTaskUI, Task> run)
        {
            using var task = StartTask(name);
            try
            {
                await run(task);
            }
            catch (Exception ex)
            {
                task.Failed(ex);
            }
        }
    }
}
