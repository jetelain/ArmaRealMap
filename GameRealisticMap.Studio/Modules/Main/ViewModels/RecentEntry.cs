using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;
using GameRealisticMap.Studio.Modules.Main.Services;
using GameRealisticMap.Studio.Toolkit;
using Gemini.Framework.Services;

namespace GameRealisticMap.Studio.Modules.Main.ViewModels
{
    internal class RecentEntry
    {
        private readonly RecentFileEntry entry;
        private readonly IEditorProvider? provider;

        public RecentEntry(RecentFileEntry entry)
        {
            this.entry = entry;
            Exists = File.Exists(entry.FullPath); 
            provider = IoC.GetAll<IEditorProvider>().FirstOrDefault(e => e.Handles(entry.FullPath));
        }

        public DateTime TimeStamp => entry.TimeStamp;

        public string Name => Path.GetFileName(entry.FullPath);

        public bool Exists { get; }

        public bool IsPinned { get => entry.IsPinned; set => entry.IsPinned = value; }

        public Uri? IconUri => provider?.FileTypes?.FirstOrDefault()?.IconSource;

        public async Task OpenFile()
        {
            await EditorHelper.OpenDefaultEditor(entry.FullPath);
        }

        internal bool IsSame(string filePath)
        {
            return string.Equals(filePath, entry.FullPath, StringComparison.OrdinalIgnoreCase);
        }
    }
}
