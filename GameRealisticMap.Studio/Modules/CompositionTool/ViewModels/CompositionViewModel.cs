using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.TerrainBuilder;

namespace GameRealisticMap.Studio.Modules.CompositionTool.ViewModels
{
    internal class CompositionViewModel
    {
        private readonly Composition composition;
        private bool wasEdited = false;
        private ObservableCollection<CompositionItem>? _items;

        public CompositionViewModel(Composition composition)
        {
            this.composition = composition;
        }

        public Composition ToDefinition()
        {
            if (_items != null && _items.Any(i => i.WasEdited))
            {
                return new Composition(_items.Select(i => i.ToDefinition()).ToList());
            }
            return composition;
        }
        public ObservableCollection<CompositionItem> Items
        {
            get
            {
                if (_items == null)
                {
                    _items = new ObservableCollection<CompositionItem>(
                        composition.Objects.Select(o => new CompositionItem(o, this)));
                }
                return _items;
            }
        }


        public ModelInfo? SingleModel => composition.Objects.Count == 1 ? composition.Objects[0].Model : null;

        public string Name => string.Join(", ", composition.Objects.Select(o => o.Model.Name));

        public bool IsEmpty => composition.Objects.Count == 0;
    }
}
