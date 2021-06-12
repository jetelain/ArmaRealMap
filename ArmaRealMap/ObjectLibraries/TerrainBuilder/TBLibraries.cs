using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace ArmaRealMap.TerrainBuilder
{
    public class TBLibraries
    {
        public static readonly XmlSerializer Serializer = new XmlSerializer(typeof(TBLibrary));

        public List<TBLibrary> Libraries { get; } = new List<TBLibrary>();

        public void Load(Config config)
        {
            LoadAllFrom(config.Libraries);
        }

        public void LoadAllFrom(string path)
        {
            foreach(var file in Directory.GetFiles(path, "*.tml", SearchOption.AllDirectories))
            {
                using(var stream = File.OpenRead(file))
                {
                    var lib = (TBLibrary)Serializer.Deserialize(stream);
                    lib.FullPath = file;
                    Libraries.Add(lib);
                }
                
            }
        }

        public TBTemplate FindByModel(string model)
        {
            return Libraries.SelectMany(l => l.Template.Where(l => string.Equals(l.File, model, StringComparison.OrdinalIgnoreCase))).FirstOrDefault();
        }

        public TBTemplate FindByName(string name)
        {
            return Libraries.SelectMany(l => l.Template.Where(l => string.Equals(l.Name, name, StringComparison.OrdinalIgnoreCase))).FirstOrDefault();
        }

        public string GetAllSqf()
        {
            return string.Join(Environment.NewLine, Libraries.Select(t => t.Name + "=" + t.GetAllFilesAsSqfArray()+ ";")) + Environment.NewLine + $"libraries=[{string.Join("," + Environment.NewLine, Libraries.Select(t => t.Name))}];";
        }
    }
}
