using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace GameRealisticMap.Studio.Modules.CompositionTool.Views
{
    internal class MetersToPixelsConverter : IValueConverter
    {
        public double Coef { get; set; } = 1;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var meters = value as float?;
            if ( meters != null)
            {
                return meters * CanvasGrid.Scale * Coef;
            }
            return CanvasGrid.Size;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
