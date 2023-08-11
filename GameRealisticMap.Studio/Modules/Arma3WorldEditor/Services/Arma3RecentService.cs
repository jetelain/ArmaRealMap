using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using GameRealisticMap.Arma3;
using GameRealisticMap.Studio.Modules.Arma3WorldEditor.ViewModels;
using NLog;

namespace GameRealisticMap.Studio.Modules.Arma3WorldEditor.Services
{
    [Export(typeof(IArma3RecentHistory))]
    internal class Arma3RecentService : IArma3RecentHistory
    {
        private static readonly Logger logger = LogManager.GetLogger("Arma3WorldHistory");

        private const int MaxEntries = 20;

        public event EventHandler<EventArgs>? Changed;

        private List<Arma3RecentEntry>? cachedList;

        public string FilePath { get; } = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "GameRealisticMap", "Arma3", "recent.json");

        private async Task<List<Arma3RecentEntry>> ReadEntries()
        {
            if (File.Exists(FilePath))
            {
                try
                {
                    using (var stream = File.OpenRead(FilePath))
                    {
                        return (await JsonSerializer.DeserializeAsync<List<Arma3RecentEntry>>(stream)) ?? new List<Arma3RecentEntry>();
                    }
                }
                catch (Exception e)
                {
                    logger.Warn(e);
                }
            }
            var list = new List<Arma3RecentEntry>();
            try
            {
                await GenerateInitialList(list);
            }
            catch (Exception e)
            {
                logger.Warn(e);
            }
            return list;
        }

        private async Task GenerateInitialList(List<Arma3RecentEntry> list)
        {
            var location = Path.Combine(Arma3ToolsHelper.GetProjectDrivePath(), "z", "arm", "addons");
            if (Directory.Exists(location))
            {
                var files = Directory.GetFiles(location, "m_*.wrp", SearchOption.AllDirectories);
                foreach (var file in files)
                {
                    var directory = Path.GetDirectoryName(file) ?? location;
                    var worldName = Path.GetFileNameWithoutExtension(file);
                    list.Add(new Arma3RecentEntry()
                    {
                        WorldName = worldName,
                        Description = GetDescription(directory, worldName),
                        ModDirectory = Arma3MapConfig.GetAutomaticTargetModDirectory(worldName),
                        PboPrefix = directory.Replace(Arma3ToolsHelper.GetProjectDrivePath(), "").TrimStart('\\', '/'),
                        TimeStamp = File.GetLastWriteTimeUtc(file)
                    });
                }
            }
            await SaveEntries(list);
        }

        private string GetDescription(string directory, string worldName)
        {
            var configFile = Path.Combine(directory, "config.cpp");
            if (File.Exists(configFile))
            {
                return ConfigFileData.ReadFromFile(configFile, worldName).Description;
            }
            return string.Empty;
        }

        private async Task SaveEntries(List<Arma3RecentEntry> entries)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
            try
            { 
                using var stream = File.Create(FilePath);
                await JsonSerializer.SerializeAsync(stream, entries);
            }
            catch (Exception e)
            {
                logger.Warn(e);
            }
        }

        private async Task<List<Arma3RecentEntry>> GetEntriesImpl()
        {
            if (cachedList != null)
            {
                return cachedList;
            }
            return cachedList = await ReadEntries();
        }

        public async Task<IReadOnlyCollection<IArma3RecentEntry>> GetEntries()
        {
            return await GetEntriesImpl();
        }

        public async Task RegisterWorld(string worldName, string pboPrefix, string description, string? modDirectory, string? configFile)
        {
            var entries = await GetEntriesImpl();

            CreateEntry(entries, worldName, pboPrefix, description, modDirectory, configFile);

            await SaveEntries(entries);

            try
            {
                Changed?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception e)
            {
                logger.Error(e);
            }
        }

        private static void CreateEntry(List<Arma3RecentEntry> entries, string worldName, string pboPrefix, string description, string? modDirectory, string? configFile)
        {
            var entry = entries.FirstOrDefault(e => string.Equals(worldName, e.WorldName, StringComparison.OrdinalIgnoreCase));
            if (entry == null)
            {
                entry = new Arma3RecentEntry() { WorldName = worldName };
                if (entries.Count > MaxEntries)
                {
                    entries.Remove(entries.OrderBy(e => e.TimeStamp).First());
                }
                entries.Add(entry);
            }
            entry.TimeStamp = DateTime.UtcNow;
            entry.PboPrefix = pboPrefix;
            entry.Description = description;
            entry.ModDirectory = modDirectory ?? entry.ModDirectory;
            entry.ConfigFile = configFile ?? entry.ConfigFile;
        }

        public async Task<IArma3RecentEntry?> GetEntryOrDefault(string worldName)
        {
            var entries = await GetEntriesImpl();

            return entries.FirstOrDefault(e => string.Equals(worldName, e.WorldName, StringComparison.OrdinalIgnoreCase));
        }
    }
}
