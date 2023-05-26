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
using GameRealisticMap.Arma3.IO;
using GameRealisticMap.Arma3.TerrainBuilder;
using Gemini.Framework;

namespace GameRealisticMap.Studio.Modules.Arma3Data
{
    [Export(typeof(IArma3DataModule))]
    [Export(typeof(IModule))]
    [Export(typeof(IArma3Previews))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    internal class Arma3DataModule : ModuleBase, IArma3Previews, IArma3DataModule
    {
        public static Uri NoPreview = new Uri("pack://application:,,,/GameRealisticMap.Studio;component/Resources/noimage.png");

        private List<string>? previewsInProject;

        private HttpClient client = new HttpClient();

        public ModelInfoLibrary Library { get; private set; }

        public ProjectDrive ProjectDrive { get; private set; }

        public ModelPreviewHelper ModelPreviewHelper { get; private set; }

        public string PreviewCachePath { get; set; } = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), 
            "GameRealisticMap", 
            "Arma3",
            "Previews");

        public string LibraryCachePath { get; set; } = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "GameRealisticMap",
            "Arma3",
            "modelinfo.json");

        public override void Initialize()
        {
            ProjectDrive = new ProjectDrive(Arma3ToolsHelper.GetProjectDrivePath(), new PboFileSystem());

            Library = new ModelInfoLibrary(ProjectDrive);

            ModelPreviewHelper = new ModelPreviewHelper(Library);
        }

        public override async Task PostInitializeAsync()
        {
            await LoadLibraryFromCache();
        }

        private async Task LoadLibraryFromCache()
        {
            if (File.Exists(LibraryCachePath))
            {
                await Library.LoadFrom(LibraryCachePath);
            }
        }

        public async Task SaveLibraryCache()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(LibraryCachePath)!);
            await Library.SaveTo(LibraryCachePath);
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
            using (var source = ProjectDrive.OpenFileIfExists(editorPreview)!)
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
            if (previewsInProject == null)
            {
                lock (this)
                {
                    if (previewsInProject == null)
                    {
                        previewsInProject = ProjectDrive.FindAll($"land_*.jpg").ToList();
                    }
                }
            }
            var previewName = $"land_{Path.GetFileNameWithoutExtension(modelPath)}.jpg";
            var editorPreview = previewsInProject.FirstOrDefault(p => p.EndsWith(previewName, StringComparison.OrdinalIgnoreCase));
            return editorPreview;
        }

        public Uri? GetTexturePreview(string texture)
        {
            var cachePng = Path.Combine(PreviewCachePath, Path.ChangeExtension(texture, ".png"));
            if (File.Exists(cachePng))
            {
                // TODO: ensure that file is still up to date
                return new Uri(cachePng);
            }
            using (var paaStream = ProjectDrive.OpenFileIfExists(texture))
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

        private static BitmapSource ReadPaaAsBitmapSource(Stream? paaStream)
        {
            var paa = new PAA(paaStream, false);
            var pixels = PAA.GetARGB32PixelData(paa, paaStream);
            var colors = paa.Palette.Colors.Select(c => Color.FromRgb(c.R8, c.G8, c.B8)).ToList();
            var bitmapPalette = (colors.Count > 0) ? new BitmapPalette(colors) : null;
            return BitmapSource.Create(paa.Width, paa.Height, 300, 300, PixelFormats.Bgra32, bitmapPalette, pixels, paa.Width * 4);
        }

        public async Task Reload()
        {
            Initialize();

            await LoadLibraryFromCache();

            previewsInProject = null;
        }

    }
}
