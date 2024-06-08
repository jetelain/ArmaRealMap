using System;
using System.Globalization;
using System.Windows.Data;
using GameRealisticMap.Geometries;

namespace GameRealisticMap.Studio.Controls
{
    public class TerrainPointStringConverter : IValueConverter
    {
        public string Format { get; set; } = "00000";

        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var sourceValue = value as TerrainPoint;
            if (sourceValue != null) 
            {
                return Math.Max(0, sourceValue.X).ToString(Format, culture) + " - " + Math.Max(0, sourceValue.Y).ToString(Format, culture);
            }
            return null;
        }

        public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var sourceValue = value as string;
            if (sourceValue != null)
            {
                var parts = sourceValue.Split('-');
                if (parts.Length == 2)
                {
                    if (float.TryParse(parts[0].Trim(), culture, out var x) && float.TryParse(parts[1].Trim(), culture, out var y))
                    {
                        return new TerrainPoint(x, y);
                    }
                }
            }
            return null;

        }
    }
}
