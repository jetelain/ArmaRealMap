using System.Text;
using GameRealisticMap.Arma3.IO;
using GeoAPI.Geometries;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;

namespace GameRealisticMap.Arma3.GameEngine.Roads
{
    public sealed class RoadsSerializer
    {
        internal const int XShift = 200000;

        private readonly IGameFileSystemWriter fileSystemWriter;

        public RoadsSerializer(IGameFileSystemWriter fileSystemWriter)
        {
            this.fileSystemWriter = fileSystemWriter;
        }

        public static IEnumerable<string> GetFilenames(string basePath)
        {
            yield return $"{basePath}\\roadslib.cfg";
            yield return $"{basePath}\\roads.dbf";
            yield return $"{basePath}\\roads.shp";
            yield return $"{basePath}\\roads.shx";
        }

        public void Serialize(IArma3MapConfig config, IEnumerable<IArma3Road> roadsList, IEnumerable<IArma3RoadTypeInfos> typeInfos)
        {
            var basePath = $"{config.PboPrefix}\\data\\roads";

            SaveRoadsShp(basePath, roadsList);
            WriteRoadsLibCgf(basePath, typeInfos);
        }

        public void Serialize(string basePath, IEnumerable<IArma3Road> roadsList, IEnumerable<IArma3RoadTypeInfos> typeInfos)
        {
            SaveRoadsShp(basePath, roadsList);
            WriteRoadsLibCgf(basePath, typeInfos);
        }

        private void SaveRoadsShp(string basePath, IEnumerable<IArma3Road> roadsList)
        {
            var features = new List<Feature>();
            foreach (var road in roadsList)
            {
                var attributesTable = new AttributesTable
                {
                    { "ID", road.TypeInfos.Id },
                    { "ORDER", road.Order }
                }; 
                if (road.Path.Points.Count < 2)
                {
                    // Skip roads with less than 2 points
                    // This should not happen, but it is better to skip them than to crash
                    continue;
                }
                features.Add(new Feature(road.Path.ToLineString(p => new Coordinate(p.X + XShift, p.Y)), attributesTable));
            }
            var shapeWriter = new ShapefileDataWriter(new ShapeFileWriter(fileSystemWriter, $"{basePath}\\roads"), new GeometryFactory(), Encoding.ASCII)
            {
                Header = GetHeader(features)
            };
            shapeWriter.Write(features);
        }

        private static DbaseFileHeader GetHeader(List<Feature> features)
        {
            if (features.Count == 0)
            {
                DbaseFileHeader dbaseFileHeader = new DbaseFileHeader(Encoding.ASCII)
                {
                    NumRecords = features.Count
                };
                dbaseFileHeader.AddColumn("ID", 'N', 10, 0);
                return dbaseFileHeader;
            }
            return ShapefileDataWriter.GetHeader(features.First(), features.Count, Encoding.ASCII);
        }

        private void WriteRoadsLibCgf(string basePath, IEnumerable<IArma3RoadTypeInfos> typeInfos)
        {
            using var writer = new StringWriter();

            writer.WriteLine(@"class RoadTypesLibrary
{");
            foreach (var infos in typeInfos)
            {
                writer.WriteLine(FormattableString.Invariant(
                $@"class Road{infos.Id:0000}
{{
	width           = {infos.TextureWidth};
	mainStrTex      = ""{infos.Texture}""; 
	mainTerTex      = ""{infos.TextureEnd}"";
	mainMat         = ""{infos.Material}"";
	map             = ""{infos.Map}"";
	AIpathOffset 	= {infos.PathOffset};
	pedestriansOnly = {infos.IsPedestriansOnly.ToString().ToLowerInvariant()};
}};"));
            }
            writer.WriteLine(@"};");
            fileSystemWriter.WriteTextFile($"{basePath}\\roadslib.cfg", writer.ToString());
        }
    }
}
