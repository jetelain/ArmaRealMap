using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GameRealisticMap.Arma3.IO;
using GameRealisticMap.Arma3.TerrainBuilder;

namespace GameRealisticMap.Studio.Modules.Arma3Data
{
    internal interface IArma3DataModule
    {
        ModelInfoLibrary Library { get; }

        ProjectDrive ProjectDrive { get; }

        ModelPreviewHelper ModelPreviewHelper { get; }

        Task SaveLibraryCache();

        Task Reload();

        IEnumerable<string> ActiveMods { get; }

        Task ChangeActiveMods (IEnumerable<string> mods);

        event EventHandler<EventArgs> Reloaded;
    }
}
