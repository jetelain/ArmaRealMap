using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GameRealisticMap.Studio.Modules.Explorer.ViewModels;
using GameRealisticMap.Studio.Modules.Main;
using GameRealisticMap.Studio.Modules.MapConfigEditor;
using GameRealisticMap.Studio.Shared;
using GameRealisticMap.Studio.Toolkit;
using Gemini.Framework;
using Gemini.Framework.Services;

namespace GameRealisticMap.Studio.Modules.GenericMapConfigEditor.ViewModels
{
    internal class GenericMapConfigEditorViewModel : MapConfigEditorBase, IExplorerRootTreeItem, IMainDocument
    {
        private readonly IShell shell;
        private string _center = string.Empty;
        private string _southWest = string.Empty;
        private float _gridCellSize = 5;
        private int _gridSize = 512;

        public GenericMapConfigEditorViewModel(IShell shell)
        {
            this.shell = shell;
        }

        public string TreeName => DisplayName;

        public string Icon => GenericMapConfigEditorProvider.IconSource;

        public IEnumerable<IExplorerTreeItem> Children => Enumerable.Empty<IExplorerTreeItem>();

        public Task SaveTo(Stream stream)
        {
            throw new NotImplementedException();
        }

        protected override Task DoLoad(string filePath)
        {
            throw new NotImplementedException();
        }

        protected override Task DoNew()
        {
            return Task.CompletedTask;
        }

        protected override async Task DoSave(string filePath)
        {
            using var stream = File.Create(filePath);
            await SaveTo(stream);
        }


        public override int[] GridSizes => GridHelper.GenericGridSizes;

        public override string Center
        {
            get { return _center; }
            set
            {
                if (Set(ref _center, value))
                {
                    if (!string.IsNullOrEmpty(value))
                    {
                        SouthWest = string.Empty;
                    }
                    NotifyOfPropertyChange(nameof(MapSelection));
                    IsDirty = true;
                }
            }
        }


        public override string SouthWest
        {
            get { return _southWest; }
            set
            {
                if (Set(ref _southWest, value))
                {
                    if (!string.IsNullOrEmpty(value))
                    {
                        Center = string.Empty;
                    }
                    NotifyOfPropertyChange(nameof(MapSelection));
                    IsDirty = true;
                }
            }
        }

        public override float GridCellSize
        {
            get { return _gridCellSize; }
            set
            {
                if (Set(ref _gridCellSize, GridHelper.NormalizeCellSize(value)))
                {
                    NotifyOfPropertyChange(nameof(MapSize));
                    NotifyOfPropertyChange(nameof(MapSelection));
                    IsDirty = true;
                }
            }
        }

        public override int GridSize
        {
            get { return _gridSize; }
            set
            {
                if (Set(ref _gridSize, value))
                {
                    NotifyOfPropertyChange(nameof(MapSize));
                    NotifyOfPropertyChange(nameof(MapSelection));
                    IsDirty = true;
                }
            }
        }

        public override float MapSize
        {
            get { return GridSize * GridCellSize; }
            set
            {
                GridSize = GridHelper.GetGridSize(GridSizes, value);
                GridCellSize = GridHelper.NormalizeCellSize(value / GridSize);
            }
        }

    }
}
