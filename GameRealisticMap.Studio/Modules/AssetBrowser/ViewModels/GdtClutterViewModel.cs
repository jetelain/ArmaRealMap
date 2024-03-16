using System;
using Caliburn.Micro;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.GameEngine.Materials;
using GameRealisticMap.Arma3.TerrainBuilder;
using GameRealisticMap.Studio.Modules.CompositionTool.Behaviors;
using GameRealisticMap.Studio.Modules.CompositionTool.ViewModels;

namespace GameRealisticMap.Studio.Modules.AssetBrowser.ViewModels
{
    internal class GdtClutterViewModel : PropertyChangedBase
    {
        private double _scaleMin;
        private double _scaleMax;
        private double _probability;
        private double _affectedByWind;
        private bool _isSwLighting;

        public GdtClutterViewModel(ClutterConfig config) 
        {
            _scaleMin = config.ScaleMin;
            _scaleMax = config.ScaleMax;
            _probability = config.Probability;
            _affectedByWind = config.AffectedByWind;
            _isSwLighting = config.IsSwLighting;
            Name = config.Name;
            Model = config.Model;
            Composition = new CompositionViewModel(new Arma3.Assets.Composition(new Arma3.Assets.CompositionObject(Model, System.Numerics.Matrix4x4.Identity)));

        }

        public string? Name { get; }

        public ModelInfo Model { get; }

        public CompositionViewModel Composition { get; }

        public double ScaleMin
        {
            get { return _scaleMin; }
            set { Set(ref _scaleMin, value); }
        }

        public double ScaleMax
        {
            get { return _scaleMax; }
            set { Set(ref _scaleMax, value); }
        }

        public double Probability
        {
            get { return _probability; }
            set { Set(ref _probability, value); }
        }

        public double AffectedByWind
        {
            get { return _affectedByWind; }
            set { Set(ref _affectedByWind, value); }
        }
    
        public bool IsSwLighting
        {
            get { return _isSwLighting; }
            set { Set(ref _isSwLighting, value); }
        }

        internal ClutterConfig ToDefinition()
        {
            return new ClutterConfig(Name, Probability, Model, AffectedByWind, IsSwLighting, ScaleMin, ScaleMax);
        }
    }
}
