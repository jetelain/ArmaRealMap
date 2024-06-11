using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameRealisticMap.Studio.Shared;
using GameRealisticMap.Studio.Toolkit;
using Gemini.Framework;
using Gemini.Framework.Services;

namespace GameRealisticMap.Studio.Modules.GenericMapConfigEditor.ViewModels
{
    internal abstract class MapConfigEditorBase : PersistedDocument
    {
        protected readonly IShell _shell;

        protected MapConfigEditorBase(IShell shell)
        {
            _shell = shell;
        }

        public abstract int[] GridSizes { get; }
        public abstract string Center { get; set; }
        public abstract string SouthWest { get; set; }
        public abstract float GridCellSize { get; set; }
        public abstract int GridSize { get; set; }
        public abstract float MapSize { get; set; }

        public LocationSelection? MapSelection
        {
            get
            {
                if (!string.IsNullOrEmpty(Center))
                {
                    return new LocationSelection(Center, true, TerrainAreaUTM.CreateFromCenter(Center, GridCellSize, GridSize));
                }
                if (!string.IsNullOrEmpty(SouthWest))
                {
                    return new LocationSelection(SouthWest, false, TerrainAreaUTM.CreateFromSouthWest(SouthWest, GridCellSize, GridSize));
                }
                return null;
            }
            set
            {
                if (value != null)
                {
                    MapSize = value.TerrainArea.SizeInMeters;

                    if (value.IsCenter)
                    {
                        Center = value.Coordinates;
                    }
                    else
                    {
                        SouthWest = value.Coordinates;
                    }
                }
            }
        }

        internal abstract Task<(IBuildersConfig, IMapProcessingOptions, ITerrainArea)> GetPreviewConfig();

        public Task GeneratePreviewNew()
        {
            return _shell.OpenDocumentAsync(new MapPreviewViewModel(this));
        }
    }
}
