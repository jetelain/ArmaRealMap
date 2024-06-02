using System.Threading.Tasks;
using GameRealisticMap.Configuration;

namespace GameRealisticMap.Studio.Modules.Main.Services
{
    public interface IGrmConfigService
    {
        ISourceLocations GetSources();

        Task SetSources(ISourceLocations sources);

        Task Load();
    }
}
