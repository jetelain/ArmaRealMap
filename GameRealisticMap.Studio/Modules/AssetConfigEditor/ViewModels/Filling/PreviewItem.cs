using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Text;
using System.Windows.Media;
using GameRealisticMap.Arma3.TerrainBuilder;
using GameRealisticMap.Geometries;
using GameRealisticMap.Studio.Modules.AssetConfigEditor.Controls;
using GameRealisticMap.Studio.Modules.AssetConfigEditor.Views.Filling;

namespace GameRealisticMap.Studio.Modules.AssetConfigEditor.ViewModels.Filling
{
    internal class PreviewItem
    {
        private static ConcurrentDictionary<string, Color> modelColor = new ConcurrentDictionary<string, Color>(StringComparer.OrdinalIgnoreCase);

        public PreviewItem(TerrainPolygon p, ModelInfo model, float scale, bool isVisual)
        {
            Polygon = p;
            Model = model;
            Scale = scale;
            Color = modelColor.GetOrAdd(model.Path, _ => AllocateColor(model));
            if (isVisual)
            {
                Color = Color.FromRgb(
                    (byte)(Color.R + (255 - Color.R) / 2),
                    (byte)(Color.G + (255 - Color.G) / 2),
                    (byte)(Color.B + (255 - Color.B) / 2));
            }

        }

        private static Color AllocateColor(ModelInfo model)
        {
            return Color.FromRgb((byte)Random.Shared.Next(64, 192), (byte)Random.Shared.Next(64, 192), (byte)Random.Shared.Next(64, 192));
        }

        public Color Color { get; }

        public TerrainPolygon Polygon { get; }

        public ModelInfo Model { get; }

        public float Scale { get; }

        public string Path => ToPath(Polygon);

        public static string ToPath(TerrainPolygon polygon)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < polygon.Shell.Count; ++i)
            {
                if (sb.Length > 0)
                {
                    sb.Append(' ');
                }
                var point = polygon.Shell[i];
                if (i == 0)
                {
                    sb.Append('M');
                }
                else
                {
                    sb.Append('L');
                }
                sb.Append((point.X * PreviewGrid.Scale).ToString(CultureInfo.InvariantCulture));
                sb.Append(',');
                sb.Append((point.Y * -PreviewGrid.Scale + PreviewGrid.Size).ToString(CultureInfo.InvariantCulture));
                sb.Append(' ');
            }
            sb.Append(" Z");
            return sb.ToString();
        }
    }
}