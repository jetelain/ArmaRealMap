using System;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
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

        private readonly IOutput output;
        private readonly IShell shell;
        private readonly IWindowManager windowManager;

        public GrmProgressRender ProgressRender { get; }

        private readonly Dispatcher dispatcher;
        private readonly PerformanceCounter? cpuCounter;
        private readonly DispatcherTimer timer;
        private double memoryPeak;
        private SuccessViewModel? lastSuccess;
        private bool isRunning;
        private readonly double totalMemGb;

        public override PaneLocation PreferredLocation => PaneLocation.Right;

        public ObservableCollection<ProgressTask> Tasks { get; } = new ObservableCollection<ProgressTask>();

        [ImportingConstructor]
        public ProgressToolViewModel(IOutput output, IShell shell, IWindowManager windowManager)
        {
            DisplayName = Labels.TaskProgress;
            this.output = output;
            this.shell = shell;
            this.windowManager = windowManager;

            ProgressRender = new GrmProgressRender(output);

            dispatcher = Application.Current.Dispatcher;


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

        public bool IsRunning
        {
            get { return isRunning; }
            set
            {
                if (Set(ref isRunning, value))
                {
                    timer.IsEnabled = value;
                }
            }
        }

        public void UpdateIsRunning()
        {
            IsRunning = ProgressRender.RootItem.Children.Any(c => c.IsRunning);
        }

        public float CpuUsage { get; private set; }

        public double MemPressure { get; private set; }

        public string MemUsage { get; private set; } = string.Empty;

        public IProgressTaskUI StartTask(string name)
        {
            output.Clear();
            memoryPeak = 0;

            IsRunning = true;
            var task = new ProgressTask(this, name);
            dispatcher.BeginInvoke(() => Tasks.Add(task));
            return task;
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
            if (lastSuccess != null)
            {
                new System.Action(() => lastSuccess?.TryCloseAsync(false)).BeginOnUIThread();
            }

            var task = (ProgressTask)StartTask(name);
            try
            {
                await run(task);
            }
            catch (Exception ex)
            {
                task.Scope.Failed(ex);
            }
            finally
            {
                task.Scope.WriteLine(FormattableString.Invariant($"Memory: Peak: {memoryPeak:0.000} G, System: {totalMemGb:0.000} G"));
                task.Done();
            }
            if (prompt && task.Error == null && !task.Scope.CancellationToken.IsCancellationRequested)
            {
                new System.Action(() => windowManager.ShowDialogAsync(lastSuccess = new SuccessViewModel(task))).BeginOnUIThread();
            }
        }

        internal async Task ShowTaskResult(ProgressTask progressTask)
        {
            if (lastSuccess != null)
            {
                await lastSuccess.TryCloseAsync(false);
            }
            await windowManager.ShowDialogAsync(lastSuccess = new SuccessViewModel(progressTask));
        }

        internal Task ShowTaskError(ProgressTask progressTask)
        {
            shell.ShowTool(output);
            return Task.CompletedTask;
        }
    }
}
