using System.Xml.Serialization;

namespace GameRealisticMap.Arma3.TerrainBuilder.TmlFiles
{
    public class TmlGenerator
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
            return GetBundle(a.Path);
        }

        internal static string GetBundle(string path)
        {
            if (path.StartsWith("a3\\"))
            {
                return "arma3";
            }
            if (path.StartsWith("z\\arm\\"))
            {
                return "arm";
            }
            var i = path.IndexOf('\\');
            if (i == -1)
            {
                return "none";
            }
            if (i < 5)
            {
                var last = path.LastIndexOf('\\'); // can't be -1, as IndexOf('\\') was different of -1
                return path.Substring(0, last).Replace("\\", "_");
            }
            return path.Substring(0, i);
        }
    }
}
