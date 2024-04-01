using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using GameRealisticMap.Arma3.IO;
using GameRealisticMap.Arma3.IO.Converters;
using GameRealisticMap.Studio.Modules.Arma3Data;
using GameRealisticMap.Studio.Modules.Arma3Data.Services;
using GameRealisticMap.Studio.Modules.AssetBrowser.Data;
using GameRealisticMap.Studio.Toolkit;

namespace GameRealisticMap.Studio.Modules.AssetBrowser.Services
{
    [Export(typeof(IGdtCatalogStorage))]
    internal class GdtCatalogService : IGdtCatalogStorage
    {
        private List<GdtCatalogItem>? cachedList;
        private readonly SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);
        private readonly IArma3DataModule _arma3DataModule;
        private readonly IArma3Previews _arma3Previews;
        private readonly IArma3ModsService _arma3ModsService;

        public event EventHandler<List<GdtCatalogItem>>? Updated;

        public string GdtCatalogPath { get; set; } = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "GameRealisticMap",
            "Arma3",
            "gdt.json");

        [ImportingConstructor]
        public GdtCatalogService(IArma3DataModule arma3DataModule, IArma3Previews arma3Previews, IArma3ModsService arma3ModsService)
        {
            _arma3DataModule = arma3DataModule;
            _arma3Previews = arma3Previews;
            _arma3ModsService = arma3ModsService;
        }

        public async Task<List<GdtCatalogItem>> GetOrLoad()
        {
            if (cachedList == null)
            {
                await semaphoreSlim.WaitAsync().ConfigureAwait(false);
                try
                {
                    if (cachedList == null)
                    {
                        cachedList = await Load().ConfigureAwait(false);
                    }
                }
                finally
                {
                    semaphoreSlim.Release();
                }
            }
            return cachedList;
        }

        private async Task<List<GdtCatalogItem>> Load()
        {
            if (File.Exists(GdtCatalogPath))
            {
                using var stream = File.OpenRead(GdtCatalogPath);
                return await JsonSerializer.DeserializeAsync<List<GdtCatalogItem>>(stream, CreateJsonOptions()).ConfigureAwait(false)
                    ?? new List<GdtCatalogItem>();
            }
            var list = new List<GdtCatalogItem>();
            var titles = BuiltinObjectsList.ReadCsv("GdtTitles");
            var pboFS = (_arma3DataModule.ProjectDrive.SecondarySource as PboFileSystem);
            if (pboFS != null)
            {
                Import(list, new GdtImporter(_arma3DataModule.Library).ImportVanilla(pboFS), titles);
            }
            var armMod = _arma3ModsService.GetModsList().FirstOrDefault(m => m.SteamId == "2982306133");
            if(armMod != null)
            {
                Import(list, new GdtImporter(_arma3DataModule.Library).ImportMod(armMod), titles);
            }
            await Save(list).ConfigureAwait(false);
            return list;
        }

        private void Import(List<GdtCatalogItem> list, List<GdtImporterItem> gdtImporterItems, List<string[]> titles)
        {
            foreach (var item in gdtImporterItems)
            {
                var existing = list.FirstOrDefault(i => string.Equals(i.Material.ColorTexture, item.ColorTexture));
                if (existing != null)
                {
                    list.Remove(existing);
                }

                using var fakeSat = GdtHelper.GenerateFakeSatPngImage(_arma3Previews, item.ColorTexture);

                var color = GdtHelper.AllocateUniqueColor(fakeSat, list.Select(i => i.Material.Id.ToWpfColor()));

                var title = titles.FirstOrDefault(e => e.Length == 2 && string.Equals(e[0], item.ColorTexture, StringComparison.OrdinalIgnoreCase))?[1];

                list.Add(new GdtCatalogItem(
                    new Arma3.Assets.TerrainMaterial(item.NormalTexture, item.ColorTexture, color.ToRgb24(), fakeSat?.ToPngByteArray()),
                    item.Config,
                    GdtCatalogItemType.GameData,
                    title));
            }
        }
    
        private JsonSerializerOptions CreateJsonOptions()
        {
            return new JsonSerializerOptions()
            {
                Converters = {
                        new JsonStringEnumConverter(),
                        new ModelInfoReferenceConverter(_arma3DataModule.Library, true) }
            };
        }

        public async Task SaveChanges(List<GdtCatalogItem> list)
        {
            await semaphoreSlim.WaitAsync().ConfigureAwait(false);
            try
            {
                cachedList = list;

                await Save(list).ConfigureAwait(false);
            }
            finally
            {
                semaphoreSlim.Release();
            }

            Updated?.Invoke(this, cachedList);
        }

        private async Task Save(List<GdtCatalogItem> list)
        {
            using var stream = File.Create(GdtCatalogPath);
            await JsonSerializer.SerializeAsync(stream, list, CreateJsonOptions()).ConfigureAwait(false);
        }

        public async Task<GdtCatalogItem?> Resolve(string path)
        {
            var items = await GetOrLoad();
            return items.FirstOrDefault(i => string.Equals(path, i.Material.ColorTexture, StringComparison.OrdinalIgnoreCase));
        }

        public async Task ImportVanilla()
        {
            var pboFS = (_arma3DataModule.ProjectDrive.SecondarySource as PboFileSystem);
            if (pboFS != null)
            {
                await Import(new GdtImporter(_arma3DataModule.Library).ImportVanilla(pboFS));
            }
        }

        public async Task ImportMod(ModInfo installed)
        {
            await Import(new GdtImporter(_arma3DataModule.Library).ImportMod(installed));
        }

        private async Task Import(List<GdtImporterItem> gdtImporterItems)
        {
            await semaphoreSlim.WaitAsync().ConfigureAwait(false);
            try
            {
                if (cachedList == null)
                {
                    cachedList = await Load().ConfigureAwait(false);
                }
                Import(cachedList, gdtImporterItems, BuiltinObjectsList.ReadCsv("GdtTitles"));
            }
            finally
            {
                semaphoreSlim.Release();
            }

            Updated?.Invoke(this, cachedList);
        }

    }
}
