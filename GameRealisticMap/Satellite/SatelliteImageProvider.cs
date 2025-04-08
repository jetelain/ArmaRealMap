using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using GameRealisticMap.Configuration;
using Pmad.ProgressTracking;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace GameRealisticMap.Satellite
{
    public class SatelliteImageProvider : IDisposable
    {
        private const double Delta = 20_037_508.342_789;

        private readonly string cacheLocation = Path.Combine(Path.GetTempPath(), "GameRealisticMap", "SatelliteImage");
        private readonly IProgressBase progress;
        private readonly HttpClient httpClient;
        private readonly SemaphoreSlim downloadSemaphore = new SemaphoreSlim(1, 1);
        private readonly ConcurrentDictionary<(int,int), Task<Image<Rgba32>>> cache = new ConcurrentDictionary<(int, int), Task<Image<Rgba32>>>();
        private readonly string endPoint;

        private int estimatedCachePressure = 0;

        private const int MaxCachePressure = 5000; // Arround 2 GiB : 10000 * 197 KB per tile

        private readonly int zoomLevel = 15; // best compromise with S2 Cloudless, almost equal to zoomLevel 16 result, 20% faster to process
        private readonly int halfTileCount;
        private readonly int tileSize = 256;

        private const double rMajor = 6378137; //Equatorial Radius, WGS84
        private const double shift = Math.PI * rMajor;

        private static readonly Regex ZoomRegex = new Regex(@"/([0-9]+)/\{[xy]\}/\{[xy]\}",RegexOptions.IgnoreCase);

        public SatelliteImageProvider(IProgressBase progress, ISourceLocations sources)
        {
            this.progress = progress;
            endPoint = sources.SatelliteImageProvider.ToString();
            if (!endPoint.Contains("{x}"))
            {
                endPoint = $"{endPoint.TrimEnd('/')}/{zoomLevel}/{{y}}/{{x}}.jpg";
            }
            else if (endPoint.Contains("{z}"))
            {
                zoomLevel = 16;
                endPoint = endPoint.Replace("{z}", zoomLevel.ToString());
            }
            else
            {
                var match = ZoomRegex.Match(endPoint);
                if (match.Success && int.TryParse(match.Groups[1].Value, out var actualZoom))
                {
                    zoomLevel = actualZoom;
                }
            }
            progress.WriteLine($"ZoomLevel={zoomLevel} EndPoint={endPoint}");

            httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("User-Agent", "GameRealisticMap/1.0");
            halfTileCount = (int)Math.Pow(2, zoomLevel) / 2;
        }

        public static string GetName(ISourceLocations sources)
        {
            return sources.SatelliteImageProvider.DnsSafeHost;
        }

        public static NetTopologySuite.Geometries.Point LatLonToWebMercator(GeoAPI.Geometries.Coordinate coordinate)
        {
            var x = coordinate.X * shift / 180;
            var y = Math.Log(Math.Tan((90 + coordinate.Y) * Math.PI / 360)) / (Math.PI / 180);
            y = y * shift / 180;
            return new NetTopologySuite.Geometries.Point(x, y);
        }

        public int ZoomLevel => zoomLevel;

        public async Task<Image<Rgba32>> GetTile(GeoAPI.Geometries.Coordinate coordinate)
        {
            var meters = LatLonToWebMercator(coordinate);

            var y = Delta - meters.Y;
            var x = Delta + meters.X;

            var tileX = x * halfTileCount / Delta;
            var tileY = y * halfTileCount / Delta;

            var tX = Math.Floor(tileX);
            var tY = Math.Floor(tileY);

            return await cache.GetOrAdd(((int)tX, (int)tY), k => LoadTile(k.Item1, k.Item2)).ConfigureAwait(false);
        }

        public async Task<Rgba32> GetPixel(GeoAPI.Geometries.Coordinate coordinate)
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
                await downloadSemaphore.WaitAsync().ConfigureAwait(false);
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
            int sleep = 10;
            while (sleep < 20000)
            {
                progress.WriteLine($"Fetch '{uri.OriginalString}'");
                await Task.Delay(sleep).ConfigureAwait(false);
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


        private async Task<Image<Rgba32>> LoadTile(int tX, int tY)
        {
            var imageUri = new Uri(endPoint.Replace("{x}", tX.ToString()).Replace("{y}", tY.ToString()), UriKind.Absolute);
            var cacheFile = Path.Combine(cacheLocation, imageUri.DnsSafeHost, imageUri.AbsolutePath.TrimStart('/'));
            if (!File.Exists(cacheFile))
            {
                await downloadSemaphore.WaitAsync().ConfigureAwait(false);
                try
                {
                    if (!File.Exists(cacheFile) || (DateTime.Now - File.GetLastWriteTime(cacheFile)).TotalDays > 7)
                    {
                        // Most images providers ask to not cache the image for more than 7 days (to ensure enough API calls)

                        Directory.CreateDirectory(Path.GetDirectoryName(cacheFile)!);
                        File.WriteAllBytes(cacheFile, await Load(imageUri).ConfigureAwait(false));
                    }
                }
                finally
                {
                    downloadSemaphore.Release();
                }
            }
            progress.WriteLine($"Load '{cacheFile}'");
            try
            {
                var image = await Image.LoadAsync<Rgba32>(cacheFile).ConfigureAwait(false);
                if (image.Width != tileSize || image.Height != tileSize)
                {
                    image.Mutate(x => x.Resize(tileSize, tileSize));
                }
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
