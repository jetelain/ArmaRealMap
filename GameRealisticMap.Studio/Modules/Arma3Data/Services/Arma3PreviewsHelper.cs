using System;
using System.IO;
using System.Windows.Media.Imaging;

namespace GameRealisticMap.Studio.Modules.Arma3Data.Services
{
    public static class Arma3PreviewsHelper
    {
        public static BitmapSource? GetBitmapSource(Uri? uri)
        {
            if (uri == null)
            {
                return null;
            }
            if (uri.LocalPath.Contains("{"))
            {
                // Avoid file-locking issue
                return BitmapFrame.Create(new MemoryStream(File.ReadAllBytes(uri.LocalPath)));
            }
            return new BitmapImage(uri) { CreateOptions = BitmapCreateOptions.DelayCreation };
        }

        public static BitmapFrame? GetBitmapFrame(Uri? uri)
        {
            if (uri == null)
            {
                return null;
            }
            if (uri.LocalPath.Contains("{"))
            {
                // Avoid file-locking issue
                return BitmapFrame.Create(new MemoryStream(File.ReadAllBytes(uri.LocalPath)));
            }
            return BitmapFrame.Create(uri);
        }
    }
}
