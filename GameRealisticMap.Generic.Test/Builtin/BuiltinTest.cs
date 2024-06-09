using GameRealisticMap.Generic.Exporters;
using GameRealisticMap.Generic.Profiles;

namespace GameRealisticMap.Generic.Test.Builtin
{
    public class BuiltinTest
    {
        [Fact]
        public async Task ExporterExistsAndSupportsFormat()
        {
            var catalog = new ExporterCatalog();
           foreach(var resource in typeof(ExportProfile).Assembly.GetManifestResourceNames())
           {
                if (resource.StartsWith(ExportProfile.BuiltinNamespace))
                {
                    var profile = await ExportProfile.LoadFromFile(ExportProfile.BuiltinPrefix + resource.Substring(ExportProfile.BuiltinNamespace.Length));
                    Assert.NotNull(profile);
                    foreach(var entry in profile.Entries)
                    {
                        var exporter = catalog.Get(entry.Exporter);
                        Assert.NotNull(entry.Format);
                        Assert.False(string.IsNullOrEmpty(entry.FileName));
                        Assert.Contains(entry.Format!.Value, exporter.Formats);
                    }
                }
           }
        }
    }
}
