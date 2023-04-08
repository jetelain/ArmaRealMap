using GameRealisticMap.Geometries;

namespace GameRealisticMap.Test.Geometries
{
    public class TerrainPolygonTest
    {
        private static TerrainPolygon Square100x100()
        {
            return new TerrainPolygon(new List<TerrainPoint>() {
                new TerrainPoint(100,100),
                new TerrainPoint(0,100),
                new TerrainPoint(0,0),
                new TerrainPoint(100,0),
                new TerrainPoint(100,100)
            }, new List<List<TerrainPoint>>());
        }
        private static TerrainPolygon Square100x100WithHole()
        {
            return new TerrainPolygon(new List<TerrainPoint>() {
                new TerrainPoint(100,100),
                new TerrainPoint(0,100),
                new TerrainPoint(0,0),
                new TerrainPoint(100,0),
                new TerrainPoint(100,100)
            }, new List<List<TerrainPoint>>() { new List<TerrainPoint>() {

                new TerrainPoint(75,75),
                new TerrainPoint(75,25),
                new TerrainPoint(25,25),
                new TerrainPoint(25,75),
                new TerrainPoint(75,75)
            } });
        }

        [Fact]
        public void TerrainPolygon_InnerCrown_NoHole()
        {
            var poly = Square100x100();

            var innerPoly = Assert.Single(poly.InnerCrown(10));
            Assert.Equal(new TerrainPoint(0, 0), innerPoly.MinPoint);
            Assert.Equal(new TerrainPoint(100, 100), innerPoly.MaxPoint);
            Assert.Equal(new List<TerrainPoint>() {
                new TerrainPoint(100,100),
                new TerrainPoint(0,100),
                new TerrainPoint(0,0),
                new TerrainPoint(100,0),
                new TerrainPoint(100,100)
            }, innerPoly.Shell);
            Assert.Equal(new List<TerrainPoint>() {
                new TerrainPoint(10,10),
                new TerrainPoint(10,90),
                new TerrainPoint(90,90),
                new TerrainPoint(90,10),
                new TerrainPoint(10,10)
            }, Assert.Single(innerPoly.Holes));
        }

        [Fact]
        public void TerrainPolygon_InnerCrown_Hole()
        {
            var poly = Square100x100WithHole();

            var innerPolyList = poly.InnerCrown(5).ToList();

            Assert.Equal(2, innerPolyList.Count);

            var innerPoly = innerPolyList[0];
            Assert.Equal(new TerrainPoint(0, 0), innerPoly.MinPoint);
            Assert.Equal(new TerrainPoint(100, 100), innerPoly.MaxPoint);
            Assert.Equal(new List<TerrainPoint>() {
                new TerrainPoint(100,100),
                new TerrainPoint(0,100),
                new TerrainPoint(0,0),
                new TerrainPoint(100,0),
                new TerrainPoint(100,100)
            }, innerPoly.Shell);
            Assert.Equal(new List<TerrainPoint>() {
                new TerrainPoint(5,5),
                new TerrainPoint(5,95),
                new TerrainPoint(95,95),
                new TerrainPoint(95,5),
                new TerrainPoint(5,5)
            }, Assert.Single(innerPoly.Holes));


            innerPoly = innerPolyList[1];
            Assert.Equal(new TerrainPoint(20, 20), innerPoly.MinPoint);
            Assert.Equal(new TerrainPoint(80, 80), innerPoly.MaxPoint);
            Assert.Equal(9, innerPoly.Shell.Count);
            Assert.Equal(new List<TerrainPoint>() {
                new TerrainPoint(25,25),
                new TerrainPoint(25,75),
                new TerrainPoint(75,75),
                new TerrainPoint(75,25),
                new TerrainPoint(25,25)
            }, Assert.Single(innerPoly.Holes));
        }


        [Fact]
        public void TerrainPolygon_Offset_NoHole()
        {
            var poly = Square100x100();

            var offsetPoly = Assert.Single(poly.Offset(10));
            Assert.Equal(new TerrainPoint(-10, -10), offsetPoly.MinPoint);
            Assert.Equal(new TerrainPoint(110, 110), offsetPoly.MaxPoint);
            Assert.Equal(9, offsetPoly.Shell.Count);
            Assert.Empty(offsetPoly.Holes);
        }

        [Fact]
        public void TerrainPolygon_Offset_Hole()
        {
            var poly = Square100x100WithHole();

            var offsetPoly = Assert.Single(poly.Offset(10));
            Assert.Equal(new TerrainPoint(-10, -10), offsetPoly.MinPoint);
            Assert.Equal(new TerrainPoint(110, 110), offsetPoly.MaxPoint);
            Assert.Equal(9, offsetPoly.Shell.Count);
            Assert.Equal(new List<TerrainPoint>() {
                new TerrainPoint(35,35),
                new TerrainPoint(35,65),
                new TerrainPoint(65,65),
                new TerrainPoint(65,35),
                new TerrainPoint(35,35)
            }, Assert.Single(offsetPoly.Holes));

            offsetPoly = Assert.Single(poly.Offset(-10));
            Assert.Equal(new TerrainPoint(10, 10), offsetPoly.MinPoint);
            Assert.Equal(new TerrainPoint(90, 90), offsetPoly.MaxPoint);
            Assert.Equal(new List<TerrainPoint>() {
                new TerrainPoint(90,90),
                new TerrainPoint(10,90),
                new TerrainPoint(10,10),
                new TerrainPoint(90,10),
                new TerrainPoint(90,90)
            }, offsetPoly.Shell);
            Assert.Equal(9, Assert.Single(offsetPoly.Holes).Count);
        }

        [Fact]
        public void TerrainPolygon_OuterCrown_NoHole()
        {
            var poly = Square100x100();

            var outerPoly = Assert.Single(poly.OuterCrown(10));
            Assert.Equal(new TerrainPoint(-10, -10), outerPoly.MinPoint);
            Assert.Equal(new TerrainPoint(110, 110), outerPoly.MaxPoint);
            Assert.Equal(9, outerPoly.Shell.Count);
            Assert.Equal(new List<TerrainPoint>() {
                new TerrainPoint(0,0),
                new TerrainPoint(0,100),
                new TerrainPoint(100,100),
                new TerrainPoint(100,0),
                new TerrainPoint(0,0)
            }, Assert.Single(outerPoly.Holes));
        }

        [Fact]
        public void TerrainPolygon_Crown_NoHole()
        {
            var poly = Square100x100();

            var crown = Assert.Single(poly.Crown(10,10));
            Assert.Equal(new TerrainPoint(-10, -10), crown.MinPoint);
            Assert.Equal(new TerrainPoint(110, 110), crown.MaxPoint);
            Assert.Equal(9, crown.Shell.Count);
            Assert.Equal(new List<TerrainPoint>() {
                new TerrainPoint(10,10),
                new TerrainPoint(10,90),
                new TerrainPoint(90,90),
                new TerrainPoint(90,10),
                new TerrainPoint(10,10)
            }, Assert.Single(crown.Holes));
        }
    }
}
