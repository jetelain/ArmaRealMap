using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GameRealisticMap.Arma3.Aerial;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Reporting;

namespace GameRealisticMap.Studio.Modules.Arma3Data.Services
{
    [Export(typeof(IArma3AerialImageService))]
    internal class Arma3AerialImageService : IArma3AerialImageService
    {
        private readonly IArma3DataModule module;

        [ImportingConstructor]
        internal Arma3AerialImageService(IArma3DataModule module)
        {
            this.module = module;
        }

        private static string AerialImagesPath { get; } = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
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
                .Select(m => AerialModelRefence.FromODOL(m, module.Library.ReadModelInfoOnly(m) ?? throw new ApplicationException($"File '{m}' not found.")))
                .ToList();

            if (references.Count > 0)
            {
                var worker = new AerialPhotoWorker(progressSystem, references, AerialImagesPath, mods);

                await worker.TakePhotos();
            }
        }

        private bool Exists(string model)
        {
            return File.Exists(Path.Combine(AerialImagesPath, Path.ChangeExtension(model, ".png")));
        }
    }
}
