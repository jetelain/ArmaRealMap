using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GameRealisticMap.Studio.Modules.Main.Services
{
    internal interface IRecentFilesService
    {
        Task AddRecentFile(string fullpath);

        Task<List<RecentFileEntry>> GetEntries();

        Task Save();

        event EventHandler Changed;
    }
}
