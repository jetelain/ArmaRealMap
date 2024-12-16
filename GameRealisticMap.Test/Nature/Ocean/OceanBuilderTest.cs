using GameRealisticMap.Geometries;
using GameRealisticMap.Nature.Ocean;

namespace GameRealisticMap.Test.Nature.Ocean
{
    public class OceanBuilderTest
    {
        [Fact]
        public void IsIsland_ShouldReturnTrue_WhenSingleOceanPolygonCoversEntireArea()
        {
            // Arrange
            var areaMock = TerrainAreaUTM.CreateFromCenter("48.858844, 2.294351", 1, 100);  
            var oceanPolygons = new List<TerrainPolygon>
                {
                    new TerrainPolygon([
                        new TerrainPoint(0, 0),
                        new TerrainPoint(100, 0),
                        new TerrainPoint(100, 100),
                        new TerrainPoint(0, 100),
                        new TerrainPoint(0, 0)
                    ],
                    [[new TerrainPoint(25, 25),
                        new TerrainPoint(75, 25),
                        new TerrainPoint(75, 75),
                        new TerrainPoint(25, 75),
                        new TerrainPoint(25, 25)]])
                };

            // Act
            var result = OceanBuilder.IsIsland(oceanPolygons, areaMock);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsIsland_ShouldReturnTrue_WhenSingleOceanPolygonCoversAlmostEntireArea()
        {
            // Arrange
            var areaMock = TerrainAreaUTM.CreateFromCenter("48.858844, 2.294351", 1, 100);
            var oceanPolygons = new List<TerrainPolygon>
                {
                    new TerrainPolygon([
                        new TerrainPoint(0, 0),
                        new TerrainPoint(100, 0),
                        new TerrainPoint(100, 99),
                        new TerrainPoint(99, 99),
                        new TerrainPoint(99, 100),
                        new TerrainPoint(0, 100),
                        new TerrainPoint(0, 0)
                    ],
                    [[new TerrainPoint(25, 25),
                        new TerrainPoint(75, 25),
                        new TerrainPoint(75, 75),
                        new TerrainPoint(25, 75),
                        new TerrainPoint(25, 25)]])
                };

            // Act
            var result = OceanBuilder.IsIsland(oceanPolygons, areaMock);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsIsland_ShouldReturnFalse_WhenMultipleOceanPolygonsExist()
        {
            // Arrange
            var areaMock = TerrainAreaUTM.CreateFromCenter("48.858844, 2.294351", 1, 100);
            var oceanPolygons = new List<TerrainPolygon>
                {
                    new TerrainPolygon([
                        new TerrainPoint(0, 0),
                        new TerrainPoint(50, 0),
                        new TerrainPoint(50, 50),
                        new TerrainPoint(0, 50),
                        new TerrainPoint(0, 0)
                    ]),
                    new TerrainPolygon([
                        new TerrainPoint(50, 50),
                        new TerrainPoint(100, 50),
                        new TerrainPoint(100, 100),
                        new TerrainPoint(50, 100),
                        new TerrainPoint(50, 50)
                    ])
                };

            // Act
            var result = OceanBuilder.IsIsland(oceanPolygons, areaMock);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsIsland_ShouldReturnFalse_WhenSingleOceanPolygonDoesNotCoverEntireArea()
        {
            // Arrange
            var areaMock = TerrainAreaUTM.CreateFromCenter("48.858844, 2.294351", 1, 100);
            var oceanPolygons = new List<TerrainPolygon>
                {
                    new TerrainPolygon([
                        new TerrainPoint(0, 0),
                        new TerrainPoint(50, 0),
                        new TerrainPoint(50, 50),
                        new TerrainPoint(0, 50),
                        new TerrainPoint(0, 0)
                    ])
                };

            // Act
            var result = OceanBuilder.IsIsland(oceanPolygons, areaMock);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsIsland_ShouldReturnFalse_WhenNoOceanPolygon()
        {
            // Arrange
            var areaMock = TerrainAreaUTM.CreateFromCenter("48.858844, 2.294351", 1, 100);
            var oceanPolygons = new List<TerrainPolygon>();

            // Act
            var result = OceanBuilder.IsIsland(oceanPolygons, areaMock);

            // Assert
            Assert.False(result);
        }
    }
}
