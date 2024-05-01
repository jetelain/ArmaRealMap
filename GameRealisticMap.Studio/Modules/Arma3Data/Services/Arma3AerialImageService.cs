using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Runtime.Caching;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using GameRealisticMap.Arma3.Aerial;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Reporting;
using GameRealisticMap.Studio.Toolkit;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace GameRealisticMap.Studio.Modules.Arma3Data.Services
{
    [Export(typeof(IArma3AerialImageService))]
    internal class Arma3AerialImageService : IArma3AerialImageService
    {
        private readonly IArma3DataModule module;
        private readonly MemoryCache cache;

        [ImportingConstructor]
        internal Arma3AerialImageService(IArma3DataModule module)
        {
            this.module = module;
            cache = new MemoryCache("Arma3AerialImageService");
        }

        private static string AerialImagesPath { get; } = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "GameRealisticMap",
            "Arma3",
            "Aerial");

        public Uri? GetImageUri(string model)
        {
            var imagePath = Path.Combine(AerialImagesPath, Path.ChangeExtension(model, ".png"));
            if (File.Exists(imagePath))
            {
                return new Uri(imagePath);
            }
            return null;
        }

        public async Task TakeImages(IEnumerable<string> models, IEnumerable<ModDependencyDefinition> mods, IProgressSystem progressSystem, bool onlyMissing = true)
        {
            var references = models
                .Where(m => !onlyMissing || !Exists(m))
                .Select(m => GetModelReference(m))
                .ToList();

            if (references.Count > 0)
            {
                var worker = new AerialPhotoWorker(progressSystem, references, AerialImagesPath, mods);

                await worker.TakePhotos();
            }
        }

        private AerialModelRefence GetModelReference(string path)
        {
            BIS.P3D.ODOL.ModelInfo? model;
            try
            {
                model = module.Library.ReadModelInfoOnly(path);
            }
            catch(Exception ex)
            {
                throw new ApplicationException($"ODOL file for model '{path}' is corrupted: {ex.Message}", ex);
            }
            if (model == null)
            {
                throw new ApplicationException($"ODOL file for model '{path}' was not found.");
            }
            return AerialModelRefence.FromODOL(path, model);
        }

        public int CountMissing(IEnumerable<string> models)
        {
            return models.Count(m => !Exists(m));
        }

        private bool Exists(string model)
        {
            return File.Exists(Path.Combine(AerialImagesPath, Path.ChangeExtension(model, ".png")));
        }

        public BitmapSource? GetImage(string model)
        {
            var item = cache.Get(model) as BitmapSource;
            if (item != null)
            {
                return item;
            }
            var imagePath = Path.Combine(AerialImagesPath, Path.ChangeExtension(model, ".png"));
            if (File.Exists(imagePath))
            {
                using var img = Image.Load<Bgra32>(imagePath);
                item = img.ToWpf();
                var policy = new CacheItemPolicy();
                policy.ChangeMonitors.Add(new HostFileChangeMonitor(new List<string>() { imagePath }));
                cache.AddOrGetExisting(model, item, policy);
                return item;
            }
            return null;

        }
    }
}
