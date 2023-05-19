using System;
using System.Globalization;
using System.Windows.Data;

namespace GameRealisticMap.Studio.Modules.AssetConfigEditor.Views
{
    internal class PercentValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var proba = value as double?;
            if (proba != null)
            {
                return $"{proba * 100} %";
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var percent = value as string;
            if (percent != null)
            {
                percent = percent.Trim(' ', '%', '\t');
                if (double.TryParse(percent, NumberStyles.Any, culture, out var percentFloat))
                {
                    if (percentFloat >= 0 && percentFloat <= 100)
                    {
                        return percentFloat / 100;
                    }
                }
            }
            return 1d;
        }
    }
}
