using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SixLabors.ImageSharp.PixelFormats;

namespace ArmaRealMap.DataSources
{
    interface ISatImageProvider : IDisposable
    {
        Rgb24 GetPixel(double latitude, double longitude);
    }
}
