using System.Numerics;
using Caliburn.Micro;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Studio.Modules.CompositionTool.ViewModels;

namespace GameRealisticMap.Studio.Modules.AssetConfigEditor.ViewModels.Individual
{
    internal class BuildingItem : PropertyChangedBase, IWithComposition, IWithCompositionRectangle
    {
        public BuildingItem(BuildingDefinition d)
        {
            _width = d.Size.X;
            _depth = d.Size.Y;
            Composition = new CompositionViewModel(d.Composition);
        }

        private float _width;
        public float Width { get { return _width; } set { _width = value; NotifyOfPropertyChange(); } }

        private float _depth;
        public float Depth { get { return _depth; } set { _depth = value; NotifyOfPropertyChange(); } }

        public CompositionViewModel Composition { get; }

        internal BuildingDefinition ToDefinition()
        {
            return new BuildingDefinition(new Vector2(_width, _depth), Composition.ToDefinition());
        }
    }
}