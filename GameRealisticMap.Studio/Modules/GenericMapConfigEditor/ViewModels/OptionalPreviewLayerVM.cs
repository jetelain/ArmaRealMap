using System;
using System.Collections.Generic;
using Caliburn.Micro;
using GameRealisticMap.Geometries;
using GameRealisticMap.Studio.Controls;
using GameRealisticMap.Studio.Shared;

namespace GameRealisticMap.Studio.Modules.GenericMapConfigEditor.ViewModels
{
    internal class OptionalPreviewLayerVM : PropertyChangedBase
    {
        private readonly Func<IContext, PreviewAdditionalLayer> generate;
        private readonly string name;
        private readonly MapPreviewViewModel preview;
        private bool isEnabled;

        public OptionalPreviewLayerVM(MapPreviewViewModel preview, string name, Func<IContext, List<TerrainPoint>> generate)
            : this(preview, name, ctx => new PreviewAdditionalLayer(name, generate(ctx)))
        {
            Type = LegendItemType.Point;
        }

        public OptionalPreviewLayerVM(MapPreviewViewModel preview, string name, Func<IContext, List<TerrainPath>> generate)
            : this(preview, name, ctx => new PreviewAdditionalLayer(name, generate(ctx)))
        {
            Type = LegendItemType.Path;
        }

        public OptionalPreviewLayerVM(MapPreviewViewModel preview, string name, Func<IContext, List<TerrainPolygon>> generate)
            : this(preview, name, ctx => new PreviewAdditionalLayer(name, generate(ctx)))
        {
            Type = LegendItemType.Polygon;
        }

        private OptionalPreviewLayerVM(MapPreviewViewModel preview, string name, Func<IContext, PreviewAdditionalLayer> generate)
        {
            this.generate = generate;
            this.name = name;
            this.preview = preview;
            Label = Labels.ResourceManager.GetString("Asset" + name) ??
                Labels.ResourceManager.GetString("Asset" + name.TrimEnd('s')) ??
                Labels.ResourceManager.GetString("Layer" + name) ?? name;
        }

        public string Label { get; }

        public string Name => name;

        public LegendItemType Type { get; }

        public bool IsEnabled
        {
            get { return isEnabled; }
            set
            {
                if (isEnabled != value)
                {
                    isEnabled = value;
                    if (value)
                    {
                        preview.AddOptionalLayer(name, generate);
                    }
                    else
                    {
                        preview.RemoveOptionalLayer(name);
                    }
                }
            }
        }

        public Func<IContext, PreviewAdditionalLayer> Generate => generate;

        internal void SetActualEnabled(bool value)
        {
            if (isEnabled != value)
            {
                isEnabled = value;
                NotifyOfPropertyChange(nameof(isEnabled));
            }
        }
    }
}
