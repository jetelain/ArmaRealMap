using System;
using System.Diagnostics;
using System.Threading;
using GameRealisticMap.Reporting;

namespace GameRealisticMap.Studio.Modules.Reporting.ViewModels
{
    internal class ProgressTask : ProgressSystemBase, IProgressTaskUI
    {
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly ProgressToolViewModel viewModel;
        private readonly Stopwatch elapsed;
        private int lastDone = 0;

        public ProgressTask(ProgressToolViewModel viewModel)
        {
            this.viewModel = viewModel;
            this.elapsed = Stopwatch.StartNew();
        }

        public int Total { get; set; } = 0;

        public CancellationToken CancellationToken => cancellationTokenSource.Token;

        public Action? DisplayResult { get; set; }

        public Exception? Error { get; set; }

        public override IProgressInteger CreateStep(string name, int total)
        {
            var report = new ProgressStep(Prefix + name, total);
            viewModel.Items.Add(report);
            return report;
        }

        public override IProgressPercent CreateStepPercent(string name)
        {
            var report = new ProgressStep(Prefix + name, 1000);
            viewModel.Items.Add(report);
            return report;
        }

        public void Dispose()
        {
            elapsed.Stop();
            if (Error != null)
            {
                return;
            }
            if (cancellationTokenSource.IsCancellationRequested)
            {
                viewModel.State = TaskState.Canceled;
                return;
            }
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
        }
    }
}
