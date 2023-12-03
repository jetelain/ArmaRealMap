using System;
using Caliburn.Micro;
using GameRealisticMap.Algorithms.RandomGenerators;
using static GameRealisticMap.Algorithms.RandomGenerators.FastNoiseLite;

namespace GameRealisticMap.Studio.Modules.DensityConfigEditor.ViewModels
{
    public sealed class DensityConfigVM : PropertyChangedBase, IRandomPointGenerationOptions, INoiseBasedRandomPointOptions
    {
        private float _threshold;
        private int _samples;
        private float _frequency;
        private NoiseType _noiseType;
        private double _proportion;
        private int? _seed;
        private double _minDensity;
        private double _maxDensity;
        private double _noiseMinDensity;
        private double _noiseMaxDensity;
        private bool _noiseUseDefault;

        public DensityConfigVM(IRandomPointGenerationOptions options)
        {
            var noiseOptions = options.NoiseOptions ?? NoiseBasedRandomPointOptions.Default;
            _threshold = noiseOptions.Threshold;
            _frequency = noiseOptions.Frequency;
            _noiseType = noiseOptions.NoiseType;
            _samples = noiseOptions.Samples;
            _proportion = options.NoiseProportion;
            NoiseTypes = Enum.GetValues<NoiseType>();
            _minDensity = _maxDensity = _noiseMinDensity = _noiseMaxDensity = 0.01d;
            _noiseUseDefault = (_maxDensity == _noiseMaxDensity) && (_minDensity == _noiseMinDensity);
        }

        public NoiseType[] NoiseTypes { get; }

        public double MinDensity
        {
            get { return _minDensity; }
            set
            {
                if (_minDensity != value)
                {
                    _minDensity = value; NotifyOfPropertyChange(); if (_noiseUseDefault)
                    { NoiseMinDensity = MinDensity; }
                    Update();
                }
            }
        }

        public double MaxDensity
        {
            get { return _maxDensity; }
            set
            {
                if (_maxDensity != value)
                {
                    _maxDensity = value; NotifyOfPropertyChange(); if (_noiseUseDefault)
                    { NoiseMaxDensity = MaxDensity; }
                    Update();
                }
            }
        }

        public double NoiseMinDensity
        {
            get { return _noiseMinDensity; }
            set
            {
                if (_noiseMinDensity != value)
                {
                    _noiseMinDensity = value;
                    NotifyOfPropertyChange();
                    if (_noiseMinDensity != _minDensity)
                    {
                        NoiseUseDefault = false;
                    }
                    Update();
                }
            }
        }

        public double NoiseMaxDensity
        {
            get { return _noiseMaxDensity; }
            set
            {
                if (_noiseMaxDensity != value)
                {
                    _noiseMaxDensity = value;
                    NotifyOfPropertyChange();
                    if (_noiseMaxDensity != _maxDensity)
                    {
                        NoiseUseDefault = false;
                    }
                    Update();
                }
            }
        }

        public float Threshold { get { return _threshold; } set { if (_threshold != value) { _threshold = Math.Clamp(value, -1f, 1f); NotifyOfPropertyChange(); Update(); } } }

        public bool NoiseUseDefault
        {
            get { return _noiseUseDefault; }
            set
            {
                if (_noiseUseDefault != value)
                {
                    _noiseUseDefault = value;
                    NotifyOfPropertyChange();
                    NotifyOfPropertyChange(nameof(NoiseUseSpecific));
                    if (_noiseUseDefault)
                    {
                        NoiseMinDensity = MinDensity;
                        NoiseMaxDensity = MaxDensity;
                    }
                }
            }
        }

        public bool NoiseUseSpecific { get { return !NoiseUseDefault; } set { NoiseUseDefault = !value; } }

        public int Samples { get { return _samples; } set { if (_samples != value) { _samples = Math.Max(2, value); NotifyOfPropertyChange(); } } }

        public int Seed { get { return _seed ?? 0; } set { if (_seed != value) { _seed = value; NotifyOfPropertyChange(); } } }

        public bool HasNoise => _proportion > 0;

        public float Frequency { get { return _frequency; } set { if (_frequency != value) { _frequency = value; NotifyOfPropertyChange(); } } }

        public NoiseType NoiseType { get { return _noiseType; } set { if (_noiseType != value) { _noiseType = value; NotifyOfPropertyChange(); Update(); } } }

        public double NoiseProportion { get { return _proportion; } set { if (_proportion != value) { _proportion = Math.Clamp(value, 0d, 1d); NotifyOfPropertyChange(); NotifyOfPropertyChange(nameof(HasNoise)); Update(); } } }

        public INoiseBasedRandomPointOptions? NoiseOptions => this;

        int? INoiseOptions.Seed => _seed;

        public bool IsBasic => _proportion == 0 && (_noiseUseDefault || (_maxDensity == _noiseMaxDensity) && (_minDensity == _noiseMinDensity));

        public string Label => ToText();

        public string ToText()
        {
            if (_proportion == 0)
            {
                if (_maxDensity == _noiseMaxDensity && _minDensity == _noiseMinDensity)
                {
                    return DensityText(_minDensity, _maxDensity);
                }
                return string.Format("Default {0}, Large areas {1}", 
                    DensityText(_minDensity, _maxDensity), 
                    DensityText(_noiseMinDensity, _noiseMaxDensity));
            }
            return string.Format("Default {0}, Large areas {1} with {2:0}% {3} noise.",
                DensityText(_minDensity, _maxDensity),
                DensityText(_noiseMinDensity, _noiseMaxDensity),
                NoiseProportion * 100,
                NoiseType);
        }

        private static string DensityText(double min, double max)
        {
            if (min == max)
            {
                return $"{max} {Labels.ObjectsPerM2}";
            }
            return string.Format(Labels.RangeObjectsPerM2, min, max);
        }

        private void Update()
        {
            NotifyOfPropertyChange(nameof(IsBasic));
            NotifyOfPropertyChange(nameof(Label));
        }
    }
}
