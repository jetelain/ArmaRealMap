using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using BIS.PAA;
using GameRealisticMap.Arma3;
using GameRealisticMap.Studio.Modules.Arma3Data.Services;
using Pmad.ProgressTracking;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using Color = System.Windows.Media.Color;

namespace GameRealisticMap.Studio.Modules.Arma3Data
{
    [Export(typeof(IArma3Previews))]
    [Export(typeof(IArma3ImageStorage))]
    internal class Arma3Previews : IArma3Previews, IArma3ImageStorage, IDisposable
    {
        private readonly IArma3DataModule _dataModule;

        public static Uri NoPreview = new Uri("pack://application:,,,/GameRealisticMap.Studio;component/Resources/noimage.png");

        private List<string>? previewsInProject;
        private Task? lastProcessPngToPaaTask;
        private readonly HttpClient client = new HttpClient();

        private readonly HashSet<string> pendingPaaConvertion = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        private static string PreviewCachePath { get; } = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "GameRealisticMap",
            "Arma3",
            "Previews");
        private static string ImageStoragePath { get; } = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "GameRealisticMap",
            "Arma3",
            "Images");

        public bool HasToProcessPngToPaa => pendingPaaConvertion.Count > 0;

        [ImportingConstructor]
        public Arma3Previews(IArma3DataModule dataModule)
        {
            _dataModule = dataModule;
            _dataModule.Reloaded += DataModuleWasReloaded;
        }

        private void DataModuleWasReloaded(object? sender, EventArgs e)
        {
            lock (this)
            {
                previewsInProject = null; // Invalidate cache
            }
        }

        public Uri GetPreviewFast(string modelPath)
        {
            var cacheJpeg = Path.Combine(PreviewCachePath, Path.ChangeExtension(modelPath, ".jpg"));
            if (File.Exists(cacheJpeg))
            {
                return new Uri(cacheJpeg);
            }
            return NoPreview;
        }

        public async Task<Uri> GetPreview(string modelPath)
        {
            var cacheJpeg = Path.Combine(PreviewCachePath, Path.ChangeExtension(modelPath, ".jpg"));
            if (File.Exists(cacheJpeg))
            {
                return new Uri(cacheJpeg);
            }
            try
            {
                return await GetPreviewSlow(modelPath, cacheJpeg).ConfigureAwait(false);
            }
            catch
            {
                return NoPreview;
            }
        }

        private async Task<Uri> GetPreviewSlow(string modelPath, string cacheJpeg)
        {
            var editorPreview = LocateGameEditorPreview(modelPath);
            if (!string.IsNullOrEmpty(editorPreview))
            {
                return await CacheGameEditorPreview(cacheJpeg, editorPreview).ConfigureAwait(false);
            }
            var response = await client.GetAsync("https://arm.pmad.net/previews/" + Path.ChangeExtension(modelPath, ".jpg").ToLowerInvariant());
            if (response.IsSuccessStatusCode)
            {
                using (var source = response.Content.ReadAsStream())
                {
                    await CopyToCache(cacheJpeg, source).ConfigureAwait(false);
                }
                return new Uri(cacheJpeg);
            }
            return NoPreview;
        }

        private async Task<Uri> CacheGameEditorPreview(string cacheJpeg, string editorPreview)
        {
            using (var source = _dataModule.ProjectDrive.OpenFileIfExists(editorPreview)!)
            {
                await CopyToCache(cacheJpeg, source).ConfigureAwait(false);
            }
            return new Uri(cacheJpeg);
        }

        private static async Task CopyToCache(string cacheJpeg, Stream source)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(cacheJpeg)!);
            using (var target = File.Create(cacheJpeg))
            {
                await source.CopyToAsync(target).ConfigureAwait(false);
            }
        }

        private string? LocateGameEditorPreview(string modelPath)
        {
            var cache = previewsInProject;
            if (cache == null)
            {
                lock (this)
                {
                    cache = previewsInProject;
                    if (cache == null)
                    {
                        cache = previewsInProject = _dataModule.ProjectDrive.FindAll($"land_*.jpg").ToList();
                    }
                }
            }
            var previewName = $"land_{Path.GetFileNameWithoutExtension(modelPath)}.jpg";
            var editorPreview = cache.FirstOrDefault(p => p.EndsWith(previewName, StringComparison.OrdinalIgnoreCase));
            return editorPreview;
        }

        public Uri? GetTexturePreview(string texture)
        {
            var cachePng = Path.Combine(PreviewCachePath, Path.ChangeExtension(texture, ".png"));
            if (texture.StartsWith("{"))
            {
                var storagePng = Path.Combine(ImageStoragePath, Path.ChangeExtension(texture, ".png"));
                if (File.Exists(storagePng))
                {
                    if (!File.Exists(cachePng) || File.GetLastWriteTimeUtc(storagePng) > File.GetLastWriteTimeUtc(cachePng))
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(cachePng)!);
                        File.Copy(storagePng, cachePng, true);
                    }
                    return new Uri(cachePng);
                }
            }
            if (File.Exists(cachePng))
            {
                var dt = _dataModule.ProjectDrive.GetLastWriteTimeUtc(texture);
                if ( dt == null || dt.Value < File.GetLastWriteTimeUtc(cachePng))
                {
                    return new Uri(cachePng);
                }
            }
            using (var paaStream = _dataModule.ProjectDrive.OpenFileIfExists(texture))
            {
                if (paaStream != null)
                {
                    var source = ReadPaaAsBitmapSource(paaStream);
                    Directory.CreateDirectory(Path.GetDirectoryName(cachePng)!);
                    using (var stream = File.Create(cachePng))
                    {
                        var encoder = new PngBitmapEncoder();
                        encoder.Frames.Add(BitmapFrame.Create(source));
                        encoder.Save(stream);
                    }
                    return new Uri(cachePng);
                }
            }
            return null;
        }

        public Uri? GetTexturePreviewSmall(string texture, int size = 512)
        {
            var fullImage = GetTexturePreview(texture);
            if (fullImage == null)
            {
                return null;
            }
            var cachePng = fullImage.LocalPath;
            var smallPng = Path.ChangeExtension(cachePng, $".{size}.png");
            if (!File.Exists(smallPng) || File.GetLastWriteTimeUtc(cachePng) > File.GetLastWriteTimeUtc(smallPng))
            {
                using var image = Image.Load(cachePng);
                image.Mutate(i => i.Resize(size, size));
                image.SaveAsPng(smallPng);
            }
            return new Uri(smallPng);
        }

        private static BitmapSource ReadPaaAsBitmapSource(Stream? paaStream)
        {
            var paa = new PAA(paaStream, false);
            var pixels = PAA.GetARGB32PixelData(paa, paaStream);
            var colors = paa.Palette.Colors.Select(c => Color.FromRgb(c.R8, c.G8, c.B8)).ToList();
            var bitmapPalette = (colors.Count > 0) ? new BitmapPalette(colors) : null;
            return BitmapSource.Create(paa.Width, paa.Height, 300, 300, PixelFormats.Bgra32, bitmapPalette, pixels, paa.Width * 4);
        }

        public void Dispose()
        {
            _dataModule.Reloaded -= DataModuleWasReloaded;
        }

        public Stream CreatePng(string path)
        {
            var file = Path.Combine(ImageStoragePath, Path.ChangeExtension(path, ".png"));
            lock (pendingPaaConvertion)
            {
                pendingPaaConvertion.Add(file);
            }
            Directory.CreateDirectory(Path.GetDirectoryName(file)!);
            return File.Create(file);
        }

        public byte[] ReadPngBytes(string path)
        {
            return File.ReadAllBytes(Path.Combine(ImageStoragePath, Path.ChangeExtension(path, ".png")));
        }

        public byte[] ReadPaaBytes(string path)
        {
            var paa = Path.Combine(ImageStoragePath, path);
            var png = Path.Combine(ImageStoragePath, Path.ChangeExtension(path, ".png"));
            if (!File.Exists(paa) || File.GetLastWriteTimeUtc(png) > File.GetLastWriteTimeUtc(paa))
            {
                LastProcessPngToPaaTask().Wait();
                Arma3ToolsHelper.ImageToPAA(new NoProgress(), png).Wait();
            }
            return File.ReadAllBytes(paa);
        }

        public async Task ProcessPngToPaa(IProgressScope? progress = null)
        {
            List<string> toProcess;
            lock (pendingPaaConvertion)
            {
                toProcess = pendingPaaConvertion.ToList();
                pendingPaaConvertion.Clear();
            }
            await LastProcessPngToPaaTask();
            await (lastProcessPngToPaaTask = Arma3ToolsHelper.ImageToPAA(progress ?? new NoProgress(), toProcess));
        }

        private async Task LastProcessPngToPaaTask()
        {
            if (lastProcessPngToPaaTask != null && !lastProcessPngToPaaTask.IsCompleted)
            {
                await lastProcessPngToPaaTask;
            }
        }
    }
}
