using System;

namespace GameRealisticMap.Studio.Modules.Main.Services
{
    public sealed class RecentFileEntry
    {
        public RecentFileEntry(string fullpath, DateTime timeStamp, bool isPinned)
        {
            FullPath = fullpath;
            TimeStamp = timeStamp;
            IsPinned = isPinned;
        }

        public string FullPath { get; }

        public DateTime TimeStamp { get; set; }

        public bool IsPinned { get; set; }
    }
}
