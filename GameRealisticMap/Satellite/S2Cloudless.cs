﻿using System.Collections.Concurrent;
using System.Diagnostics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;

namespace GameRealisticMap.Satellite
{
    internal class S2Cloudless : IDisposable
    {
        private const double Delta = 20_037_508.342_789;

        private readonly string cacheLocation = Path.Combine(Path.GetTempPath(), "GameRealisticMap", "S2Cloudless");
        private readonly HttpClient httpClient;
        private readonly SemaphoreSlim downloadSemaphore = new SemaphoreSlim(1, 1);
        private readonly ConcurrentDictionary<string, Task<Image<Rgb24>>> cache = new ConcurrentDictionary<string, Task<Image<Rgb24>>>();

        private static readonly int zoomLevel = 15; // best compromise, almost equal to zoomLevel 16 result, 20% faster to process
        private static readonly int halfTileCount = (int)Math.Pow(2, zoomLevel) / 2;
        private static readonly int tileSize = 256;
        private static readonly string endPoint = "https://tiles.maps.eox.at/wmts/1.0.0/s2cloudless-2020_3857/default/GoogleMapsCompatible/";

        public S2Cloudless()
        {
            httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:109.0) Gecko/20100101 Firefox/111.0");
        }

        public static NetTopologySuite.Geometries.Point LatLonToWebMercator(GeoAPI.Geometries.Coordinate coordinate)
        {
            var rMajor = 6378137; //Equatorial Radius, WGS84
            var shift = Math.PI * rMajor;
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

            var tilePath = FormattableString.Invariant($"{zoomLevel}/{tY}/{tX}.jpg");

            if (cache.Count > 10000)
            {
                await downloadSemaphore.WaitAsync();
                try
                {
                    if (cache.Count > 10000)
                    {
                        cache.Clear();
                    }
                }
                finally
                {
                    downloadSemaphore.Release();
                }
            }

            var tile = await cache.GetOrAdd(tilePath, LoadTile).ConfigureAwait(false);

            return tile[pX, pY];
        }

        private async Task<byte[]> Load(Uri uri)
        {
            Trace.WriteLine(uri);
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
                    Trace.WriteLine(ex.Message);
                }
                sleep += 500;
            }
            throw new ApplicationException($"Failed to load '{uri}'");
        }


        private async Task<Image<Rgb24>> LoadTile(string filePath)
        {
            var cacheFile = System.IO.Path.Combine(cacheLocation, filePath);
            Trace.WriteLine(cacheFile);
            if (!File.Exists(cacheFile))
            {
                await downloadSemaphore.WaitAsync();
                try
                {
                    if (!File.Exists(cacheFile))
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(cacheFile));
                        File.WriteAllBytes(cacheFile, await Load(new Uri(endPoint + filePath, UriKind.Absolute)));
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
                return await Image.LoadAsync<Rgb24>(cacheFile, new JpegDecoder()).ConfigureAwait(false);
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
                return await LoadTile(filePath).ConfigureAwait(false);
            }
        }

        public void Dispose()
        {
            foreach (var img in cache.Values)
            {
                img.Dispose();
            }
            cache.Clear();
            httpClient.Dispose();
        }
    }

}