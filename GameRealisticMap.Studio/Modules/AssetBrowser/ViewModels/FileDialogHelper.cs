using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using System.Windows.Media.Imaging;

namespace GameRealisticMap.Studio.Modules.AssetBrowser.ViewModels
{
    internal class FileDialogHelper
    {
        public static BitmapFrame? BrowseImage()
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = "Image|*.png;*.tiff;*.tif;*.bmp";
            if (dialog.ShowDialog() == true)
            {
                return BitmapFrame.Create(new Uri(dialog.FileName));
            }
            return null;
        }
    }
}
