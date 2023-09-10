using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Documents;
using Caliburn.Micro;
using GameRealisticMap.Geometries;
using GameRealisticMap.Studio.Shared;

namespace GameRealisticMap.Studio.Modules.MapConfigEditor.ViewModels
{
    internal class OptionalPreviewLayerVM : PropertyChangedBase
    {
        private readonly Func<IContext, PreviewAdditionalLayer> generate;
        private readonly string name;
        private readonly MapPreviewViewModel preview;

        public OptionalPreviewLayerVM(MapPreviewViewModel preview, string name, Func<IContext, List<TerrainPoint>> generate)
            : this(preview, name, ctx => new PreviewAdditionalLayer(name, generate(ctx)))
        {
        }

        public OptionalPreviewLayerVM(MapPreviewViewModel preview, string name, Func<IContext, List<TerrainPath>> generate)
            : this(preview, name, ctx => new PreviewAdditionalLayer(name, generate(ctx)))
        {
        }
        public OptionalPreviewLayerVM(MapPreviewViewModel preview, string name, Func<IContext, List<TerrainPolygon>> generate)
            : this(preview, name, ctx => new PreviewAdditionalLayer(name, generate(ctx)))
        {
        }

        public OptionalPreviewLayerVM(MapPreviewViewModel preview, string name, Func<IContext, PreviewAdditionalLayer> generate)
        {
            this.generate = generate;
            this.name = name;
            this.preview = preview;
            Label = Labels.ResourceManager.GetString("Asset" + name) ?? Labels.ResourceManager.GetString("Layer" + name) ?? name;
        }

        public string Label { get; }

        public bool IsNotAdded { get; set; } = true;

        public Task Add()
        {
            preview.AddOptionalLayer(name, generate);
            IsNotAdded = false;
            NotifyOfPropertyChange(nameof(IsNotAdded));
            return Task.CompletedTask;
        }
    }
}
