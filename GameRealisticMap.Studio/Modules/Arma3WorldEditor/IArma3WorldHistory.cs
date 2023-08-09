using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GameRealisticMap.Studio.Modules.Arma3WorldEditor
{
    public interface IArma3WorldHistory
    {
        Task RegisterWorld(string worldName, string pboPrefix, string description, string? modDirectory);

        Task<IReadOnlyCollection<IArma3WorldEntry>> GetEntries();

        event EventHandler<EventArgs>? Changed;
    }
}
