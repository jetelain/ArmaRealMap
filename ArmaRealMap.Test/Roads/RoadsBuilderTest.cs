using System.Collections.Generic;
using ArmaRealMap.Geometries;
using ArmaRealMap.Roads;
using Xunit;

namespace ArmaRealMap.Test.Roads
{
    public class RoadsBuilderTest
    {
        [Fact]
        public void RoadsBuilder_PreventSplines()
        {
            Assert.Equal(new List<TerrainPoint>()
            {
                new TerrainPoint(0,0),
                new TerrainPoint(8,0),
                new TerrainPoint(10,0),
                new TerrainPoint(10,2),
                new TerrainPoint(10,10)
            },
            RoadsBuilder.PreventSplines(new List<TerrainPoint>()
            {
                new TerrainPoint(0,0),
                new TerrainPoint(10,0),
                new TerrainPoint(10,10)
            }, 2));

            Assert.Equal(new List<TerrainPoint>()
            {
                new TerrainPoint(0,0),
                new TerrainPoint(8,0),
                new TerrainPoint(10,0),
                new TerrainPoint(10,2),
                new TerrainPoint(10,10)
            },
            RoadsBuilder.PreventSplines(new List<TerrainPoint>()
            {
                new TerrainPoint(0,0),
                new TerrainPoint(8,0),
                new TerrainPoint(10,0),
                new TerrainPoint(10,2),
                new TerrainPoint(10,10)
            }, 2));

            Assert.Equal(new List<TerrainPoint>()
            {
                new TerrainPoint(0,0),
                new TerrainPoint(8,0),
                new TerrainPoint(10,0),
                new TerrainPoint(10,2),
                new TerrainPoint(10,8),
                new TerrainPoint(10,10),
                new TerrainPoint(12,10),
                new TerrainPoint(18,10),
                new TerrainPoint(20,10),
                new TerrainPoint(20,12),
                new TerrainPoint(20,20)
            },
            RoadsBuilder.PreventSplines(new List<TerrainPoint>()
            {
                new TerrainPoint(0,0),
                new TerrainPoint(10,0),
                new TerrainPoint(10,10),
                new TerrainPoint(20,10),
                new TerrainPoint(20,20)
            }, 2));
        }
    }
}
