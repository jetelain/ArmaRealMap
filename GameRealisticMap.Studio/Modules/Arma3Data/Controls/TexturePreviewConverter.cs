﻿using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using Caliburn.Micro;

namespace GameRealisticMap.Studio.Modules.Arma3Data.Controls
{
    public sealed class TexturePreviewConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var texture = value as string;
            if (!string.IsNullOrEmpty(texture))
            {
                var uri = IoC.Get<IArma3Previews>().GetTexturePreview(texture); // GetTexturePreview can be really slow, find a way to make this lazy
                if (uri != null)
                {
                    return new BitmapImage(uri) { CreateOptions = BitmapCreateOptions.DelayCreation };
                }
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}