using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GameRealisticMap.Studio.Modules.Arma3WorldEditor
{
    /// <summary>
    /// Persists and provides access to the list of recently-opened Arma 3 worlds.
    /// Raises <see cref="Changed"/> when the list is modified so that UI can refresh.
    /// </summary>
    public interface IArma3RecentHistory
    {
        Task RegisterWorld(string worldName, string pboPrefix, string description, string? modDirectory, string? configFile = null);

        Task<IReadOnlyCollection<IArma3RecentEntry>> GetEntries();

        Task<IArma3RecentEntry?> GetEntryOrDefault(string worldName);

        event EventHandler<EventArgs>? Changed;
    }
}
