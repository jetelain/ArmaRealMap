using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Threading;
using Caliburn.Micro;
using Gemini.Framework;
using Gemini.Framework.Services;
using Gemini.Modules.Output;
using NLog;

namespace GameRealisticMap.Studio.Modules.Reporting.ViewModels
{
    [Export(typeof(IProgressTool))]
    internal class ProgressToolViewModel : Tool, IProgressTool
    {
        private static readonly Logger logger = NLog.LogManager.GetLogger("ProgressTool");

        private TaskState state =  TaskState.None;
        private ProgressTask? current = null;
        private readonly IOutput output;
        private readonly IShell shell;
        private readonly IWindowManager windowManager;
        private readonly PerformanceCounter? cpuCounter;
        private readonly DispatcherTimer timer;
        private double memoryPeak;
        private readonly double totalMemGb;

        public override PaneLocation PreferredLocation => PaneLocation.Right;

        public BindableCollection<ProgressStep> Items { get; set; } = new BindableCollection<ProgressStep>();

        [ImportingConstructor]
        public ProgressToolViewModel(IOutput output, IShell shell, IWindowManager windowManager)
        {
            DisplayName = Labels.TaskProgress;
            this.output = output;
            this.shell = shell;
            this.windowManager = windowManager;

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(500);
            timer.Tick += Timer_Tick;
            try
            {
                totalMemGb = SystemNative.GetTotalMemoryInGigaBytes();
                cpuCounter = new PerformanceCounter("Process", "% Processor Time", Assembly.GetEntryAssembly()?.GetAssemblyName() ?? "_Total", true);
            }
            catch(Exception ex)
            {
                logger.Warn(ex);
            }
        }

        public bool HasPerformanceCounter => cpuCounter != null;

        private void Timer_Tick(object? sender, EventArgs e)
        {
            if (cpuCounter != null)
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
            try
            {
                var memoryGB = Process.GetCurrentProcess().PrivateMemorySize64 / 1024 / 1024 / 1024.0;
                MemUsage = $"{memoryGB:0.0} G";
                NotifyOfPropertyChange(nameof(MemUsage));
                memoryPeak = Math.Max(memoryGB, memoryPeak);
                if (totalMemGb > 0 )
                {
                    MemPressure = memoryGB / totalMemGb * 100.0;
                    NotifyOfPropertyChange(nameof(MemPressure));
                }
            }
            catch { }
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

        public double MemPressure { get; private set; }

        public string MemUsage { get; private set; } = string.Empty;

        public IProgressTaskUI StartTask(string name)
        {
            output.Clear();
            Items.Clear();

            Percent = 0;
            NotifyOfPropertyChange(nameof(Percent));

            TaskName = name;
            NotifyOfPropertyChange(nameof(TaskName));

            State = TaskState.Running;

            memoryPeak = 0;

            return current = new ProgressTask(this, name);
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
                return windowManager.ShowDialogAsync(new SuccessViewModel(current));
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

        public Task? RunTask(string name, Func<IProgressTaskUI, Task> run, bool prompt = true)
        {
            if (!IsRunning)
            {
                shell.ShowTool(this);
                return Task.Run(() => DoRunTask(name, run, prompt));
            }
            return null;
        }

        private async Task DoRunTask(string name, Func<IProgressTaskUI, Task> run, bool prompt)
        {
            using var task = (ProgressTask)StartTask(name);
            try
            {
                await run(task);
            }
            catch (Exception ex)
            {
                task.Failed(ex);
            }
            task.WriteLine(FormattableString.Invariant($"Memory: Peak: {memoryPeak:0.000} G, System: {totalMemGb:0.000} G"));
            task.Dispose();
            if (prompt && task.Error == null && !task.CancellationToken.IsCancellationRequested)
            {
                new System.Action(() => windowManager.ShowDialogAsync(new SuccessViewModel(task))).BeginOnUIThread();
            }
        }

    }
}
