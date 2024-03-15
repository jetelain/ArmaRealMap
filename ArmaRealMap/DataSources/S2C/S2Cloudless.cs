using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;

namespace ArmaRealMap.DataSources.S2C
{
    public class S2Cloudless : ISatImageProvider, IDisposable
    {
        private const double Delta = 20_037_508.342_789;

        private readonly string cacheLocation;
        private readonly SemaphoreSlim downloadSemaphore = new SemaphoreSlim(1, 1);

        // zoom 16
        // 16_777_216px x 16_777_216px
        // 256x256px tiles
        // 65536x65536 tiles

        // 
        public S2Cloudless(string cacheLocation)
        {
            this.cacheLocation = cacheLocation;
        }

        private readonly ConcurrentDictionary<string, Task<Image<Rgb24>>> cache = new ConcurrentDictionary<string, Task<Image<Rgb24>>>();
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

            return cache.GetOrAdd(tileUri, LoadTile).Result[pX, pY];
        }

        private async Task<byte[]> Load(string uri)
        {
            Trace.WriteLine(uri);
            int sleep = 10;
            while (sleep < 20000)
            {
                await Task.Delay(sleep);
                try
                {
                    using(var client = new HttpClient() { Timeout = TimeSpan.FromSeconds(1) })
                    {
                        client.DefaultRequestHeaders.Add("User-Agent","Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:99.0) Gecko/20100101 Firefox/99.0");
                        return await client.GetByteArrayAsync(uri).ConfigureAwait(false);
                    }
                }
                catch(Exception ex)
                {
                    Trace.WriteLine(ex.Message);
                }
                sleep += 500;
            }
            throw new ApplicationException($"Failed to load '{uri}'");
        }


        private async Task<Image<Rgb24>> LoadTile(string uri)
        {
            var file = uri.Substring(91);
            var cacheFile = System.IO.Path.Combine(cacheLocation, file);
            if (!File.Exists(cacheFile))
            {
                await downloadSemaphore.WaitAsync();
                try
                {
                    if (!File.Exists(cacheFile))
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(cacheFile));
                        File.WriteAllBytes(cacheFile, await Load(uri));
                        Trace.WriteLine("--> OK");
                    }
                }
                finally
                {
                    downloadSemaphore.Release();
                }
            }

            try
            {
                return await Image.LoadAsync<Rgb24>(cacheFile).ConfigureAwait(false);
            }
            catch
            {
                await downloadSemaphore.WaitAsync();
                try
                {
                    await Task.Delay(1000);
                    File.Delete(cacheFile);
                }
                finally
                {
                    downloadSemaphore.Release();
                }
                return await LoadTile(uri).ConfigureAwait(false);
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
