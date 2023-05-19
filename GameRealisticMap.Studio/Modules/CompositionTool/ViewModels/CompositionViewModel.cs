using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.TerrainBuilder;

namespace GameRealisticMap.Studio.Modules.CompositionTool.ViewModels
{
    internal class CompositionViewModel
    {
        private readonly Composition composition;

        public CompositionViewModel(Composition composition)
        {
            this.composition = composition;
        }

        public Composition ToDefinition()
        {
            return composition;
        }

        public ModelInfo? SingleModel => composition.Objects.Count == 1 ? composition.Objects[0].Model : null;

        public string Name => string.Join(", ", composition.Objects.Select(o => o.Model.Name));
    }
}
