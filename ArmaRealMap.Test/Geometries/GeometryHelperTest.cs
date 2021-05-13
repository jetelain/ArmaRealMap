using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArmaRealMap.Geometries;
using NetTopologySuite.Geometries;
using Xunit;

namespace ArmaRealMap.Test.Geometries
{
    public class GeometryHelperTest
    {
        [Fact]
        public void GeometryHelper_CropRingToMap()
        {
            var map = new MapInfos()
            {
                P1 = new TerrainPoint(100, 100),
                P2 = new TerrainPoint(300, 300)
            };

            var cropped = GeometryHelper.CropRingToMap(map, new List<Coordinate>() { 
                new Coordinate(50,275),
                new Coordinate(150,225),
                new Coordinate(50,175),
                new Coordinate(50,225),
                new Coordinate(50,275)
            });
            Assert.Equal(new List<Coordinate>() {
                new Coordinate(150,225),
                new Coordinate(100,200),
                new Coordinate(100,250),
                new Coordinate(150,225)
            }, cropped);

            cropped = GeometryHelper.CropRingToMap(map, new List<Coordinate>() {
                new Coordinate(150,225),
                new Coordinate(50,275),
                new Coordinate(50,225),
                new Coordinate(50,175),
                new Coordinate(150,225),
            });
            Assert.Equal(new List<Coordinate>() {
                new Coordinate(150,225),
                new Coordinate(100,250),
                new Coordinate(100,200),
                new Coordinate(150,225)
            }, cropped);

            cropped = GeometryHelper.CropRingToMap(map, new List<Coordinate>() {
                new Coordinate(225,175),
                new Coordinate(375,125),
                new Coordinate(275,25),
                new Coordinate(225,175),
            });
            Assert.Equal(new List<Coordinate>() {
                new Coordinate(225,175),
                new Coordinate(300,150),
                new Coordinate(300,100),
                new Coordinate(250,100),
                new Coordinate(225,175)
            }, cropped);


            cropped = GeometryHelper.CropRingToMap(map, new List<Coordinate>() {
                new Coordinate(225,175),
                new Coordinate(150,175),
                new Coordinate(150,225),
                new Coordinate(225,175)
            });
            Assert.Equal(new List<Coordinate>() {
                new Coordinate(225,175),
                new Coordinate(150,175),
                new Coordinate(150,225),
                new Coordinate(225,175)
            }, cropped);



            cropped = GeometryHelper.CropRingToMap(map, new List<Coordinate>() {
                new Coordinate(75,50),
                new Coordinate(150,125),
                new Coordinate(200,75),
                new Coordinate(250,125),
                new Coordinate(325,50),
                new Coordinate(75,50)
            });
            Assert.Equal(new List<Coordinate>() {
                new Coordinate(150,125),
                new Coordinate(175,100),
                new Coordinate(225,100),
                new Coordinate(250,125),
                new Coordinate(275,100),
                new Coordinate(125,100),
                new Coordinate(150,125)
            }, cropped);
        }

        [Fact]
        public void GeometryHelper_PointAtBoundary()
        {
            var map = new MapInfos()
            {
                P1 = new TerrainPoint(100, 100),
                P2 = new TerrainPoint(300, 300)
            };

            var point = GeometryHelper.PointAtBoundary(map, new Coordinate(150,175), new Coordinate(50,75));
            Assert.Equal(100, point.X);
            Assert.Equal(125, point.Y);

            point = GeometryHelper.PointAtBoundary(map, new Coordinate(150, 175), new Coordinate(75, 100));
            Assert.Equal(100, point.X);
            Assert.Equal(125, point.Y);

            point = GeometryHelper.PointAtBoundary(map, new Coordinate(150, 175), new Coordinate(200, 25));
            Assert.Equal(175, point.X);
            Assert.Equal(100, point.Y);

            point = GeometryHelper.PointAtBoundary(map, new Coordinate(150, 175), new Coordinate(50, 175));
            Assert.Equal(100, point.X);
            Assert.Equal(175, point.Y);

            point = GeometryHelper.PointAtBoundary(map, new Coordinate(150, 175), new Coordinate(150, 50));
            Assert.Equal(150, point.X);
            Assert.Equal(100, point.Y);

            point = GeometryHelper.PointAtBoundary(map, new Coordinate(250, 175), new Coordinate(300, 25));
            Assert.Equal(275, point.X);
            Assert.Equal(100, point.Y);

            point = GeometryHelper.PointAtBoundary(map, new Coordinate(250, 175), new Coordinate(250, 25));
            Assert.Equal(250, point.X);
            Assert.Equal(100, point.Y);

            point = GeometryHelper.PointAtBoundary(map, new Coordinate(250, 175), new Coordinate(350, 175));
            Assert.Equal(300, point.X);
            Assert.Equal(175, point.Y);

            point = GeometryHelper.PointAtBoundary(map, new Coordinate(250, 175), new Coordinate(350, 225));
            Assert.Equal(300, point.X);
            Assert.Equal(200, point.Y);

            point = GeometryHelper.PointAtBoundary(map, new Coordinate(250, 225), new Coordinate(150, 325));
            Assert.Equal(175, point.X);
            Assert.Equal(300, point.Y);

            point = GeometryHelper.PointAtBoundary(map, new Coordinate(250, 225), new Coordinate(300, 375));
            Assert.Equal(275, point.X);
            Assert.Equal(300, point.Y);


            point = GeometryHelper.PointAtBoundary(map, new Coordinate(250, 225), new Coordinate(350, 275));
            Assert.Equal(300, point.X);
            Assert.Equal(250, point.Y);

            point = GeometryHelper.PointAtBoundary(map, new Coordinate(250, 225), new Coordinate(400, 300));
            Assert.Equal(300, point.X);
            Assert.Equal(250, point.Y);
        }
    }
}
