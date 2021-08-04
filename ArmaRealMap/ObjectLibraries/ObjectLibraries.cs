using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using ArmaRealMap.TerrainBuilder;

namespace ArmaRealMap.Libraries
{
    public class ObjectLibraries
    {
        private static readonly JsonSerializerOptions options = new JsonSerializerOptions
        {
            Converters ={
                new JsonStringEnumConverter()
            },
            WriteIndented = true
        };


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

            var jsons = new HashSet<string>(Directory.GetFiles(config.Libraries, "*.json"), StringComparer.OrdinalIgnoreCase);
            foreach (var file in Directory.GetFiles(config.Libraries, "*.txt"))
            {
                var name = Path.GetFileNameWithoutExtension(file);
                if (Enum.TryParse(name, out ObjectCategory category))
                {
                    Libraries.Add(ParseText(file, category, jsons));
                }
            }
            foreach (var json in jsons)
            {
                var lib = JsonSerializer.Deserialize<ObjectLibrary>(File.ReadAllText(json), options);
                if (lib.Terrain == null || lib.Terrain == config.Terrain)
                {
                    Libraries.Add(lib);
                }
            }
        }

        internal SingleObjetInfos GetObject(string name)
        {
            return Libraries.SelectMany(l => l.Objects).First(o => o.Name == name);
        }

        private ObjectLibrary ParseText(string file, ObjectCategory categor, HashSet<string> jsons)
        {
            var json = Path.ChangeExtension(file, ".json");
            ObjectLibrary lib;
            if (File.Exists(json))
            {
                lib = JsonSerializer.Deserialize<ObjectLibrary>(File.ReadAllText(json), options);
                jsons.Remove(json);
            }
            else
            {
                lib = new ObjectLibrary() { Category = categor } ;
            }
            lib.Objects = lib.Objects ?? new List<SingleObjetInfos>();
            lib.Compositions = lib.Compositions ?? new List<CompositionInfos>();
            foreach (var line in File.ReadAllLines(file))
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

                    var template = TerrainBuilder.FindByModel(model.TrimStart('\\'));

                    var obj = new SingleObjetInfos();
                    obj.Name = template?.Name ?? Path.GetFileNameWithoutExtension(model);

                    obj.Width = maxX - minX;
                    obj.Depth = maxY - minY;

                    obj.CX = posX + maxX - (obj.Width / 2);
                    obj.CY = posY + maxY - (obj.Depth / 2);

                    lib.Objects.RemoveAll(o => o.Name == obj.Name);
                    lib.Objects.Add(obj);
                }
            }

            File.WriteAllText(Path.ChangeExtension(file, ".json"), JsonSerializer.Serialize(lib, options));
            File.Move(file, Path.ChangeExtension(file, ".txt.old"));
            return lib;
        }
    }
}
