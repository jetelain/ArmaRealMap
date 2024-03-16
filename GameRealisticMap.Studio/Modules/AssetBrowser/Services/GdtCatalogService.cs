using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using GameRealisticMap.Arma3.IO.Converters;
using GameRealisticMap.Studio.Modules.Arma3Data;
using System.ComponentModel.Composition;

namespace GameRealisticMap.Studio.Modules.AssetBrowser.Services
{
    [Export(typeof(IGdtCatalogService))]
    internal class GdtCatalogService : IGdtCatalogService
    {
        private List<GdtCatalogItem>? cachedList;
        private readonly SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);
        private readonly IArma3DataModule _arma3DataModule;

        public string GdtCatalogPath { get; set; } = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "GameRealisticMap",
            "Arma3",
            "gdt.json");

        [ImportingConstructor]
        public GdtCatalogService(IArma3DataModule arma3DataModule)
        {
            _arma3DataModule = arma3DataModule;
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
            return new List<GdtCatalogItem>();
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
        }

        private async Task Save(List<GdtCatalogItem> list)
        {
            using var stream = File.Create(GdtCatalogPath);
            await JsonSerializer.SerializeAsync(stream, list, CreateJsonOptions()).ConfigureAwait(false);
        }
    }
}
