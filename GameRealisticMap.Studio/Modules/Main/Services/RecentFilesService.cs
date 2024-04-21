using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using NLog;

namespace GameRealisticMap.Studio.Modules.Main.Services
{
    [Export(typeof(IRecentFilesService))]
    internal class RecentFilesService : IRecentFilesService
    {
        private static readonly Logger logger = LogManager.GetLogger("RecentFilesService");
        private readonly SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);
        private const int MaxCount = 25;

        private List<RecentFileEntry>? cachedList;

        public event EventHandler? Changed;

        private static string RecentList { get; } = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "GameRealisticMap",
            "recent.json");

        public async Task AddRecentFile(string fullpath)
        {
            var list = await GetEntries();
            await semaphore.WaitAsync();
            try
            {
                var existing = list.FirstOrDefault(e => string.Equals(e.FullPath, fullpath, StringComparison.OrdinalIgnoreCase));
                if (existing != null)
                {
                    existing.TimeStamp = DateTime.UtcNow;
                }
                else
                {
                    if (list.Count == MaxCount)
                    {
                        var entry = list.Where(i => !i.IsPinned).OrderBy(i => i.TimeStamp).FirstOrDefault();
                        if (entry == null)
                        {
                            return;
                        }
                        list.Remove(entry);
                    }
                    list.Add(new RecentFileEntry(fullpath, DateTime.UtcNow, false));
                }
            }
            finally 
            { 
                semaphore.Release(); 
            }
            await Save();
        }

        public async Task Save()
        {
            await semaphore.WaitAsync();
            try
            {
                if (cachedList != null)
                {
                    using var stream = File.Create(RecentList);
                    await JsonSerializer.SerializeAsync(stream, cachedList);
                }
            }
            finally
            {
                semaphore.Release();
            }
            Changed?.Invoke(this, EventArgs.Empty);
        }

        public async Task<List<RecentFileEntry>> GetEntries()
        {
            if (cachedList != null)
            {
                return cachedList;
            }

            await semaphore.WaitAsync();
            try
            {
                if (cachedList == null)
                {
                    if ( File.Exists(RecentList) )
                    {
                        try
                        {
                            using var stream = File.OpenRead(RecentList);
                            cachedList = await JsonSerializer.DeserializeAsync<List<RecentFileEntry>>(stream);
                        }
                        catch(Exception ex)
                        {
                            logger.Error(ex);
                        }
                    }
                    if (cachedList == null)
                    {
                        cachedList = new List<RecentFileEntry>();
                    }
                }
                return cachedList;
            }
            finally
            {
                semaphore.Release();
            }
        }
    }
}
