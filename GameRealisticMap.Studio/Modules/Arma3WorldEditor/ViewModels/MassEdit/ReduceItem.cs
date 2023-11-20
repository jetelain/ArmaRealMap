using System;
using System.Linq;
using Caliburn.Micro;

namespace GameRealisticMap.Studio.Modules.Arma3WorldEditor.ViewModels.MassEdit
{
    public class ReduceItem : PropertyChangedBase
    {
        private ReduceViewModel? owner;

        private string _source = string.Empty;

        private double _factor = 0.5;

        public string Source { get { return _source; } set { if (value != _source ) { _source = value; NotifyOfPropertyChange(); NotifyOfPropertyChange(nameof(Count)); NotifyOfPropertyChange(nameof(TargetCount)); } } }

        public double Factor { get { return _factor; } set { if (value != _factor) { _factor = value; NotifyOfPropertyChange(); NotifyOfPropertyChange(nameof(Count)); NotifyOfPropertyChange(nameof(TargetCount)); } } }

        public int Count
        {
            get
            {
                if (string.IsNullOrEmpty(_source))
                {
                    return 0;
                }
                return owner?.ParentEditor?.World?.Objects?.Count(o => string.Equals(o.Model, _source, StringComparison.OrdinalIgnoreCase)) ?? 0;
            }
        }

        public int TargetCount => (int)(Count * Factor);

        internal void Attach(ReduceViewModel replaceViewModel)
        {
            if (owner == null)
            {
                owner = replaceViewModel;
                NotifyOfPropertyChange(nameof(Count));
                NotifyOfPropertyChange(nameof(TargetCount));
            }
        }
    }
}