using GameRealisticMap.Arma3.GameEngine.Roads;
using GameRealisticMap.Arma3.Test.GameEngine;
using GameRealisticMap.Geometries;

namespace GameRealisticMap.Arma3.Test.GameEngine.Roads
{
    public class RoadsSerializerTest
    {
        private const string BasePath = @"prefix\data\roads";

        private static EditableArma3RoadTypeInfos MakeType(int id, float width, string texture, string textureEnd, string material, string map, float pathOffset, bool pedestriansOnly)
            => new EditableArma3RoadTypeInfos(id, width, texture, textureEnd, material, map, pathOffset, pedestriansOnly);

        private static EditableArma3Road MakeRoad(int order, EditableArma3RoadTypeInfos type, params TerrainPoint[] points)
            => new EditableArma3Road(order, type, new TerrainPath(points.ToList()));

        private static GameFileSystemMock Serialize(IEnumerable<EditableArma3Road> roads, IEnumerable<EditableArma3RoadTypeInfos> types)
        {
            var fs = new GameFileSystemMock();
            fs.CreateDirectory(BasePath);
            new RoadsSerializer(fs).Serialize(BasePath, roads, types);
            return fs;
        }

        [Fact]
        public void Serialize_WritesExpectedFiles()
        {
            var type1 = MakeType(1, 12.5f, "tex.paa", "tex_end.paa", "mat.rvmat", "main road", 1f, false);
            var road1 = MakeRoad(0, type1, new TerrainPoint(100, 200), new TerrainPoint(300, 400));

            var fs = Serialize(new[] { road1 }, new[] { type1 });

            Assert.True(fs.TextFiles.ContainsKey($@"{BasePath}\roadslib.cfg"));
            Assert.True(fs.BinaryFiles.ContainsKey($@"{BasePath}\roads.shp"));
            Assert.True(fs.BinaryFiles.ContainsKey($@"{BasePath}\roads.dbf"));
            Assert.True(fs.BinaryFiles.ContainsKey($@"{BasePath}\roads.shx"));
        }

        [Fact]
        public void Serialize_RoadsLibCfg_ContainsExpectedContent()
        {
            var type1 = MakeType(1, 12.5f,
                @"a3\roads_f\roads_ae\data\surf_roadtarmac_highway_ca.paa",
                @"a3\roads_f\roads_ae\data\surf_roadtarmac_highway_end_ca.paa",
                @"a3\roads_f\roads_ae\data\surf_roadtarmac_highway.rvmat",
                "main road", 1f, false);

            var fs = Serialize(Array.Empty<EditableArma3Road>(), new[] { type1 });

            var cfg = fs.TextFiles[$@"{BasePath}\roadslib.cfg"];
            Assert.Contains("class RoadTypesLibrary", cfg);
            Assert.Contains("class Road0001", cfg);
            Assert.Contains("width           = 12.5", cfg);
            Assert.Contains(@"a3\roads_f\roads_ae\data\surf_roadtarmac_highway_ca.paa", cfg);
            Assert.Contains("main road", cfg);
            Assert.Contains("pedestriansOnly = false", cfg);
        }

        [Fact]
        public void Serialize_SkipsRoadsWithFewerThanTwoPoints()
        {
            var type1 = MakeType(1, 12.5f, "tex.paa", "tex_end.paa", "mat.rvmat", "main road", 1f, false);
            var singlePointRoad = MakeRoad(0, type1, new TerrainPoint(100, 200));
            var validRoad = MakeRoad(0, type1, new TerrainPoint(0, 0), new TerrainPoint(1, 1));

            var fs = Serialize(new[] { singlePointRoad, validRoad }, new[] { type1 });

            // Only the valid road is written; the single-point road is skipped
            var deserializer = new RoadsDeserializer(fs);
            var result = deserializer.Deserialize(BasePath);
            Assert.Single(result.Roads);
        }

        [Fact]
        public void Serialize_FullRoadsLib_ContainsAllNineTypes()
        {
            var types = new[]
            {
                MakeType(1, 12.5f,
                    @"a3\roads_f\roads_ae\data\surf_roadtarmac_highway_ca.paa",
                    @"a3\roads_f\roads_ae\data\surf_roadtarmac_highway_end_ca.paa",
                    @"a3\roads_f\roads_ae\data\surf_roadtarmac_highway.rvmat",
                    "main road", 1f, false),
                MakeType(2, 12.5f,
                    @"a3\roads_f\roads_ae\data\surf_roadtarmac_highway_ca.paa",
                    @"a3\roads_f\roads_ae\data\surf_roadtarmac_highway_end_ca.paa",
                    @"a3\roads_f\roads_ae\data\surf_roadtarmac_highway.rvmat",
                    "main road", 1f, false),
                MakeType(3, 9f,
                    @"a3\structures_f_exp\data\roads\surf_exp_roadtarmac_main_road_ca.paa",
                    @"a3\structures_f_exp\data\roads\surf_exp_roadtarmac_main_road_end_ca.paa",
                    @"a3\structures_f_exp\data\roads\surf_exp_roadtarmac_main_road.rvmat",
                    "road", 1.5f, false),
                MakeType(4, 8.5f,
                    @"a3\roads_f\roads_ae\data\surf_roadconcrete_city_road_ca.paa",
                    @"a3\roads_f\roads_ae\data\surf_roadconcrete_city_road_end_ca.paa",
                    @"a3\roads_f\roads_ae\data\surf_roadconcrete_city_road.rvmat",
                    "track", 1.5f, false),
                MakeType(5, 5f,
                    @"a3\roads_f\roads_ae\data\surf_roadconcrete_city_road_ca.paa",
                    @"a3\roads_f\roads_ae\data\surf_roadconcrete_city_road_end_ca.paa",
                    @"a3\roads_f\roads_ae\data\surf_roadconcrete_city_road.rvmat",
                    "track", 0f, false),
                MakeType(6, 5f,
                    @"a3\roads_f\roads_ae\data\surf_roaddirt_road_ca.paa",
                    @"a3\roads_f\roads_ae\data\surf_roaddirt_road_end_ca.paa",
                    @"a3\roads_f\roads_ae\data\surf_roaddirt_road.rvmat",
                    "track", 2f, false),
                MakeType(7, 4.5f,
                    @"a3\roads_f\roads_ae\data\surf_roaddirt_path_ca.paa",
                    @"a3\roads_f\roads_ae\data\surf_roaddirt_path_end_ca.paa",
                    @"a3\roads_f\roads_ae\data\surf_roaddirt_path.rvmat",
                    "track", 2.5f, false),
                MakeType(8, 2.5f,
                    @"a3\roads_f\roads_ae\data\surf_roadconcrete_city_road_ca.paa",
                    @"a3\roads_f\roads_ae\data\surf_roadconcrete_city_road_end_ca.paa",
                    @"a3\roads_f\roads_ae\data\surf_roadconcrete_city_road.rvmat",
                    "trail", 0f, true),
                MakeType(9, 2f,
                    @"a3\structures_f_exp\data\roads\surf_exp_traildirt_trail_ca.paa",
                    @"a3\structures_f_exp\data\roads\surf_exp_traildirt_trail_end_ca.paa",
                    @"a3\structures_f_exp\data\roads\surf_exp_traildirt_trail.rvmat",
                    "trail", 0f, true),
            };

            var fs = Serialize(Array.Empty<EditableArma3Road>(), types);

            var cfg = fs.TextFiles[$@"{BasePath}\roadslib.cfg"];
            Assert.Contains("class RoadTypesLibrary", cfg);
            for (var i = 1; i <= 9; i++)
            {
                Assert.Contains($"class Road{i:0000}", cfg);
            }
            Assert.Contains("width           = 12.5", cfg);
            Assert.Contains("width           = 9", cfg);
            Assert.Contains("width           = 8.5", cfg);
            Assert.Contains("width           = 4.5", cfg);
            Assert.Contains("width           = 2.5", cfg);
            Assert.Contains("width           = 2", cfg);
            Assert.Contains("map             = \"main road\"", cfg);
            Assert.Contains("map             = \"road\"", cfg);
            Assert.Contains("map             = \"track\"", cfg);
            Assert.Contains("map             = \"trail\"", cfg);
            Assert.Contains("pedestriansOnly = false", cfg);
            Assert.Contains("pedestriansOnly = true", cfg);
            Assert.Contains("AIpathOffset 	= 2.5", cfg);
            Assert.Equal(RoadsDeserializerTest.FullSampleRoadsLibCfg, cfg, ignoreLineEndingDifferences: true, ignoreWhiteSpaceDifferences: true);
        }

        [Fact]
        public void GetFilenames_ReturnsExpectedPaths()
        {
            var files = RoadsSerializer.GetFilenames(@"prefix\data\roads").ToList();
            Assert.Equal(4, files.Count);
            Assert.Contains(@"prefix\data\roads\roadslib.cfg", files);
            Assert.Contains(@"prefix\data\roads\roads.dbf", files);
            Assert.Contains(@"prefix\data\roads\roads.shp", files);
            Assert.Contains(@"prefix\data\roads\roads.shx", files);
        }
    }
}
