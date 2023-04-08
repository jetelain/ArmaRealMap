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
