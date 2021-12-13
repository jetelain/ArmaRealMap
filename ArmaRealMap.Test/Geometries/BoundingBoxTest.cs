using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArmaRealMap.Geometries;
using Xunit;

namespace ArmaRealMap.Test.Geometries
{
    public class BoundingBoxTest
    {
        [Fact]
        public void BoundingBox_ComputeInner()
        {
            var box = BoundingBox.ComputeInner(new[] {
                new TerrainPoint(0,0),
                new TerrainPoint(20,0),
                new TerrainPoint(20,8),
                new TerrainPoint(18,10),
                new TerrainPoint(0,10)
            });
            Assert.Equal(0, box.Angle);
            Assert.Equal(18, box.Width);
            Assert.Equal(10, box.Height);
            Assert.Equal(9, box.Center.X);
            Assert.Equal(5, box.Center.Y);

            //var box = BoundingBox.ComputeInner(new[] {
            //    new TerrainPoint(54.0f,54.8f),
            //    new TerrainPoint(22.9f,52.7f),
            //    new TerrainPoint(23.4f,45.7f),
            //    new TerrainPoint(54.5f,47.8f)
            //});

        }
        /*+		
         * 

*/
    }
}
