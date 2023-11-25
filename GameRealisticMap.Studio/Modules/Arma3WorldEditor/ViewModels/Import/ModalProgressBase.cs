using System;
using System.Threading.Tasks;
using Gemini.Framework;

namespace GameRealisticMap.Studio.Modules.Arma3WorldEditor.ViewModels.Import
{
    internal abstract class ModalProgressBase : WindowBase, IProgress<double>
    {
        private bool _isWorking;
        private double _workingPercent;
        private string _error = string.Empty;

        public Task Cancel() => TryCloseAsync(false);

        public bool IsWorking
        {
            get { return _isWorking; }
            set { _isWorking = value; NotifyOfPropertyChange(); NotifyOfPropertyChange(nameof(IsNotValid)); NotifyOfPropertyChange(nameof(IsValid)); }
        }

        public double WorkingPercent
        {
            get { return _workingPercent; }
            set { _workingPercent = value; NotifyOfPropertyChange(); }
        }

        public void Report(double value)
        {
            WorkingPercent = value;
        }

        public string Error
        {
            get { return _error; }
            set { _error = value; NotifyOfPropertyChange(); if (!_isWorking) { NotifyOfPropertyChange(nameof(IsNotValid)); NotifyOfPropertyChange(nameof(IsValid)); } }
        }

        public bool IsNotValid => !IsWorking && !string.IsNullOrEmpty(Error);

        public bool IsValid => !IsWorking && string.IsNullOrEmpty(Error);

    }
}
