using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using Pmad.ProgressTracking;

namespace GameRealisticMap.Studio.Modules.Reporting.ViewModels
{
    public class ProgressItemViewModel2 : INotifyPropertyChanged
    {
        private readonly ProgressBase item;
        private double percentDone;
        public event PropertyChangedEventHandler? PropertyChanged;

        public ObservableCollection<ProgressItemViewModel2> Children { get; } = new ObservableCollection<ProgressItemViewModel2>();

        public ProgressItemViewModel2? Parent { get; private set; }

        internal ProgressItemViewModel2(ProgressBase item)
        {
            this.item = item;
        }

        public string Name => item.Name;

        public bool IsDone => item.IsDone;

        public bool IsRunning => !item.IsDone;

        public bool IsIndeterminate => item.IsIndeterminate;

        public double PercentDone
        {
            get { return percentDone; }
            set
            {
                if (percentDone != value)
                {
                    percentDone = value;
                    NotifyPropertyChanged(nameof(PercentDone));
                }
            }
        }

        public string Status => item.Text ?? item.GetDefaultStatusText();

        internal void AddChild(ProgressItemViewModel2 child)
        {
            Children.Add(child);
            child.Parent = this;
        }

        internal void Finished()
        {
            PercentDone = 100.0;
            NotifyPropertyChanged(nameof(IsDone));
            NotifyPropertyChanged(nameof(IsRunning));
            NotifyPropertyChanged(nameof(Status));
            NotifyPropertyChanged(nameof(IsIndeterminate));
            Parent?.UpdatePercent();
        }

        internal void UpdatePercent()
        {
            PercentDone = item.PercentDone;
            if (string.IsNullOrEmpty(item.Text) && item.IsTimeLinear)
            {
                NotifyPropertyChanged(nameof(Status));
            }
        }

        internal void TextChanged()
        {
            NotifyPropertyChanged(nameof(Status));
        }

        private void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class WpfProgressRender2 : ProgressRenderBase, IDisposable
    {
        private readonly ConcurrentDictionary<int, ProgressItemViewModel2> table = new ConcurrentDictionary<int, ProgressItemViewModel2>();
        protected readonly Dispatcher dispatcher;
        private readonly DispatcherTimer timer;

        public WpfProgressRender2(CancellationToken cancellationToken = default)
            : this(Application.Current.Dispatcher, cancellationToken)
        {

        }

        public ProgressItemViewModel2 RootItem { get; }

        public WpfProgressRender2(Dispatcher dispatcher, CancellationToken cancellationToken = default)
            : base(cancellationToken)
        {
            this.dispatcher = dispatcher;
            this.RootItem = GetViewModel(Root);
            this.timer = new DispatcherTimer(TimeSpan.FromMilliseconds(500), DispatcherPriority.Normal, Update, dispatcher);
        }

        private void Update(object? sender, EventArgs e)
        {
            foreach (var item in table.Values)
            {
                if (item.IsRunning)
                {
                    item.UpdatePercent();
                }
            }
        }

        private ProgressItemViewModel2 GetViewModel(ProgressBase item)
        {
            return table.GetOrAdd(item.Id, _ => new ProgressItemViewModel2(item));
        }

        public override void Finished(ProgressBase progressBase)
        {
            GetViewModel(progressBase).Finished();
            UpdateTimer();
        }

        public override void PercentChanged(ProgressBase progressBase)
        {

        }

        public override void Started(ProgressScope progressScope, ProgressBase item)
        {
            var parent = GetViewModel(progressScope);
            var child = GetViewModel(item);
            dispatcher.BeginInvoke(() => parent.AddChild(child));
            UpdateTimer();
        }

        private void UpdateTimer()
        {
            lock (timer)
            {
                var wantedState = table.Values.Any(t => t != RootItem && t.IsRunning);
                if (wantedState != timer.IsEnabled)
                {
                    timer.IsEnabled = wantedState;
                }
            }
        }

        public override void TextChanged(ProgressBase progressBase)
        {
            GetViewModel(progressBase).TextChanged();
        }

        public override void WriteLine(ProgressBase progressBase, string message)
        {
            dispatcher.BeginInvoke(() => WriteLine(GetViewModel(progressBase), message));
        }

        protected virtual void WriteLine(ProgressItemViewModel2 progressItemViewModel, string message)
        {

        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            lock (timer)
            {
                if (timer.IsEnabled)
                {
                    timer.IsEnabled = false;
                }
            }
        }
    }
}
