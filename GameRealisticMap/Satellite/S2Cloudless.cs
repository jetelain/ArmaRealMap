using System.Collections.Concurrent;
using System.Diagnostics;
using GameRealisticMap.Reporting;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;

namespace GameRealisticMap.Satellite
{
    internal class S2Cloudless : IDisposable
    {
        private const double Delta = 20_037_508.342_789;

        private readonly string cacheLocation = Path.Combine(Path.GetTempPath(), "GameRealisticMap", "S2Cloudless");
        private readonly IProgressSystem progress;
        private readonly HttpClient httpClient;
        private readonly SemaphoreSlim downloadSemaphore = new SemaphoreSlim(1, 1);
        private readonly ConcurrentDictionary<(int,int), Task<Image<Rgb24>>> cache = new ConcurrentDictionary<(int, int), Task<Image<Rgb24>>>();

        private int estimatedCachePressure = 0;

        private const int MaxCachePressure = 10000; // Arround 2 GiB : 10000 * 197 KB per tile

        private static readonly int zoomLevel = 15; // best compromise, almost equal to zoomLevel 16 result, 20% faster to process
        private static readonly int halfTileCount = (int)Math.Pow(2, zoomLevel) / 2;
        private static readonly int tileSize = 256;
        private static readonly string endPoint = "https://tiles.maps.eox.at/wmts/1.0.0/s2cloudless-2020_3857/default/GoogleMapsCompatible/";

        private const double rMajor = 6378137; //Equatorial Radius, WGS84
        private const double shift = Math.PI * rMajor;

        public S2Cloudless(IProgressSystem progress)
        {
            this.progress = progress;
            httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:109.0) Gecko/20100101 Firefox/111.0");
        }

        public static NetTopologySuite.Geometries.Point LatLonToWebMercator(GeoAPI.Geometries.Coordinate coordinate)
        {
            var x = coordinate.X * shift / 180;
            var y = Math.Log(Math.Tan((90 + coordinate.Y) * Math.PI / 360)) / (Math.PI / 180);
            y = y * shift / 180;
            return new NetTopologySuite.Geometries.Point(x, y);
        }

        public int ZoomLevel => zoomLevel;

        public async Task<Rgb24> GetPixel(GeoAPI.Geometries.Coordinate coordinate)
        {
            var meters = LatLonToWebMercator(coordinate);

            var y = Delta - meters.Y;
            var x = Delta + meters.X;

            var tileX = x * halfTileCount / Delta;
            var tileY = y * halfTileCount / Delta;

            var tX = Math.Floor(tileX);
            var tY = Math.Floor(tileY);

            var pX = (int)Math.Floor((tileX - tX) * tileSize);
            var pY = (int)Math.Floor((tileY - tY) * tileSize);

            if (estimatedCachePressure > MaxCachePressure)
            {
                await downloadSemaphore.WaitAsync();
                try
                {
                    if (estimatedCachePressure > MaxCachePressure)
                    {
                        ClearCache();
                    }
                }
                finally
                {
                    downloadSemaphore.Release();
                }
            }

            var tile = await cache.GetOrAdd(((int)tX,(int)tY), k =>LoadTile(k.Item1,k.Item2)).ConfigureAwait(false);

            return tile[pX, pY];
        }

        private void ClearCache()
        {
            foreach (var img in cache.Values)
            {
                img.Dispose();
            }
            cache.Clear();
            estimatedCachePressure = 0;
        }

        private async Task<byte[]> Load(Uri uri)
        {
            progress.WriteLine(uri.OriginalString);
            int sleep = 10;
            while (sleep < 20000)
            {
                await Task.Delay(sleep);
                try
                {
                    return await httpClient.GetByteArrayAsync(uri).ConfigureAwait(false);
                }
                catch(Exception ex)
                {
                    progress.WriteLine(ex.Message);
                }
                sleep += 500;
            }
            throw new ApplicationException($"Failed to load '{uri}'");
        }


        private async Task<Image<Rgb24>> LoadTile(int tX, int tY)
        {
            var filePath = FormattableString.Invariant($"{zoomLevel}/{tY}/{tX}.jpg");

            var cacheFile = System.IO.Path.Combine(cacheLocation, filePath);
            progress.WriteLine(cacheFile);
            if (!File.Exists(cacheFile))
            {
                await downloadSemaphore.WaitAsync();
                try
                {
                    if (!File.Exists(cacheFile))
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(cacheFile));
                        File.WriteAllBytes(cacheFile, await Load(new Uri(endPoint + filePath, UriKind.Absolute)));
                        progress.WriteLine("--> OK");
                    }
                }
                finally
                {
                    downloadSemaphore.Release();
                }
            }

            try
            {
                var image = await Image.LoadAsync<Rgb24>(cacheFile, new JpegDecoder()).ConfigureAwait(false);
                Interlocked.Increment(ref estimatedCachePressure);
                return image;
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
                return await LoadTile(tX, tY).ConfigureAwait(false);
            }
        }

        public void Dispose()
        {
            ClearCache();
            httpClient.Dispose();
        }
    }

}
