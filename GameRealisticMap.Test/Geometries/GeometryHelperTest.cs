using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameRealisticMap.Geometries;
using NetTopologySuite.Geometries;
using Xunit;

namespace GameRealisticMap.Test.Geometries
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


        [Fact]
        public void GeometryHelper_KeepAway()
        {
            var paths = new TerrainSpacialIndex<ITerrainGeo>(20);
            paths.Insert( new TerrainPath(new(0, 0), new(0, 10)) );
            Assert.Equal(new(2, 5), GeometryHelper.KeepAway(new TerrainPoint(1, 5), paths, 2f));
            Assert.Equal(new(-2, 5), GeometryHelper.KeepAway(new TerrainPoint(-1, 5), paths, 2f));
        }

        [Fact]
        public void GeometryHelper_GetFacing()
        {
            var paths = new[] { new TerrainPath(new(0, 0), new(0, 10)) };
            Assert.Equal(90, GeometryHelper.GetFacing(new TerrainPoint(1, 5), paths));
            Assert.Equal(-90, GeometryHelper.GetFacing(new TerrainPoint(-1, 5), paths));

            paths = new[] { new TerrainPath(new(0, 0), new(10, 0)) };
            Assert.Equal(0, GeometryHelper.GetFacing(new TerrainPoint(5, -1), paths));
            Assert.Equal(-180, GeometryHelper.GetFacing(new TerrainPoint(5, 1), paths));

            paths = new[] { new TerrainPath(new(0, 0), new(10, 10)) };
            Assert.Equal(45, GeometryHelper.GetFacing(new TerrainPoint(5, 0), paths));
            Assert.Equal(-135, GeometryHelper.GetFacing(new TerrainPoint(0, 5), paths));
        }
    }
}
