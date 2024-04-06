using System;
using System.Globalization;
using System.IO;
using System.Net.Cache;
using System.Security.Policy;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using Caliburn.Micro;
using GameRealisticMap.Studio.Modules.Arma3Data.Services;
using PdfSharpCore.Drawing;

namespace GameRealisticMap.Studio.Modules.Arma3Data.Controls
{
    public sealed class TexturePreviewConverter : IValueConverter
    {
        public int? Size { get; set; }

        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var texture = value as string;
            if (!string.IsNullOrEmpty(texture))
            {
                var previews = IoC.Get<IArma3Previews>();

                var uri = Size == null ? previews.GetTexturePreview(texture)
                    : previews.GetTexturePreviewSmall(texture, Size.Value);

                return Arma3PreviewsHelper.GetBitmapSource(uri);
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
