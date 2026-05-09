using GameRealisticMap.Arma3.GameEngine.Roads;
using GameRealisticMap.Arma3.Test.GameEngine;
using GameRealisticMap.Geometries;

namespace GameRealisticMap.Arma3.Test.GameEngine.Roads
{
    public class RoadsDeserializerTest
    {
        private const string BasePath = @"prefix\data\roads";

        private static EditableArma3RoadTypeInfos MakeType(int id, float width, string texture, string textureEnd, string material, string map, float pathOffset, bool pedestriansOnly)
            => new EditableArma3RoadTypeInfos(id, width, texture, textureEnd, material, map, pathOffset, pedestriansOnly);

        private static EditableArma3Road MakeRoad(int order, EditableArma3RoadTypeInfos type, params TerrainPoint[] points)
            => new EditableArma3Road(order, type, new TerrainPath(points.ToList()));

        private static EditableArma3Roads RoundTrip(IEnumerable<EditableArma3Road> roads, IEnumerable<EditableArma3RoadTypeInfos> types)
        {
            var fs = new GameFileSystemMock();
            fs.CreateDirectory(BasePath);
            new RoadsSerializer(fs).Serialize(BasePath, roads, types);
            return new RoadsDeserializer(fs).Deserialize(BasePath);
        }

        // Serialize with one valid dummy road to avoid empty shapefile NullReferenceException in NTS reader,
        // then deserialize to test roadslib.cfg parsing.
        private static EditableArma3Roads RoundTripTypesOnly(IEnumerable<EditableArma3RoadTypeInfos> types)
        {
            var dummyType = types.First();
            var dummyRoad = MakeRoad(0, dummyType, new TerrainPoint(0, 0), new TerrainPoint(1, 1));
            return RoundTrip(new[] { dummyRoad }, types);
        }

        [Fact]
        public void Deserialize_TypeInfos_ArePreserved()
        {
            var type1 = MakeType(1, 12.5f,
                @"a3\roads_f\roads_ae\data\surf_roadtarmac_highway_ca.paa",
                @"a3\roads_f\roads_ae\data\surf_roadtarmac_highway_end_ca.paa",
                @"a3\roads_f\roads_ae\data\surf_roadtarmac_highway.rvmat",
                "main road", 1f, false);
            var type2 = MakeType(8, 2.5f,
                @"a3\roads_f\roads_ae\data\surf_roadconcrete_city_road_ca.paa",
                @"a3\roads_f\roads_ae\data\surf_roadconcrete_city_road_end_ca.paa",
                @"a3\roads_f\roads_ae\data\surf_roadconcrete_city_road.rvmat",
                "trail", 0f, true);

            var result = RoundTripTypesOnly(new[] { type1, type2 });

            Assert.Equal(2, result.RoadTypeInfos.Count);

            var r1 = result.RoadTypeInfos.First(t => t.Id == 1);
            Assert.Equal(12.5f, r1.TextureWidth);
            Assert.Equal(@"a3\roads_f\roads_ae\data\surf_roadtarmac_highway_ca.paa", r1.Texture);
            Assert.Equal(@"a3\roads_f\roads_ae\data\surf_roadtarmac_highway_end_ca.paa", r1.TextureEnd);
            Assert.Equal(@"a3\roads_f\roads_ae\data\surf_roadtarmac_highway.rvmat", r1.Material);
            Assert.Equal("main road", r1.Map);
            Assert.Equal(1f, r1.PathOffset);
            Assert.False(r1.IsPedestriansOnly);

            var r2 = result.RoadTypeInfos.First(t => t.Id == 8);
            Assert.Equal(2.5f, r2.TextureWidth);
            Assert.Equal("trail", r2.Map);
            Assert.Equal(0f, r2.PathOffset);
            Assert.True(r2.IsPedestriansOnly);
        }

        [Fact]
        public void Deserialize_Roads_ArePreserved()
        {
            var type1 = MakeType(1, 12.5f, "tex.paa", "tex_end.paa", "mat.rvmat", "main road", 1f, false);
            var type2 = MakeType(3, 9f, "tex2.paa", "tex2_end.paa", "mat2.rvmat", "road", 1.5f, false);

            var road1 = MakeRoad(0, type1, new TerrainPoint(100, 200), new TerrainPoint(300, 400));
            var road2 = MakeRoad(1, type2, new TerrainPoint(500, 600), new TerrainPoint(700, 800), new TerrainPoint(900, 1000));

            var result = RoundTrip(new[] { road1, road2 }, new[] { type1, type2 });

            Assert.Equal(2, result.Roads.Count);

            var deserialized1 = result.Roads.First(r => r.RoadTypeInfos.Id == 1);
            Assert.Equal(0, deserialized1.Order);
            Assert.Equal(2, deserialized1.Path.Points.Count);
            Assert.Equal(100f, deserialized1.Path.Points[0].X, 1f);
            Assert.Equal(200f, deserialized1.Path.Points[0].Y, 1f);
            Assert.Equal(300f, deserialized1.Path.Points[1].X, 1f);
            Assert.Equal(400f, deserialized1.Path.Points[1].Y, 1f);

            var deserialized2 = result.Roads.First(r => r.RoadTypeInfos.Id == 3);
            Assert.Equal(1, deserialized2.Order);
            Assert.Equal(3, deserialized2.Path.Points.Count);
        }

        internal const string FullSampleRoadsLibCfg = @"class RoadTypesLibrary
{
class Road0001
{
    width = 12.5;
    mainStrTex      = ""a3\roads_f\roads_ae\data\surf_roadtarmac_highway_ca.paa""; 
    mainTerTex      = ""a3\roads_f\roads_ae\data\surf_roadtarmac_highway_end_ca.paa"";
    mainMat         = ""a3\roads_f\roads_ae\data\surf_roadtarmac_highway.rvmat"";
    map             = ""main road"";
    AIpathOffset 	= 1;
    pedestriansOnly = false;
};
class Road0002
{
    width = 12.5;
    mainStrTex      = ""a3\roads_f\roads_ae\data\surf_roadtarmac_highway_ca.paa""; 
    mainTerTex      = ""a3\roads_f\roads_ae\data\surf_roadtarmac_highway_end_ca.paa"";
    mainMat         = ""a3\roads_f\roads_ae\data\surf_roadtarmac_highway.rvmat"";
    map             = ""main road"";
    AIpathOffset 	= 1;
    pedestriansOnly = false;
};
class Road0003
{
    width = 9;
    mainStrTex      = ""a3\structures_f_exp\data\roads\surf_exp_roadtarmac_main_road_ca.paa""; 
    mainTerTex      = ""a3\structures_f_exp\data\roads\surf_exp_roadtarmac_main_road_end_ca.paa"";
    mainMat         = ""a3\structures_f_exp\data\roads\surf_exp_roadtarmac_main_road.rvmat"";
    map             = ""road"";
    AIpathOffset 	= 1.5;
    pedestriansOnly = false;
};
class Road0004
{
    width = 8.5;
    mainStrTex      = ""a3\roads_f\roads_ae\data\surf_roadconcrete_city_road_ca.paa""; 
    mainTerTex      = ""a3\roads_f\roads_ae\data\surf_roadconcrete_city_road_end_ca.paa"";
    mainMat         = ""a3\roads_f\roads_ae\data\surf_roadconcrete_city_road.rvmat"";
    map             = ""track"";
    AIpathOffset 	= 1.5;
    pedestriansOnly = false;
};
class Road0005
{
    width = 5;
    mainStrTex      = ""a3\roads_f\roads_ae\data\surf_roadconcrete_city_road_ca.paa""; 
    mainTerTex      = ""a3\roads_f\roads_ae\data\surf_roadconcrete_city_road_end_ca.paa"";
    mainMat         = ""a3\roads_f\roads_ae\data\surf_roadconcrete_city_road.rvmat"";
    map             = ""track"";
    AIpathOffset 	= 0;
    pedestriansOnly = false;
};
class Road0006
{
    width = 5;
    mainStrTex      = ""a3\roads_f\roads_ae\data\surf_roaddirt_road_ca.paa""; 
    mainTerTex      = ""a3\roads_f\roads_ae\data\surf_roaddirt_road_end_ca.paa"";
    mainMat         = ""a3\roads_f\roads_ae\data\surf_roaddirt_road.rvmat"";
    map             = ""track"";
    AIpathOffset 	= 2;
    pedestriansOnly = false;
};
class Road0007
{
    width = 4.5;
    mainStrTex      = ""a3\roads_f\roads_ae\data\surf_roaddirt_path_ca.paa""; 
    mainTerTex      = ""a3\roads_f\roads_ae\data\surf_roaddirt_path_end_ca.paa"";
    mainMat         = ""a3\roads_f\roads_ae\data\surf_roaddirt_path.rvmat"";
    map             = ""track"";
    AIpathOffset 	= 2.5;
    pedestriansOnly = false;
};
class Road0008
{
    width = 2.5;
    mainStrTex      = ""a3\roads_f\roads_ae\data\surf_roadconcrete_city_road_ca.paa""; 
    mainTerTex      = ""a3\roads_f\roads_ae\data\surf_roadconcrete_city_road_end_ca.paa"";
    mainMat         = ""a3\roads_f\roads_ae\data\surf_roadconcrete_city_road.rvmat"";
    map             = ""trail"";
    AIpathOffset 	= 0;
    pedestriansOnly = true;
};
class Road0009
{
    width = 2;
    mainStrTex      = ""a3\structures_f_exp\data\roads\surf_exp_traildirt_trail_ca.paa""; 
    mainTerTex      = ""a3\structures_f_exp\data\roads\surf_exp_traildirt_trail_end_ca.paa"";
    mainMat         = ""a3\structures_f_exp\data\roads\surf_exp_traildirt_trail.rvmat"";
    map             = ""trail"";
    AIpathOffset 	= 0;
    pedestriansOnly = true;
};
};
";

        [Fact]
        public void Deserialize_FullRoadsLib_AllNineTypesParsedCorrectly()
        {
            // Inject the literal sample roadslib.cfg; use a dummy shapefile from a minimal round-trip
            var dummyType = MakeType(1, 12.5f, "tex.paa", "tex_end.paa", "mat.rvmat", "main road", 1f, false);
            var dummyRoad = MakeRoad(0, dummyType, new TerrainPoint(0, 0), new TerrainPoint(1, 1));
            var fs = new GameFileSystemMock();
            fs.CreateDirectory(BasePath);
            new RoadsSerializer(fs).Serialize(BasePath, new[] { dummyRoad }, new[] { dummyType });
            fs.WriteTextFile($@"{BasePath}\roadslib.cfg", FullSampleRoadsLibCfg);

            var result = new RoadsDeserializer(fs).Deserialize(BasePath);

            Assert.Equal(9, result.RoadTypeInfos.Count);

            var t1 = result.RoadTypeInfos.First(t => t.Id == 1);
            Assert.Equal(12.5f, t1.TextureWidth);
            Assert.Equal(@"a3\roads_f\roads_ae\data\surf_roadtarmac_highway_ca.paa", t1.Texture);
            Assert.Equal(@"a3\roads_f\roads_ae\data\surf_roadtarmac_highway_end_ca.paa", t1.TextureEnd);
            Assert.Equal(@"a3\roads_f\roads_ae\data\surf_roadtarmac_highway.rvmat", t1.Material);
            Assert.Equal("main road", t1.Map);
            Assert.Equal(1f, t1.PathOffset);
            Assert.False(t1.IsPedestriansOnly);

            var t3 = result.RoadTypeInfos.First(t => t.Id == 3);
            Assert.Equal(9f, t3.TextureWidth);
            Assert.Equal(@"a3\structures_f_exp\data\roads\surf_exp_roadtarmac_main_road_ca.paa", t3.Texture);
            Assert.Equal("road", t3.Map);
            Assert.Equal(1.5f, t3.PathOffset);
            Assert.False(t3.IsPedestriansOnly);

            var t4 = result.RoadTypeInfos.First(t => t.Id == 4);
            Assert.Equal(8.5f, t4.TextureWidth);
            Assert.Equal("track", t4.Map);
            Assert.Equal(1.5f, t4.PathOffset);
            Assert.False(t4.IsPedestriansOnly);

            var t7 = result.RoadTypeInfos.First(t => t.Id == 7);
            Assert.Equal(4.5f, t7.TextureWidth);
            Assert.Equal("track", t7.Map);
            Assert.Equal(2.5f, t7.PathOffset);
            Assert.False(t7.IsPedestriansOnly);

            var t8 = result.RoadTypeInfos.First(t => t.Id == 8);
            Assert.Equal(2.5f, t8.TextureWidth);
            Assert.Equal("trail", t8.Map);
            Assert.Equal(0f, t8.PathOffset);
            Assert.True(t8.IsPedestriansOnly);

            var t9 = result.RoadTypeInfos.First(t => t.Id == 9);
            Assert.Equal(2f, t9.TextureWidth);
            Assert.Equal(@"a3\structures_f_exp\data\roads\surf_exp_traildirt_trail_ca.paa", t9.Texture);
            Assert.Equal("trail", t9.Map);
            Assert.Equal(0f, t9.PathOffset);
            Assert.True(t9.IsPedestriansOnly);
        }

        [Fact]
        public void Deserialize_TypeInfos_ParsedCorrectly_SampleRoadsLib()
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
                MakeType(7, 4.5f,
                    @"a3\roads_f\roads_ae\data\surf_roaddirt_path_ca.paa",
                    @"a3\roads_f\roads_ae\data\surf_roaddirt_path_end_ca.paa",
                    @"a3\roads_f\roads_ae\data\surf_roaddirt_path.rvmat",
                    "track", 2.5f, false),
                MakeType(9, 2f,
                    @"a3\structures_f_exp\data\roads\surf_exp_traildirt_trail_ca.paa",
                    @"a3\structures_f_exp\data\roads\surf_exp_traildirt_trail_end_ca.paa",
                    @"a3\structures_f_exp\data\roads\surf_exp_traildirt_trail.rvmat",
                    "trail", 0f, true),
            };

            var result = RoundTripTypesOnly(types);

            Assert.Equal(5, result.RoadTypeInfos.Count);

            var t3 = result.RoadTypeInfos.First(t => t.Id == 3);
            Assert.Equal(9f, t3.TextureWidth);
            Assert.Equal(1.5f, t3.PathOffset);
            Assert.Equal("road", t3.Map);
            Assert.False(t3.IsPedestriansOnly);

            var t9 = result.RoadTypeInfos.First(t => t.Id == 9);
            Assert.Equal(2f, t9.TextureWidth);
            Assert.Equal(0f, t9.PathOffset);
            Assert.Equal("trail", t9.Map);
            Assert.True(t9.IsPedestriansOnly);
        }
    }
}
