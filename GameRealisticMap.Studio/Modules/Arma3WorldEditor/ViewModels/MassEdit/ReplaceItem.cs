using System;
using System.Linq;
using Caliburn.Micro;
using GameRealisticMap.Arma3.Edit;

namespace GameRealisticMap.Studio.Modules.Arma3WorldEditor.ViewModels.MassEdit
{
    public class ReplaceItem : PropertyChangedBase
    {
        private ReplaceViewModel? owner;
        private string _source = string.Empty;
        private string _target = string.Empty;
        private double? _shiftX;
        private double? _shiftY;
        private double? _shiftZ;

        public string Source { get { return _source; } set { if (value != _source ) { _source = value; NotifyOfPropertyChange(); NotifyOfPropertyChange(nameof(Count)); } } }

        public string Target { get { return _target; } set { if (value != _target) { _target = value; NotifyOfPropertyChange(); } } }

        public string ShiftX { get { return _shiftX?.ToString() ?? string.Empty; } set { var nValue = Parse(value); if (nValue != _shiftX) { _shiftX = nValue; NotifyOfPropertyChange(); } } }
        
        public string ShiftY { get { return _shiftY?.ToString() ?? string.Empty; } set { var nValue = Parse(value); if (nValue != _shiftY) { _shiftY = nValue; NotifyOfPropertyChange(); } } }
        
        public string ShiftZ { get { return _shiftZ?.ToString() ?? string.Empty; } set { var nValue = Parse(value); if (nValue != _shiftZ) { _shiftZ = nValue; NotifyOfPropertyChange(); } } }
        
        private static double? Parse(string value)
        {
            if (!string.IsNullOrEmpty(value) && double.TryParse(value, out var result))
            {
                return result;
            }
            return null;
        }

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

        internal WrpMassReplace ToOperation()
        {
            return new WrpMassReplace(Source, Target)
            {
                XShift = _shiftX,
                YShift = _shiftY,
                ZShift = _shiftZ
            };
        }
    }
}