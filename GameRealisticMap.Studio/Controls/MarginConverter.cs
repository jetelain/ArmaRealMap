using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace GameRealisticMap.Studio.Controls
{
    public sealed class MarginConverter : IValueConverter
    {
        public double Scale { get; set; } = 1;

        public bool Right { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                if (Right)
                {
                    return new Thickness(0, 0, System.Convert.ToDouble(value) * Scale, 0);
                }
                return new Thickness(System.Convert.ToDouble(value) * Scale, 0, 0, 0);
            }
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
