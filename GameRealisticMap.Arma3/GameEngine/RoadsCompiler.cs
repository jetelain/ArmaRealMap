using System.Text;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.IO;
using GameRealisticMap.ManMade;
using GameRealisticMap.ManMade.Roads;
using GameRealisticMap.ManMade.Roads.Libraries;
using GameRealisticMap.Reporting;
using GeoAPI.Geometries;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using Pmad.ProgressTracking;

namespace GameRealisticMap.Arma3.GameEngine
{
    public class RoadsCompiler
    {
        private readonly IProgressScope progress;
        private readonly IGameFileSystemWriter fileSystemWriter;
        private readonly IRoadTypeLibrary<Arma3RoadTypeInfos> roadTypeLibrary;

        public RoadsCompiler(IProgressScope progress, IGameFileSystemWriter fileSystemWriter, IRoadTypeLibrary<Arma3RoadTypeInfos> roadTypeLibrary)
        {
            this.progress = progress;
            this.fileSystemWriter = fileSystemWriter;
            this.roadTypeLibrary = roadTypeLibrary;
        }

        public void Write(IArma3MapConfig config, IReadOnlyCollection<Road> roadsList)
        {
            fileSystemWriter.CreateDirectory($"{config.PboPrefix}\\data\\roads");

            WriteRoadsLibCgf(config);
            SaveRoadsShp(config, roadsList);
        }

        private void SaveRoadsShp(IArma3MapConfig config, IReadOnlyCollection<Road> roadsList)
        {
            var features = new List<Feature>();
            foreach (var road in roadsList)
            {
                if (road.SpecialSegment == WaySpecialSegment.Bridge)
                {
                    continue;
                }
                var attributesTable = new AttributesTable();
                attributesTable.Add("ID", ((int)road.RoadType) + 1);
                attributesTable.Add("ORDER", ((int)road.RoadType) + 1);
                var path = road.Path;
                if (road.RoadType < RoadTypeId.SingleLaneDirtPath)
                {
                    path = road.Path.PreventSplines(road.Width * 1.5f);
                }
                features.Add(new Feature(path.ToLineString(p => new Coordinate(p.X + 200000, p.Y)), attributesTable));
            }
            var shapeWriter = new ShapefileDataWriter(new ShapeFileWriter(fileSystemWriter, $"{config.PboPrefix}\\data\\roads\\roads"), new GeometryFactory(), Encoding.ASCII)
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

        private void WriteRoadsLibCgf(IArma3MapConfig config)
        {
            using var writer = new StringWriter(); 
            
            writer.WriteLine(@"class RoadTypesLibrary
{");
            foreach (var id in Enum.GetValues<RoadTypeId>())
            {
                var infos = roadTypeLibrary.GetInfo(id);
                writer.WriteLine(FormattableString.Invariant(
                $@" class Road{((int)id)+1:0000}
{{
	width = {infos.TextureWidth};
	mainStrTex      = ""{infos.Texture}""; 
	mainTerTex      = ""{infos.TextureEnd}"";
	mainMat         = ""{infos.Material}"";
	map             = ""{GetMap(id)}"";
	AIpathOffset 	= {GetAIPathOffset(id)};
	pedestriansOnly = {IsPedestriansOnly(id).ToString().ToLowerInvariant()};
}};"));
            }
            writer.WriteLine(@"};");

            fileSystemWriter.WriteTextFile($"{config.PboPrefix}\\data\\roads\\roadslib.cfg", writer.ToString());
        }

        private bool IsPedestriansOnly(RoadTypeId id)
        {
            return id == RoadTypeId.Trail || id == RoadTypeId.ConcreteFootway;
        }

        private static float GetAIPathOffset(RoadTypeId id)
        {
            switch (id)
            {
                case RoadTypeId.TwoLanesMotorway:
                case RoadTypeId.TwoLanesPrimaryRoad:
                    return 1;
                case RoadTypeId.TwoLanesSecondaryRoad:
                case RoadTypeId.TwoLanesConcreteRoad:
                    return 1.5f;
                case RoadTypeId.SingleLaneDirtRoad:
                    return 2;
                case RoadTypeId.SingleLaneDirtPath:
                    return 2.5f;
                case RoadTypeId.Trail:
                case RoadTypeId.ConcreteFootway:
                default:
                    return 0;
            }
        }

        private static string GetMap(RoadTypeId id)
        {
            switch (id)
            {
                case RoadTypeId.TwoLanesMotorway:
                case RoadTypeId.TwoLanesPrimaryRoad:
                    return "main road";

                case RoadTypeId.TwoLanesSecondaryRoad:
                    return "road";

                case RoadTypeId.TwoLanesConcreteRoad:
                case RoadTypeId.SingleLaneDirtRoad:
                case RoadTypeId.SingleLaneDirtPath:
                default:
                    return "track";

                case RoadTypeId.Trail:
                case RoadTypeId.ConcreteFootway:
                    return "trail";
            }
        }
    }
}
