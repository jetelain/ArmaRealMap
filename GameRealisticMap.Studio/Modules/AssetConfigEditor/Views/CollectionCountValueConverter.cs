using System;
using System.Globalization;
using System.Windows.Data;

namespace GameRealisticMap.Studio.Modules.AssetConfigEditor.Views
{
    internal class CollectionCountValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var count = value as int?;
            if (count != null && count.Value > 0)
            {
                return string.Format(Labels.CountItems, count);
            }
            return Labels.CountNone;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
