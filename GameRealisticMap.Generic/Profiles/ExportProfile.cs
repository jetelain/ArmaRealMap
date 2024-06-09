using System.Text.Json;
using System.Text.Json.Serialization;

namespace GameRealisticMap.Generic.Profiles
{
    public class ExportProfile
    {
        public const string Default = BuiltinPrefix + "All.GeoJson.grmep";
        public const string BuiltinPrefix = "builtin:";
        internal const string BuiltinNamespace = "GameRealisticMap.Generic.Builtin.";

        public ExportProfile(List<ExportEntry> entries)
        {
            Entries = entries;
        }

        public List<ExportEntry> Entries { get; }


        public static List<string> GetBuiltinList()
        {
            return typeof(ExportProfile).Assembly.GetManifestResourceNames()
                .Where(r => r.StartsWith(BuiltinNamespace, StringComparison.Ordinal))
                .Select(r => BuiltinPrefix + r.Substring(BuiltinNamespace.Length))
                .OrderBy(r => r)
                .ToList();
        }

        public static JsonSerializerOptions JsonOptions => new JsonSerializerOptions() {
                Converters = { new JsonStringEnumConverter() },
                WriteIndented = true
            };
        
        public static async Task<ExportProfile> LoadFromFile(string path)
        {
            ExportProfile? assets = null;

            if (path.StartsWith(BuiltinPrefix))
            {
                var filename = path.Substring(BuiltinPrefix.Length);
                using (var source = typeof(ExportProfile).Assembly.GetManifestResourceStream(BuiltinNamespace + filename))
                {
                    if (source == null)
                    {
                        throw new FileNotFoundException($"Builtin file '{filename}' does not exists (name is case sensitive).");
                    }
                    assets = await JsonSerializer.DeserializeAsync<ExportProfile>(source, JsonOptions);
                }
            }
            else
            {
                using (var source = File.OpenRead(path))
                {
                    assets = await JsonSerializer.DeserializeAsync<ExportProfile>(source, JsonOptions);
                }
            }
            if (assets == null)
            {
                throw new IOException("Invalid JSON file");
            }
            return assets;
        }

    }
}
