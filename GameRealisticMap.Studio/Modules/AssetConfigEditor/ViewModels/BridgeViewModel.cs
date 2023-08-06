using Caliburn.Micro;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.Assets.Detection;
using GameRealisticMap.Studio.Modules.CompositionTool.ViewModels;

namespace GameRealisticMap.Studio.Modules.AssetConfigEditor.ViewModels
{
    internal class BridgeViewModel : PropertyChangedBase, IWithComposition, IWithCompositionRectangle, IModelImporterTarget
    {
        public BridgeViewModel(string label, StraightSegmentDefinition? d)
        {
            _size = d?.Size ?? 0;
            Composition = new CompositionViewModel(d?.Model ?? new Composition());
            this.Label = label;
            CompositionImporter = new CompositionImporter(this);
        }

        public string Label { get; }

        public CompositionImporter CompositionImporter { get; }

        public float Depth { get => Size; set => Size = value; }

        public float Width { get => 200; set { } }

        public void AddComposition(Composition composition, ObjectPlacementDetectedInfos detected)
        {
            Composition = new CompositionViewModel(composition.Translate(-detected.GeneralRadius.Center));
            NotifyOfPropertyChange(nameof(Composition));
            Size = detected.GeneralRadius.Radius;
        }

        internal void Clear()
        {
            Composition = new CompositionViewModel(new Composition());
            NotifyOfPropertyChange(nameof(Composition));
            Size = 0;
        } 

        private float _size;
        public float Size
        {
            get { return _size; }
            set { _size = value; NotifyOfPropertyChange(); NotifyOfPropertyChange(nameof(Width)); NotifyOfPropertyChange(nameof(Depth)); }
        }

        public CompositionViewModel Composition { get; protected set; }

        internal StraightSegmentDefinition ToDefinition()
        {
            return new StraightSegmentDefinition(Composition.ToDefinition(), Size);
        }

        public void CompositionWasRotated(int degrees)
        {
            // Nothing to do
        }
    }
}
