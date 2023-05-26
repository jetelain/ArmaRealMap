using System.Runtime.Versioning;
using System.Text.RegularExpressions;
using BIS.PBO;

namespace GameRealisticMap.Arma3.IO
{
    public class PboFileSystem : IGameFileSystem
    {
        private static readonly string[] EntryExtensionsList = new[] { ".p3d", ".paa", ".rvmat" };
        private static readonly string[] PboPrefixIgnore = new[] { 
            "map_", "missions_", "sounds_", "musics_", "dubbing_", 
            "editorpreviews_", "ui_", "functions_", "anims_", "air_", "armor_", "characters_", "boat_"
        };

        private readonly IEnumerable<string> gamePaths;
        private readonly IEnumerable<string> mods;
        private readonly Dictionary<string, IPBOFileEntry> index = new(StringComparer.OrdinalIgnoreCase);
        
        public PboFileSystem(IEnumerable<string> gamePaths, IEnumerable<string> mods)
        {
            this.gamePaths = gamePaths;
            this.mods = mods;
        }

        [SupportedOSPlatform("windows")]
        public PboFileSystem()
            : this(GetArma3Paths(), Enumerable.Empty<string>())
        {
        }

        public static IEnumerable<string> GetArma3Paths(string basePath)
        {
            yield return Path.Combine(basePath, "Addons");
            yield return Path.Combine(basePath, "AoW");
            yield return Path.Combine(basePath, "Argo");
            yield return Path.Combine(basePath, "Curator");
            yield return Path.Combine(basePath, "Dta");
            yield return Path.Combine(basePath, "Enoch");
            yield return Path.Combine(basePath, "Expansion");
            yield return Path.Combine(basePath, "Heli");
            yield return Path.Combine(basePath, "Jets");
            yield return Path.Combine(basePath, "Kart");
            yield return Path.Combine(basePath, "Mark");
            yield return Path.Combine(basePath, "Orange");
            yield return Path.Combine(basePath, "Tank");
        }

        [SupportedOSPlatform("windows")]
        public static IEnumerable<string> GetArma3Paths()
        {
            var basePath = Arma3ToolsHelper.GetArma3Path();
            if (!string.IsNullOrEmpty(basePath))
            {
                return GetArma3Paths(basePath);
            }
            return Enumerable.Empty<string>();
        }

        public void BuildIndex()
        {
            foreach(var path in gamePaths)
            {
                foreach(var pboPath in Directory.GetFiles(path, "*.pbo", SearchOption.AllDirectories).Where(f => ShouldRead(f, PboPrefixIgnore)))
                {
                    AddCacheDefault(pboPath);
                }
            }

            foreach (var path in mods)
            {
                foreach (var pboPath in Directory.GetFiles(path, "*.pbo", SearchOption.AllDirectories))
                {
                    AddCacheDefault(pboPath);
                }
            }
        }

        private void AddCacheDefault(string pboPath)
        {
            var pboFile = new PBO(pboPath, false);
            foreach (var entry in pboFile.Files.Where(ShouldBeCached))
            {
                index[pboFile.Prefix + "\\" + entry.FileName] = entry;
            }
        }

        private bool ShouldRead(string file, string[] ignorePrefix)
        {
            var fileName = Path.GetFileName(file);
            return !ignorePrefix.Any(suffix => fileName.StartsWith(suffix, StringComparison.OrdinalIgnoreCase));
        }

        private static bool ShouldBeCached(IPBOFileEntry entry)
        {
            return EntryExtensionsList.Any(suffix => entry.FileName.EndsWith(suffix, StringComparison.OrdinalIgnoreCase));
        }

        public bool Exists(string path)
        {
            LazyBuildIndex();
            return index.ContainsKey(path);
        }

        private void LazyBuildIndex()
        {
            if (index.Count == 0)
            {
                BuildIndex();
            }
        }

        public Stream? OpenFileIfExists(string path)
        {
            LazyBuildIndex();
            if (index.TryGetValue(path, out var entry))
            {
                return entry.OpenRead();
            }

            return null;
        }

        public IEnumerable<string> FindAll(string pattern)
        {
            var regex = new Regex("\\\\" + Regex.Escape(pattern).Replace("\\*", ".*") + "$", RegexOptions.IgnoreCase);
            var extension = Path.GetExtension(pattern);
            if (EntryExtensionsList.Contains(extension, StringComparer.OrdinalIgnoreCase))
            {
                LazyBuildIndex();
                return index.Keys.Where(v => regex.IsMatch(v));
            }
            var isJpeg = string.Equals(extension, ".jpg", StringComparison.OrdinalIgnoreCase);
            var result = new List<string>();
            foreach (var path in gamePaths)
            {
                foreach (var pboPath in Directory.GetFiles(path, "*.pbo", SearchOption.AllDirectories))
                {
                    if (isJpeg && !Path.GetFileName(pboPath).StartsWith("editorpreviews_"))
                    {
                        continue;
                    }
                    FindAll(regex, result, pboPath);
                }
            }
            foreach (var path in mods)
            {
                foreach (var pboPath in Directory.GetFiles(path, "*.pbo", SearchOption.AllDirectories))
                {
                    FindAll(regex, result, pboPath);
                }
            }
            return result.Distinct();
        }

        private void FindAll(Regex regex, List<string> result, string pboPath)
        {
            var pboFile = new PBO(pboPath, false);

            foreach (var entry in pboFile.Files)
            {
                var entryPath = pboFile.Prefix + "\\" + entry.FileName;

                if (regex.IsMatch(entryPath))
                {
                    index[entryPath] = entry;
                    result.Add(entryPath);
                }
            }
        }
    }
}
