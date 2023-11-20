using System;
using System.Linq;
using Caliburn.Micro;

namespace GameRealisticMap.Studio.Modules.Arma3WorldEditor.ViewModels.MassEdit
{
    public class ReplaceItem : PropertyChangedBase
    {
        private ReplaceViewModel? owner;

        private string _source = string.Empty;

        private string _target = string.Empty;

        public string Source { get { return _source; } set { if (value != _source ) { _source = value; NotifyOfPropertyChange(); NotifyOfPropertyChange(nameof(Count)); } } }

        public string Target { get { return _target; } set { if (value != _target) { _target = value; NotifyOfPropertyChange(); } } }

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

        internal void Attach(ReplaceViewModel replaceViewModel)
        {
            if (owner == null)
            {
                owner = replaceViewModel;
                NotifyOfPropertyChange(nameof(Count));
            }
        }
    }
}