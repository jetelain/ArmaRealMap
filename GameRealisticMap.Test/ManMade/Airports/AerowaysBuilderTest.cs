using System.Numerics;
using GameRealisticMap.ManMade.Airports;
using GameRealisticMap.Osm;
using GameRealisticMap.Reporting;

namespace GameRealisticMap.Test.ManMade.Airports
{
    public class AerowaysBuilderTest
    {

        [Fact]
        public void Build_AerowayMappedAsArea()
        {
            var context = new BuildContextMock(
                TerrainAreaUTM.CreateFromCenter( new CoordinateSharp.Coordinate(38.801367692363094, -0.39838115945233704), 2.5f, 256),
                OsmDataSource.CreateFromInlineXml(
    @"<osm version=""0.6"">
      <node id=""2959416595"" lat=""38.8027006"" lon=""-0.3979034""/>
      <node id=""2959416596"" lat=""38.8026003"" lon=""-0.3977210""/>
      <node id=""2959416597"" lat=""38.8014882"" lon=""-0.3986705""/>
      <node id=""2959416598"" lat=""38.8015928"" lon=""-0.3988556""/>
      <way id=""292386029"">
        <nd ref=""2959416595""/>
        <nd ref=""2959416596""/>
        <nd ref=""2959416597""/>
        <nd ref=""2959416598""/>
        <nd ref=""2959416595""/>
        <tag k=""aeroway"" v=""runway""/>
        <tag k=""length"" v=""500""/>
        <tag k=""ref"" v=""03/21""/>
        <tag k=""surface"" v=""earth""/>
        <tag k=""width"" v=""30""/>
      </way>
    </osm>"));

            context.SetData(new AirportData(new List<GameRealisticMap.Geometries.TerrainPolygon>()));

            var result = new AerowaysBuilder(new NoProgressSystem()).Build(context);
            Assert.Empty(result.InsideAirports);

            var aeroway = Assert.Single(result.OutsideAirports);
            Assert.Equal(AerowayTypeId.Runway, aeroway.Type);
            Assert.Equal(19.82779f, aeroway.Width, 1);
            Assert.Equal(148.90303f, aeroway.Segment.Length, 1);
            Assert.Equal(new Vector2(-0.5342595f, -0.8453205f), aeroway.OverallVector);
        }

    }
}
