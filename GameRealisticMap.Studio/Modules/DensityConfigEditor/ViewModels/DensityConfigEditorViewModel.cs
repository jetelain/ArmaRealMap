using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GameRealisticMap.Algorithms.Definitions;
using GameRealisticMap.Algorithms.RandomGenerators;
using GameRealisticMap.Geometries;
using GameRealisticMap.Studio.UndoRedo;
using Gemini.Framework;
using Gemini.Modules.UndoRedo;

namespace GameRealisticMap.Studio.Modules.DensityConfigEditor.ViewModels
{
    internal class DensityConfigEditorViewModel : Document
    {
        private DensityConfigVM _config;
        private int _noisePreviewCount = 1000;
        private int _noisePreviewSize = 0;
        private int _defaultPreviewCount = 1000;
        private int _defaultPreviewSize = 0;

        public DensityConfigEditorViewModel()
            : this(new DensityConfigVM(new DensityWithNoiseDefinition(0.01, 0.01), null))
        {

        }

        public DensityConfigEditorViewModel(DensityConfigVM config)
        {
            _config = config;
            DisplayName = Labels.DensityConfiguration;
        }

        protected override IUndoRedoManager CreateUndoRedoManager()
        {
            return _config.UndoRedoManager ?? base.CreateUndoRedoManager();
        }

        private void ConfigPropertyChanged(object? _, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(DensityConfigVM.Threshold):
                case nameof(DensityConfigVM.Samples):
                case nameof(DensityConfigVM.Seed):
                case nameof(DensityConfigVM.UseRandomSeed):
                case nameof(DensityConfigVM.Frequency):
                case nameof(DensityConfigVM.NoiseType):
                case nameof(DensityConfigVM.NoiseProportion):
                    UpdateNoisePreview();
                    return;
                case nameof(DensityConfigVM.ActualNoiseMinDensity):
                case nameof(DensityConfigVM.ActualNoiseMaxDensity):
                    UpdateNoisePreviewSize();
                    return;
                case nameof(DensityConfigVM.MinDensity):
                case nameof(DensityConfigVM.MaxDensity):
                    UpdateDefaultPreviewSize();
                    return;
            }
        }

        private void UpdateNoisePreviewSize()
        {
            NoisePreviewSize = (int)Math.Sqrt(_noisePreviewCount / ((_config.ActualNoiseMaxDensity + _config.ActualNoiseMinDensity) / 2d));
        }

        private void UpdateDefaultPreviewSize()
        {
            DefaultPreviewSize = (int)Math.Sqrt(_defaultPreviewCount / ((_config.MaxDensity + _config.MinDensity) / 2d));
        }

        public DensityConfigVM Config => _config;

        public List<TerrainPoint> NoisePoints { get; private set; } = new List<TerrainPoint>();

        public List<TerrainPoint> DefaultPoints { get; private set; } = new List<TerrainPoint>();

        public int NoisePreviewCount
        {
            get { return _noisePreviewCount; }
            set
            {
                if (_noisePreviewCount != value)
                {
                    _noisePreviewCount = value;
                    NotifyOfPropertyChange();
                    UpdateNoisePreviewSize();
                }
            }
        }

        public int NoisePreviewSize
        {
            get { return _noisePreviewSize; }
            set
            {
                if (_noisePreviewSize != value)
                {
                    _noisePreviewSize = value;
                    NotifyOfPropertyChange();
                    UpdateNoisePreview();
                }
            }
        }


        public int DefaultPreviewCount
        {
            get { return _defaultPreviewCount; }
            set
            {
                if (_defaultPreviewCount != value)
                {
                    _defaultPreviewCount = value;
                    NotifyOfPropertyChange();
                    UpdateDefaultPreviewSize();
                }
            }
        }

        public int DefaultPreviewSize
        {
            get { return _defaultPreviewSize; }
            set
            {
                if (_defaultPreviewSize != value)
                {
                    _defaultPreviewSize = value;
                    NotifyOfPropertyChange();
                    UpdateDefaultPreview();
                }
            }
        }

        private void UpdateNoisePreview()
        {
            var rpg = RandomPointGenerator.Create(new System.Random(0), new Envelope(TerrainPoint.Empty, new TerrainPoint(NoisePreviewSize, NoisePreviewSize)), Config);
            NoisePoints = Enumerable.Range(0, NoisePreviewCount).Select(_ => rpg.GetRandomPoint()).ToList();
            NotifyOfPropertyChange(nameof(NoisePoints));
        }

        private void UpdateDefaultPreview()
        {
            var rpg = RandomPointGenerator.Create(new System.Random(0), new Envelope(TerrainPoint.Empty, new TerrainPoint(DefaultPreviewSize, DefaultPreviewSize)));
            DefaultPoints = Enumerable.Range(0, DefaultPreviewCount).Select(_ => rpg.GetRandomPoint()).ToList();
            NotifyOfPropertyChange(nameof(DefaultPoints));
        }

        public Task ComputeNoiseDensity()
        {
            var normal = GetUsedSurface(RandomPointGenerator.Create(new System.Random(0), new Envelope(TerrainPoint.Empty, new TerrainPoint(1000, 1000))));
            var noise = GetUsedSurface(RandomPointGenerator.Create(new System.Random(0), new Envelope(TerrainPoint.Empty, new TerrainPoint(1000, 1000)), Config));
            UndoRedoManager.ExecuteAction(new BatchAction(Labels.ComputeDensityFromDefault)
                {
                    new PropertyAction<bool>(true, Config.NoiseUseSpecific, "", value => Config.NoiseUseSpecific = value),
                    new PropertyAction<double>(Math.Round(Config.MinDensity * (noise*noise) / (normal*normal), 6), Config.NoiseMinDensity, "", value => Config.NoiseMinDensity = value),
                    new PropertyAction<double>(Math.Round(Config.MaxDensity * (noise*noise) / (normal*normal), 6), Config.NoiseMaxDensity, "", value => Config.NoiseMaxDensity = value),
                });
            return Task.CompletedTask;
        }

        private static double GetUsedSurface(IRandomPointGenerator rpg, int size = 1000, int sampling = 2)
        {
            var count = size * size;
            var matrix = new bool[(size / sampling) + 1, (size / sampling) + 1];
            var used = 0;
            for (int i = 0; i < count; ++i)
            {
                var p = rpg.GetRandomPoint();
                var x = (int)p.X / sampling;
                var y = (int)p.Y / sampling;
                if (!matrix[x, y])
                {
                    matrix[x, y] = true;
                    used++;
                }
            }
            return (double)used / (double)count;
        }

        protected override Task OnActivateAsync(CancellationToken cancellationToken)
        {
            _config.PropertyChanged -= ConfigPropertyChanged;
            _config.PropertyChanged += ConfigPropertyChanged;
            UpdateNoisePreviewSize();
            UpdateDefaultPreviewSize();
            return base.OnActivateAsync(cancellationToken);
        }

        protected override Task OnDeactivateAsync(bool close, CancellationToken cancellationToken)
        {
            _config.PropertyChanged -= ConfigPropertyChanged;
            return base.OnDeactivateAsync(close, cancellationToken);
        }

    }
}
