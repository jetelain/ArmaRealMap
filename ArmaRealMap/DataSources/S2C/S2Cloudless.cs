using System;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Threading;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;

namespace ArmaRealMap.DataSources.S2C
{
    public class S2Cloudless : ISatImageProvider, IDisposable
    {
        private const double Delta = 20_037_508.342_789;

        private readonly HttpClient client = new HttpClient();
        private readonly string cacheLocation;

        // zoom 16
        // 16_777_216px x 16_777_216px
        // 256x256px tiles
        // 65536x65536 tiles

        // 
        public S2Cloudless(string cacheLocation)
        {
            this.cacheLocation = cacheLocation;
        }

        private readonly ConcurrentDictionary<string, Image<Rgb24>> cache = new ConcurrentDictionary<string, Image<Rgb24>>();
        public static NetTopologySuite.Geometries.Point LatLonToWebMercator(double lat, double lon)
        {
            var rMajor = 6378137; //Equatorial Radius, WGS84
            var shift = Math.PI * rMajor;
            var x = lon * shift / 180;
            var y = Math.Log(Math.Tan((90 + lat) * Math.PI / 360)) / (Math.PI / 180);
            y = y * shift / 180;
            return new NetTopologySuite.Geometries.Point(x, y);
        }

        public Rgb24 GetPixel(double lat, double lon)
        {
            var meters = LatLonToWebMercator(lat, lon);

            var y = Delta - meters.Y;
            var x = Delta + meters.X;

            var tileX = x * 32768 / Delta;
            var tileY = y * 32768 / Delta;

            var tX = Math.Floor(tileX);
            var tY = Math.Floor(tileY);

            var pX = (int)Math.Floor((tileX - tX) * 256);
            var pY = (int)Math.Floor((tileY - tY) * 256);

            var tileUri = $"https://tiles.maps.eox.at/wmts/1.0.0/s2cloudless-2020_3857/default/GoogleMapsCompatible/16/{tY}/{tX}.jpg";

            if ( cache.Count > 10000)
            {
                lock (this)
                {
                    if (cache.Count > 10000)
                    {
                        cache.Clear();
                    }
                }
            }

            return cache.GetOrAdd(tileUri, LoadTile)[pX, pY];
        }

        private Image<Rgb24> LoadTile(string uri)
        {
            var file = uri.Substring(91).Replace("/", "_");
            var cacheFile = System.IO.Path.Combine(cacheLocation, file);
            if (!System.IO.File.Exists(cacheFile))
            {
                lock (this)
                {
                    if (!System.IO.File.Exists(cacheFile))
                    {

                        byte[] data;
                        try
                        {
                            Thread.Sleep(5);
                            data = client.GetByteArrayAsync(uri).ConfigureAwait(false).GetAwaiter().GetResult(); //new WebClient().DownloadData(uri);
                        }
                        catch
                        {
                            Thread.Sleep(250);
                            data = client.GetByteArrayAsync(uri).ConfigureAwait(false).GetAwaiter().GetResult(); //new WebClient().DownloadData(uri);
                        }
                        System.IO.File.WriteAllBytes(cacheFile, data);

                    }

                }
            }

            try
            {
                return Image.Load<Rgb24>(cacheFile, new JpegDecoder());
            }
            catch
            {
                lock (this)
                {
                    Thread.Sleep(2000);
                    System.IO.File.Delete(cacheFile);
                }
                return LoadTile(uri);
            }
        }

        public void Dispose()
        {
            foreach (var img in cache.Values)
            {
                img.Dispose();
            }
            cache.Clear();
        }
    }

}
