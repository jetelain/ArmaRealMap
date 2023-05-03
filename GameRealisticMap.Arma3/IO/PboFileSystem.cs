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

        private readonly IEnumerable<string> searchPaths;
        private readonly Dictionary<string, IPBOFileEntry> index = new(StringComparer.OrdinalIgnoreCase);

        public PboFileSystem(IEnumerable<string> searchPaths)
        {
            this.searchPaths = searchPaths;
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

        public void BuildIndex()
        {
            foreach(var path in searchPaths)
            {
                foreach(var pboPath in Directory.GetFiles(path, "*.pbo", SearchOption.AllDirectories))
                {
                    var fileName = Path.GetFileName(pboPath);
                    if (PboPrefixIgnore.Any(suffix => fileName.StartsWith(suffix, StringComparison.OrdinalIgnoreCase)))
                    {
                        continue;
                    }

                    var pboFile = new PBO(pboPath, false);
                    foreach(var entry in pboFile.Files)
                    {
                        if (EntryExtensionsList.Any(suffix => entry.FileName.EndsWith(suffix, StringComparison.OrdinalIgnoreCase)))
                        {
                            index[pboFile.Prefix + "\\" + entry.FileName] = entry;
                        }
                    }
                }
            }
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
    }
}
