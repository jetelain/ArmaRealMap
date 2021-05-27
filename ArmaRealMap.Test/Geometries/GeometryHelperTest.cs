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
        public void GeometryHelper_PointsOnPathRegular()
        {
            Assert.Equal(new[] {
                new TerrainPoint(0,0),
                new TerrainPoint(0,3),
                new TerrainPoint(0,6),
                new TerrainPoint(0,9),
                new TerrainPoint(0,12)
            },
            GeometryHelper.PointsOnPathRegular(new[] {
                new TerrainPoint(0,0),
                new TerrainPoint(0,4),
                new TerrainPoint(0,8),
                new TerrainPoint(0,12)
            }, 3f));

            Assert.Equal(new[] {
                new TerrainPoint(0,0),
                new TerrainPoint(0,2),
                new TerrainPoint(0,4),
                new TerrainPoint(0,6),
                new TerrainPoint(0,8),
                new TerrainPoint(0,10),
                new TerrainPoint(0,12)
            },
            GeometryHelper.PointsOnPathRegular(new[] {
                            new TerrainPoint(0,0),
                            new TerrainPoint(0,4),
                            new TerrainPoint(0,8),
                            new TerrainPoint(0,12)
            }, 2f));

            Assert.Equal(new[] {
                new TerrainPoint(0,0),
                new TerrainPoint(0,5),
                new TerrainPoint(0,10),
                new TerrainPoint(0,12)
            },
            GeometryHelper.PointsOnPathRegular(new[] {
                                        new TerrainPoint(0,0),
                                        new TerrainPoint(0,4),
                                        new TerrainPoint(0,8),
                                        new TerrainPoint(0,12)
            }, 5f));
        }




    }
}
