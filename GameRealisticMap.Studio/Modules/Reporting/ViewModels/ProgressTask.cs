using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using Pmad.ProgressTracking;
using Pmad.ProgressTracking.Wpf;

namespace GameRealisticMap.Studio.Modules.Reporting.ViewModels
{
    internal class ProgressTask : Caliburn.Micro.PropertyChangedBase, IProgressTaskUI
    {
        private static readonly Logger logger = LogManager.GetLogger("Task");

        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly ProgressToolViewModel viewModel;
        private readonly ProgressScope scope;
        private TaskState state = TaskState.Running;

        public ProgressItemViewModel ScopeViewModel { get; }

        public ProgressTask(ProgressToolViewModel viewModel, string name)
        {
            this.viewModel = viewModel;
            this.TaskName = name;
            scope = (ProgressScope)viewModel.ProgressRender.CreateScope(name, 0, cancellationTokenSource.Token);
            ScopeViewModel = viewModel.ProgressRender.GetViewModel(scope);
            logger.Info("Task '{0}' (#{1})", name, scope.Id);
            ScopeViewModel.PropertyChanged += OnPropertyChanged;
        }

        private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IsRunning))
            {
                NotifyOfPropertyChange(nameof(IsRunning));
            }
        }

        public string TaskName { get; }

        public List<SuccessAction> SuccessActions { get; } = new List<SuccessAction>();

        public IProgressScope Scope => scope;

        public Exception? Error => ((ProgressScope)Scope).Error;

        public double ElapsedSeconds => ((ProgressScope)Scope).ElapsedMilliseconds / 1000d;

        public ObservableCollection<ProgressItemViewModel> Children => ScopeViewModel.Children;

        public bool IsRunning => ScopeViewModel.IsRunning;

        public TaskState State
        {
            get { return state; }
            set
            {
                Set(ref state, value);
            }
        }

        public void Done()
        {
            scope.Dispose();
            viewModel.UpdateIsRunning();

            if (scope.Error != null)
            {
                scope.WriteLine($"ERROR: {scope.Error.Message}");
                scope.WriteLine($"Task FAILED after {scope.ElapsedMilliseconds / 1000d:0.0} seconds");
                State = TaskState.Failed;
                return;
            }
            if (cancellationTokenSource.IsCancellationRequested)
            {
                scope.WriteLine($"Task CANCELED after {scope.ElapsedMilliseconds / 1000d:0.0} seconds");
                State = TaskState.Canceled;
                return;
            }
            scope.WriteLine($"Task done SUCCESSFULLY in {scope.ElapsedMilliseconds / 1000d:0.0} seconds");
            State = TaskState.Done;
        }

        public void Cancel()
        {
            cancellationTokenSource.Cancel();
            logger.Info("Cancel requested");
            State = TaskState.Canceling;
        }

        public void Failed(Exception e)
        {
            Scope.Failed(e);
        }

        public void AddSuccessAction(Action action, string label, string description = "")
        {
            SuccessActions.Add(new SuccessAction(action, label, description));
        }

        public Task CancelTask()
        {
            Cancel();
            return Task.CompletedTask;
        }

        public Task ShowTaskResult()
        {
            return viewModel.ShowTaskResult(this);
        }

        public Task ShowTaskError()
        {
            return viewModel.ShowTaskError(this);
        }

    }
}
