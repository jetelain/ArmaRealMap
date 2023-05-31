using System;
using System.Diagnostics;
using System.Threading;
using GameRealisticMap.Reporting;
using Gemini.Modules.Output;

namespace GameRealisticMap.Studio.Modules.Reporting.ViewModels
{
    internal class ProgressTask : ProgressSystemBase, IProgressTaskUI
    {
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly ProgressToolViewModel viewModel;
        private readonly Stopwatch elapsed;
        private readonly IOutput output;
        private int lastDone = 0;

        public ProgressTask(ProgressToolViewModel viewModel, IOutput output)
        {
            this.output = output;
            this.viewModel = viewModel;
            this.elapsed = Stopwatch.StartNew();
        }

        public int Total { get; set; } = 0;

        public CancellationToken CancellationToken => cancellationTokenSource.Token;

        public Action? DisplayResult { get; set; }

        public Exception? Error { get; set; }

        public override IProgressInteger CreateStep(string name, int total)
        {
            var report = new ProgressStep(Prefix + name, total, this);
            viewModel.Items.Add(report);
            return report;
        }

        public override IProgressPercent CreateStepPercent(string name)
        {
            var report = new ProgressStep(Prefix + name, 1000, this);
            viewModel.Items.Add(report);
            return report;
        }

        public void Dispose()
        {
            elapsed.Stop();
            if (Error != null)
            {
                WriteLine($"ERROR: {Error.Message}");
                WriteLine($"Task FAILED after {elapsed.ElapsedMilliseconds / 1000d:0.0} seconds");
                return;
            }
            if (cancellationTokenSource.IsCancellationRequested)
            {
                WriteLine($"Task CANCELED after {elapsed.ElapsedMilliseconds / 1000d:0.0} seconds");
                viewModel.State = TaskState.Canceled;
                return;
            }
            WriteLine($"Task done SUCCESSFULLY in {elapsed.ElapsedMilliseconds/1000d:0.0} seconds");
            viewModel.State = TaskState.Done;
            UpdateProgress(Total);
        }

        public void Cancel()
        {
            cancellationTokenSource.Cancel();
            viewModel.State = TaskState.Canceling;
        }

        public void ReportOneDone()
        {
            ++lastDone;
            UpdateProgress(lastDone);
        }

        public void Report(int done)
        {
            lastDone = done;
            UpdateProgress(done);
        }

        private void UpdateProgress(int done)
        {
            if (Total == 0)
            {
                viewModel.Percent = 0;
            }
            else
            {
                viewModel.Percent = done * 100.0 / Total;
            }
            viewModel.NotifyOfPropertyChange(nameof(viewModel.Percent));
        }

        public void Failed(Exception e)
        {
            viewModel.State = TaskState.Failed;
            Error = e;
            WriteLine($"Exception: {e}");
        }

        public override void WriteLine(string message)
        {
            viewModel.WriteLine(message);
        }
    }
}
