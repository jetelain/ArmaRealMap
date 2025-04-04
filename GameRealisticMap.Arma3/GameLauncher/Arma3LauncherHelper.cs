using System.Text.Json;
using System.Xml.Linq;
using GameRealisticMap.Arma3.Assets;

namespace GameRealisticMap.Arma3.GameLauncher
{
    public static class Arma3LauncherHelper
    {
        public static async Task CreateLauncherPresetAsync(List<ModDependencyDefinition> dependencies, string localModpath, string presetName)
        {
            // Ensure that local mod is registred within launcher
            Arma3LauncherLocalMods mods;
            var local = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Arma 3 Launcher", "Local.json");
            if (File.Exists(local))
            {
                using (var stream = File.OpenRead(local))
                {
                    mods = (await JsonSerializer.DeserializeAsync<Arma3LauncherLocalMods>(stream))!;
                }
                if (!mods.knownLocalMods.Contains(localModpath, StringComparer.OrdinalIgnoreCase))
                {
                    mods.knownLocalMods.Add(localModpath);
                    mods.userDirectories.Add(localModpath);
                    using (var stream = File.Create(local))
                    {
                        await JsonSerializer.SerializeAsync(stream, mods);
                    }
                }
            }

            // Create a ready-to-use preset
            var directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Arma 3 Launcher", "Presets");
            var preset = Path.Combine(directory, presetName + ".preset2");
            if (!File.Exists(preset))
            {
                var doc = new XDocument(new XElement("addons-presets",
                    new XElement("last-update", DateTime.Now),
                    new XElement("published-ids",
                        dependencies.Where(d => !d.IsCdlc).Select(d => new XElement("id", "steam:" + d.SteamId))
                        .Concat(new[] { new XElement("id", "local:" + localModpath.ToUpperInvariant().TrimEnd('\\') + "\\") })),
                    new XElement("dlcs-appids")));
                Directory.CreateDirectory(directory);
                doc.Save(preset);
            }
        }
    }
}
