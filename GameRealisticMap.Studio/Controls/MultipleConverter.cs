using System;
using System.Globalization;
using System.Windows.Data;

namespace GameRealisticMap.Studio.Controls
{
    public sealed class MultipleConverter : IValueConverter
    {
        public double Scale { get; set; } = 1;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                return System.Convert.ToDouble(value) * Scale;
            }
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
