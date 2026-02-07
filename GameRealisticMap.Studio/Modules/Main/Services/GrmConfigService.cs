using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Threading.Tasks;
using GameRealisticMap.Configuration;

namespace GameRealisticMap.Studio.Modules.Main.Services
{
    [Export(typeof(IGrmConfigService))]
    internal class GrmConfigService : IGrmConfigService
    {
        private ISourceLocations? sources;

        public ISourceLocations GetSources()
        {
            return sources ?? DefaultSourceLocations.Instance;
        }

        public async Task Load()
        {
            sources = await SourceLocations.Load();
        }

        public async Task SetSources(ISourceLocations newSources)
        {
            await SourceLocations.Save(newSources);
            sources = await SourceLocations.Load();
        }
    }
}
