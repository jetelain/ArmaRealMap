using System;
using System.Collections.Generic;
using System.Linq;
using GameRealisticMap.Algorithms.Definitions;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Studio.Modules.AssetConfigEditor.ViewModels.Filling;
using GameRealisticMap.Studio.Modules.Explorer.ViewModels;

namespace GameRealisticMap.Studio.Modules.AssetConfigEditor.ViewModels
{
    internal abstract class PathObjectsViewModelBase<TId, TDefinition> : AssetProbabilityBase<TId, TDefinition> 
        where TId : struct, Enum
        where TDefinition : class, IWithProbability, IRowDefition<Composition>
    {
        protected bool useObjects;

        public PathObjectsViewModelBase(TId id, TDefinition? definition, AssetConfigEditorViewModel parent)
            : base(id, definition, parent)
        {

        }

        public abstract List<IExplorerTreeItem> SegmentsItems { get; }

        public abstract List<IExplorerTreeItem> ObjectsItems { get; }

        public IExplorerTreeItem? MainChild => Children.FirstOrDefault(); // Used by "Count" column on main view

        public override IEnumerable<IExplorerTreeItem> Children => useObjects ? ObjectsItems : SegmentsItems;

        public string DensityText => string.Empty; // To avoid binding error

        public bool UseObjects
        {
            get { return useObjects; }
            set
            {
                if (useObjects != value)
                {
                    useObjects = value;
                    NotifyOfPropertyChange();
                    NotifyOfPropertyChange(nameof(UseSegments));
                    NotifyOfPropertyChange(nameof(Children));
                    NotifyOfPropertyChange(nameof(MainChild));
                }
            }
        }

        public bool UseSegments
        {
            get { return !UseObjects; }
            set { UseObjects = !value; }
        }

    }
}
