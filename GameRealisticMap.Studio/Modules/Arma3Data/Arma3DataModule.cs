using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using BIS.PAA;
using GameRealisticMap.Arma3;
using GameRealisticMap.Arma3.IO;
using GameRealisticMap.Arma3.TerrainBuilder;
using Gemini.Framework;
using static PdfSharpCore.Pdf.PdfDictionary;

namespace GameRealisticMap.Studio.Modules.Arma3Data
{
    [Export(typeof(Arma3DataModule))] // TODO: an interface would be nicer
    [Export(typeof(IModule))]
    [Export(typeof(IArma3Previews))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    internal class Arma3DataModule : ModuleBase, IArma3Previews
    {
        private List<string>? previewsInProject;

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
            "library.json");

        public override void Initialize()
        {
            ProjectDrive = new ProjectDrive(Arma3ToolsHelper.GetProjectDrivePath(), new PboFileSystem());

            Library = new ModelInfoLibrary(ProjectDrive);

            ModelPreviewHelper = new ModelPreviewHelper(Library);
        }

        public override async Task PostInitializeAsync()
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

        public IEnumerable<ModelInfo> Import(IEnumerable<string> paths)
        {
            try
            {
                return paths.Select(ProjectDrive.GetGamePath).Select(p => Library.ResolveByPath(p)).ToList();
            }
            catch
            {
                return new List<ModelInfo>();
            }
        }

        public Uri? GetPreview(ModelInfo modelInfo)
        {
            var cacheJpeg = Path.Combine(PreviewCachePath, Path.ChangeExtension(modelInfo.Path, ".jpg"));
            if (File.Exists(cacheJpeg))
            {
                return new Uri(cacheJpeg);
            }
            var cachePng = Path.Combine(PreviewCachePath, Path.ChangeExtension(modelInfo.Path, ".png"));
            if (File.Exists(cachePng))
            {
                return new Uri(cachePng);
            }
            var editorPreview = LocateGameEditorPreview(modelInfo);
            if (!string.IsNullOrEmpty(editorPreview))
            {
                return CacheGameEditorPreview(cacheJpeg, editorPreview);
            }
            return null;
        }

        private Uri CacheGameEditorPreview(string cacheJpeg, string editorPreview)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(cacheJpeg)!);
            using (var target = File.Create(cacheJpeg))
            {
                using (var source = ProjectDrive.OpenFileIfExists(editorPreview)!)
                {
                    source.CopyTo(target);
                }
            }
            return new Uri(cacheJpeg);
        }

        private string? LocateGameEditorPreview(ModelInfo modelInfo)
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
            var previewName = $"land_{Path.GetFileNameWithoutExtension(modelInfo.Path)}.jpg";
            var editorPreview = previewsInProject.FirstOrDefault(p => p.EndsWith(previewName, StringComparison.OrdinalIgnoreCase));
            return editorPreview;
        }

        public BitmapSource? GetTexturePreview(string texture)
        {
            var cachePng = Path.Combine(PreviewCachePath, Path.ChangeExtension(texture, ".png"));
            if (File.Exists(cachePng))
            {
                // TODO: ensure that file is still up to date
                return new BitmapImage(new Uri(cachePng));
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
                    return new BitmapImage(new Uri(cachePng));
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
    }
}
