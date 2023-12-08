using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameRealisticMap.Geometries;

namespace GameRealisticMap.Test.Geometries
{
    public class TerrainPathHelperTest
    {

        [Fact]
        public void AutoMergeNotOriented()
        {
            var a = new TerrainPoint(0, 1);
            var b = new TerrainPoint(0, 2);
            var c = new TerrainPoint(0, 3);
            var d = new TerrainPoint(0, 4);

            Assert.Equal(new[] { a, b, c }, TerrainPathHelper.AutoMergeNotOriented(new[] { a, b }, new[] { b, c }));
            Assert.Equal(new[] { a, b, c }, TerrainPathHelper.AutoMergeNotOriented(new[] { a, b }, new[] { c, b }));
            Assert.Equal(new[] { a, b, c }, TerrainPathHelper.AutoMergeNotOriented(new[] { b, a }, new[] { b, c }));
            Assert.Equal(new[] { a, b, c }, TerrainPathHelper.AutoMergeNotOriented(new[] { b, a }, new[] { c, b }));

            Assert.Equal(new[] { a, b, c, d }, TerrainPathHelper.AutoMergeNotOriented(new[] { a, b }, new[] { c, d }));
            Assert.Equal(new[] { a, b, c, d }, TerrainPathHelper.AutoMergeNotOriented(new[] { a, b }, new[] { d, c }));
            Assert.Equal(new[] { a, b, c, d }, TerrainPathHelper.AutoMergeNotOriented(new[] { b, a }, new[] { c, d }));
            Assert.Equal(new[] { a, b, c, d }, TerrainPathHelper.AutoMergeNotOriented(new[] { b, a }, new[] { d, c }));
        }
    }
}
