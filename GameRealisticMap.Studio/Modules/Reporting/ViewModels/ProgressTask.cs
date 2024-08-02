using System;
using System.Collections.Generic;
using System.Threading;
using NLog;
using Pmad.ProgressTracking;

namespace GameRealisticMap.Studio.Modules.Reporting.ViewModels
{
    internal class ProgressTask : IProgressTaskUI
    {
        private static readonly Logger logger = LogManager.GetLogger("Task");

        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly ProgressToolViewModel viewModel;

        public ProgressTask(ProgressToolViewModel viewModel, string name)
        {
            this.viewModel = viewModel;
            this.TaskName = name;
            Scope = viewModel.ProgressRender.CreateScope(name, 0/*, cancellationTokenSource.Token*/);
            logger.Info("Task '{0}'", name);
        }

        public string TaskName { get; }

        public List<SuccessAction> SuccessActions { get; } = new List<SuccessAction>();

        public IProgressScope Scope { get; }

        //public Exception? Error { get; set; }

        public double ElapsedSeconds => ((ProgressScope)Scope).ElapsedMilliseconds / 1000d;

        //public override IProgressInteger CreateStep(string name, int total)
        //{
        //    var report = new ProgressStep(Prefix + name, total, this);
        //    viewModel.Items.Add(report);
        //    return report;
        //}

        //public override IProgressPercent CreateStepPercent(string name)
        //{
        //    var report = new ProgressStep(Prefix + name, 1000, this);
        //    viewModel.Items.Add(report);
        //    return report;
        //}

        public void Dispose()
        {
            Scope.Dispose();

            var scope = (ProgressScope)Scope;
            /*if (Error != null)
            {
                WriteLine($"ERROR: {Error.Message}");
                WriteLine($"Task FAILED after {elapsed.ElapsedMilliseconds / 1000d:0.0} seconds");
                return;
            }*/

            if (cancellationTokenSource.IsCancellationRequested)
            {
                Scope.WriteLine($"Task CANCELED after {scope.ElapsedMilliseconds / 1000d:0.0} seconds");
                viewModel.State = TaskState.Canceled;
                return;
            }
            Scope.WriteLine($"Task done SUCCESSFULLY in {scope.ElapsedMilliseconds / 1000d:0.0} seconds");
            viewModel.State = TaskState.Done;
        }

        public void Cancel()
        {
            cancellationTokenSource.Cancel();
            viewModel.State = TaskState.Canceling;
            logger.Info("Cancel requested");
        }

        //public void ReportOneDone()
        //{
        //    ++lastDone;
        //    UpdateProgress(lastDone);
        //}

        //public void Report(int done)
        //{
        //    lastDone = done;
        //    UpdateProgress(done);
        //}

        //private void UpdateProgress(int done)
        //{
        //    if (Total == 0)
        //    {
        //        viewModel.Percent = 0;
        //    }
        //    else
        //    {
        //        viewModel.Percent = done * 100.0 / Total;
        //    }
        //    viewModel.NotifyOfPropertyChange(nameof(viewModel.Percent));
        //}

        //public void Failed(Exception e)
        //{
        //    viewModel.State = TaskState.Failed;
        //    Error = e;
        //    viewModel.WriteLine($"Exception: {e}");
        //    logger.Error(e);
        //}

        //public override void WriteLine(string message)
        //{
        //    logger.Debug(message);
        //    viewModel.WriteLine(message);
        //}

        public void AddSuccessAction(Action action, string label, string description = "")
        {
            SuccessActions.Add(new SuccessAction(action, label, description));
        }
    }
}
