using System;
using System.Globalization;
using System.Windows.Data;

namespace GameRealisticMap.Studio.Controls
{
    internal sealed class EnumToBooleanConverter : IValueConverter
    {
        private readonly Enum enumValue;

        public EnumToBooleanConverter(Enum value)
        {
            this.enumValue = value;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return enumValue.Equals(value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool booleanValue && booleanValue)
            {
                return enumValue;
            }
            return Binding.DoNothing;
        }
    }
}