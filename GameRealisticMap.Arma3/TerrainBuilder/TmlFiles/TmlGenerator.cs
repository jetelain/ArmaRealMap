using System.Xml.Serialization;

namespace GameRealisticMap.Arma3.TerrainBuilder.TmlFiles
{
    internal class TmlGenerator
    {
        public static readonly XmlSerializer Serializer = new XmlSerializer(typeof(TmlLibrary));

        public void WriteLibrariesTo(IEnumerable<ModelInfo> models, string targetDirectory)
        {
            Directory.CreateDirectory(targetDirectory);

            foreach (var bundle in models.GroupBy(a => GetBundle(a)))
            {
                var lib = new TmlLibrary()
                {
                    Name = bundle.Key,
                    Template =
                        bundle.Select(m => new TmlTemplate()
                        {
                            Name = m.Name,
                            File = m.Path,
                            BoundingCenter = new TmlVector() { X = -999.0000f, Y = -999.0000f, Z = -999.0000f },
                            BoundingMax = new TmlVector() { X = -999.0000f, Y = -999.0000f, Z = -999.0000f },
                            BoundingMin = new TmlVector() { X = 999.0000f, Y = 999.0000f, Z = 999.0000f },
                            Height = 0,
                            Hash = TmlTemplate.GenerateHash(m.Name)
                        }).ToList()
                };
                using (var output = new StreamWriter(Path.Combine(targetDirectory, lib.Name + ".tml"), false))
                {
                    Serializer.Serialize(output, lib);
                }
            }
        }

        private string GetBundle(ModelInfo a)
        {
            if (a.Path.StartsWith("a3\\"))
            {
                return "arma3";
            }
            if (a.Path.StartsWith("z\\arm\\"))
            {
                return "arm";
            }
            var i = a.Path.IndexOf('\\');
            if (i == -1)
            {
                return "none";
            }
            if (i < 5)
            {
                return Path.GetDirectoryName(a.Path)!.Replace("\\", "_");
            }
            return a.Path.Substring(i);
        }
    }
}
