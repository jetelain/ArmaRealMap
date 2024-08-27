using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;

namespace ArmaRealMapWebSite.Controllers
{
    public class S2CController : Controller
    {
        private readonly string cacheLocation = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "GameRealisticMap", "S2CloudlessCache");
        private readonly SemaphoreSlim downloadSemaphore = new SemaphoreSlim(1, 1);

        private readonly HttpClient httpClient;
        private readonly ILogger<S2CController> logger;

        public S2CController(IHttpClientFactory httpClientFactory, ILogger<S2CController> logger)
        {
            this.httpClient = httpClientFactory.CreateClient("S2C");
            this.logger = logger;
        }

        [Route("sat/{dataset}/{zoom}/{y}/{x}.jpg")]
        public async Task<ActionResult> GetTile(string dataset, int zoom, int y, int x)
        {
            if (dataset != "s2cloudless-2020" || zoom > 16 || zoom < 0 || x < 0 || y < 0)
            {
                return NotFound();
            }
            var endPoint = "https://tiles.maps.eox.at/wmts/1.0.0/s2cloudless-2020_3857/default/GoogleMapsCompatible/";
            var filePath = FormattableString.Invariant($"{zoom}/{y}/{x}.jpg");
            var cacheFile = System.IO.Path.Combine(cacheLocation, filePath);
            if (!System.IO.File.Exists(cacheFile))
            {
                await downloadSemaphore.WaitAsync().ConfigureAwait(false);
                try
                {
                    if (!System.IO.File.Exists(cacheFile))
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(cacheFile));
                        var bytes = await Load(new Uri(endPoint + filePath, UriKind.Absolute)).ConfigureAwait(false);
                        var id = Image.DetectFormat(bytes);
                        if (id.DefaultMimeType == "image/jpeg")
                        {
                            await System.IO.File.WriteAllBytesAsync(cacheFile, bytes);
                        }
                    }
                }
                finally
                {
                    downloadSemaphore.Release();
                }
            }
            if (!System.IO.File.Exists(cacheFile))
            {
                return NotFound();
            }
            return File(await System.IO.File.ReadAllBytesAsync(cacheFile), "image/jpeg");
        }

        private async Task<byte[]> Load(Uri uri)
        {
            int sleep = 10;
            while (sleep < 5000)
            {
                await Task.Delay(sleep).ConfigureAwait(false);
                try
                {
                    return await httpClient.GetByteArrayAsync(uri).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    logger.Log(LogLevel.Warning, "{0}: {1}", uri.OriginalString, ex.Message);
                }
                sleep += 500;
            }
            throw new ApplicationException($"Failed to load '{uri}'");
        }
    }
}
