using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GameRealisticMap.Studio.Modules.Arma3WorldEditor
{
    public interface IArma3RecentHistory
    {
        Task RegisterWorld(string worldName, string pboPrefix, string description, string? modDirectory, string? configFile = null);

        Task<IReadOnlyCollection<IArma3RecentEntry>> GetEntries();

        Task<IArma3RecentEntry?> GetEntryOrDefault(string worldName);

        event EventHandler<EventArgs>? Changed;
    }
}
