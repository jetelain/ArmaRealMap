using System.Globalization;
using System.Text.RegularExpressions;
using GameRealisticMap.Arma3.IO;
using GameRealisticMap.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;

namespace GameRealisticMap.Arma3.GameEngine.Roads
{
    public sealed class RoadsDeserializer
    {
        internal const int XShift = RoadsSerializer.XShift;

        private readonly IGameFileSystem fileSystem;

        private readonly Regex roadClassRegex = new ("class\\s*Road([0-9]+)\\s*{([^}]+)}");
        private readonly Regex propertyRegex = new("([a-zA-Z]+)\\s*=\\s*([^;]+);");

        public RoadsDeserializer(IGameFileSystem fileSystem)
        {
            this.fileSystem = fileSystem;
        }

        public EditableArma3Roads Deserialize(string basePath)
        {
            var result = new EditableArma3Roads();

            var roadslib = fileSystem.ReadAllText($"{basePath}\\roadslib.cfg");
            foreach (Match match in roadClassRegex.Matches(roadslib))
            {
                var id = int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
                result.RoadTypeInfos.Add(CreateTypeInfos(id, match.Groups[2].Value));
            }

            using (var shapeReader = new ShapefileDataReader(new ShapeFileReader(fileSystem, $"{basePath}\\roads"), new GeometryFactory()))
            {
                var idIndex = Array.FindIndex(shapeReader.DbaseHeader.Fields, i => string.Equals(i.Name, "ID", StringComparison.OrdinalIgnoreCase));
                var orderIndex = Array.FindIndex(shapeReader.DbaseHeader.Fields, i => string.Equals(i.Name, "ORDER", StringComparison.OrdinalIgnoreCase));
                var shift = shapeReader.FieldCount - shapeReader.DbaseHeader.NumFields;
                while (shapeReader.Read())
                {
                    if (shapeReader.Geometry is LineString line)
                    {
                        var path = new TerrainPath(line.Coordinates.Select(p => new TerrainPoint((float)(p.X - XShift),(float)p.Y)).ToList());
                        var id = shapeReader.GetInt32(idIndex + shift);
                        var order = orderIndex != -1 ? shapeReader.GetInt32(orderIndex + shift) : 0;
                        var type = result.RoadTypeInfos.First(i => i.Id == id);
                        result.Roads.Add(new EditableArma3Road(order, type, path));
                    }
                }
            }
            return result;
        }

        private EditableArma3RoadTypeInfos CreateTypeInfos(int id, string config)
        {
            var dict = new Dictionary<string, string>();
            foreach (Match match in propertyRegex.Matches(config))
            {
                dict.Add(match.Groups[1].Value, match.Groups[2].Value.Trim('\'', '"'));
            }
            return new EditableArma3RoadTypeInfos(id,
                GetFloat(dict, "width"),
                Get(dict, "mainStrTex"),
                Get(dict, "mainTerTex"),
                Get(dict, "mainMat"),
                Get(dict, "map"),
                GetFloat(dict, "AIpathOffset"),
                GetBoolean(dict, "pedestriansOnly"));
        }

        private static bool GetBoolean(Dictionary<string, string> dict, string v)
        {
            return bool.TryParse(Get(dict, v), out var value) ? value : false;
        }

        private static string Get(Dictionary<string, string> dict, string v)
        {
            return dict.TryGetValue(v, out var value) ? value : string.Empty;
        }

        private static float GetFloat(Dictionary<string, string> dict, string v)
        {
            return float.TryParse(Get(dict, v), NumberStyles.Any, CultureInfo.InvariantCulture, out var value) ? value : 0;
        }
    }
}
