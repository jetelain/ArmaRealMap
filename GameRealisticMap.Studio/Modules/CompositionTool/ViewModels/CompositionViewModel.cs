using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Caliburn.Micro;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.TerrainBuilder;
using GameRealisticMap.Studio.UndoRedo;
using Gemini.Modules.UndoRedo;

namespace GameRealisticMap.Studio.Modules.CompositionTool.ViewModels
{
    internal class CompositionViewModel : PropertyChangedBase
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
            if (_items != null && (wasEdited || _items.Any(i => i.WasEdited)))
            {
                return new Composition(_items.Select(i => i.ToDefinition()).ToList());
            }
            return composition;
        }

        internal void AddRange(IEnumerable<CompositionObject> objects, IUndoRedoManager undoRedoManager)
        {
            wasEdited = true;
            var items = Items;
            foreach(var obj in objects)
            {
                items.AddUndoable(undoRedoManager, new CompositionItem(obj, this));
            }
            NotifyOfPropertyChange(nameof(Name));
            NotifyOfPropertyChange(nameof(IsEmpty));
            NotifyOfPropertyChange(nameof(SingleModel));
        }

        internal void RemoveItem(CompositionItem obj, IUndoRedoManager undoRedoManager)
        {
            wasEdited = true;
            Items.RemoveUndoable(undoRedoManager, obj);
            NotifyOfPropertyChange(nameof(Name));
            NotifyOfPropertyChange(nameof(IsEmpty));
            NotifyOfPropertyChange(nameof(SingleModel));
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

        public ModelInfo? SingleModel
        {
            get
            {
                if (_items != null)
                {
                    return _items.Count == 1 ? _items[0].Model : null;
                }
                return composition.Objects.Count == 1 ? composition.Objects[0].Model : null;
            }
        }

        public string Name => string.Join(", ", Names);

        public IEnumerable<string> Names
        {
            get
            {
                if (_items != null)
                {
                    return _items.Select(o => o.Model.Name);
                }
                return composition.Objects.Select(o => o.Model.Name);
            }
        }

        public bool IsEmpty
        {
            get
            {
                if (_items != null)
                {
                    return _items.Count == 0;
                }
                return composition.Objects.Count == 0;
            }
        }
    }
}
