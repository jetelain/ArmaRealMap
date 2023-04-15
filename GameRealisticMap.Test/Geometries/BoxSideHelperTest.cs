using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using GameRealisticMap.Geometries;

namespace GameRealisticMap.Test.Geometries
{
    public class BoxSideHelperTest
    {
        [Fact]
        public void GetClosest()
        {
            var paths = new[]
            {
                new TerrainPath(new (10,0), new(50,40)),
                new TerrainPath(new (60,0), new(0,60))
            };

            var box = new BoundingBox(new TerrainPoint(10, 20), 10, 5, -45);
            Assert.Equal(BoxSide.Right, BoxSideHelper.GetClosest(box, paths, 20));

            box = new BoundingBox(new TerrainPoint(15, 35), 10, 5, -45);
            Assert.Equal(BoxSide.Top, BoxSideHelper.GetClosest(box, paths, 20));

            box = new BoundingBox(new TerrainPoint(10, 20), 10, 5, 135);
            Assert.Equal(BoxSide.Left, BoxSideHelper.GetClosest(box, paths, 20));

            box = new BoundingBox(new TerrainPoint(15, 35), 10, 5, 135);
            Assert.Equal(BoxSide.Bottom, BoxSideHelper.GetClosest(box, paths, 20));
        }
    }
}
