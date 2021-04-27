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
        public List<TBLibrary> Libraries { get; } = new List<TBLibrary>();

        public void Load(Config config)
        {
            var serializer = new XmlSerializer(typeof(TBLibrary));
            foreach(var file in Directory.GetFiles(config.Libraries, "*.tml", SearchOption.AllDirectories))
            {
                using(var stream = File.OpenRead(file))
                {
                    var lib = (TBLibrary)serializer.Deserialize(stream);
                    lib.FullPath = file;
                    Libraries.Add(lib);
                }
                
            }

        }

        public TBTemplate FindModel(string model)
        {
            return Libraries.SelectMany(l => l.Template.Where(l => string.Equals(l.File, model, StringComparison.OrdinalIgnoreCase))).FirstOrDefault();
        }


    }
}
