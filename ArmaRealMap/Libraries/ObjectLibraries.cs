using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using ArmaRealMap.TerrainBuilder;

namespace ArmaRealMap.Libraries
{
    public class ObjectLibraries
    {
        private static readonly Regex TextLine = new Regex(@"\[""([^""]+)"",\[([0-9\.\-]+),([0-9\.\-]+),([0-9\.\-]+)\],\[\[([0-9\.\-]+),([0-9\.\-]+),([0-9\.\-]+)\],\[([0-9\.\-]+),([0-9\.\-]+),([0-9\.\-]+)\],[0-9\.\-]+\]\]", RegexOptions.Compiled);
        public List<ObjectLibrary> Libraries { get; } = new List<ObjectLibrary>();

        public ObjectLibraries()
        {
            TerrainBuilder = new TBLibraries();
        }

        public TBLibraries TerrainBuilder { get; }

        public void Load(Config config)
        {
            TerrainBuilder.Load(config);

            foreach (var file in Directory.GetFiles(config.Libraries, "*.txt"))
            {
                var name = Path.GetFileNameWithoutExtension(file);
                if (Enum.TryParse(name, out BuildingCategory category))
                {
                    Libraries.Add(ParseText(file, category));
                }
            }
        }

        private ObjectLibrary ParseText(string file, BuildingCategory category)
        {
            var lib = new ObjectLibrary();
            lib.Category = category;
            lib.Objects = new List<ObjetInfos>();
            foreach(var line in File.ReadAllLines(file))
            {
                var math = TextLine.Match(line);
                if ( math.Success)
                {
                    var model = math.Groups[1].Value;

                    var posX = float.Parse(math.Groups[2].Value, CultureInfo.InvariantCulture);
                    var posY = float.Parse(math.Groups[3].Value, CultureInfo.InvariantCulture);
                    var posZ = float.Parse(math.Groups[4].Value, CultureInfo.InvariantCulture);

                    var minX = float.Parse(math.Groups[5].Value, CultureInfo.InvariantCulture);
                    var minY = float.Parse(math.Groups[6].Value, CultureInfo.InvariantCulture);
                    var minZ = float.Parse(math.Groups[7].Value, CultureInfo.InvariantCulture);

                    var maxX = float.Parse(math.Groups[8].Value, CultureInfo.InvariantCulture);
                    var maxY = float.Parse(math.Groups[9].Value, CultureInfo.InvariantCulture);
                    var maxZ = float.Parse(math.Groups[10].Value, CultureInfo.InvariantCulture);

                    var template = TerrainBuilder.FindModel(model.TrimStart('\\'));

                    var obj = new ObjetInfos();
                    obj.Name = template?.Name ?? Path.GetFileNameWithoutExtension(model);

                    obj.Width = maxX - minX;
                    obj.Depth = maxY - minY;

                    obj.CX = posX + maxX - (obj.Width / 2);
                    obj.CY = posY + maxY - (obj.Depth / 2);

                    lib.Objects.Add(obj);
                }
            }
            return lib;
        }
    }
}
