using System;
using System.Linq;
using System.Numerics;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Studio.Modules.CompositionTool.ViewModels;

namespace GameRealisticMap.Studio.Modules.AssetConfigEditor.ViewModels.Individual
{
    internal class BuildingItem : IWithComposition
    {
        public BuildingItem(BuildingDefinition d)
        {
            Width = d.Size.X;
            Depth = d.Size.Y;
            Composition = new CompositionViewModel(d.Composition);
        }

        public float Width { get; set; }

        public float Depth { get; set; }

        public CompositionViewModel Composition { get; set; }

        internal BuildingDefinition ToDefinition()
        {
            return new BuildingDefinition(new Vector2(Width, Depth), Composition.ToDefinition());
        }
    }
}