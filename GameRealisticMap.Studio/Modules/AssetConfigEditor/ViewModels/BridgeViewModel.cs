using System;
using System.Linq;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.Assets.Detection;
using GameRealisticMap.Arma3.TerrainBuilder;
using GameRealisticMap.Studio.Modules.AssetConfigEditor.ViewModels.Filling;
using GameRealisticMap.Studio.Modules.CompositionTool.ViewModels;

namespace GameRealisticMap.Studio.Modules.AssetConfigEditor.ViewModels
{
    internal class BridgeViewModel : FenceItem, IModelImporterTarget
    {
        public BridgeViewModel(string label, StraightSegmentDefinition? d)
            : base(d ?? new StraightSegmentDefinition(new Composition(), 0))
        {
            this.Label = label;
            CompositionImporter = new CompositionImporter(this);
        }

        public string Label { get; }

        public CompositionImporter CompositionImporter { get; }

        public override float Depth { get => Size; set => Size = value; }

        public override float Width { get => 200; set { } }

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
    }
}