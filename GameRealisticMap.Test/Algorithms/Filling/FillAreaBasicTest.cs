using System.Numerics;
using GameRealisticMap.Algorithms;
using GameRealisticMap.Algorithms.Filling;
using GameRealisticMap.Geometries;
using Pmad.ProgressTracking;
using Xunit.Abstractions;

namespace GameRealisticMap.Test.Algorithms.Filling
{
    public class FillAreaBasicTest : FillAreaTestBase
    {
        public FillAreaBasicTest(ITestOutputHelper output)
            : base(output)
        {

        }

        [Fact]
        public void FillAreaBasic_Large()
        {
            var fill = new FillAreaBasic<string>(new NoProgress(), new[] { new BasicDefinitionMock(0.01) { 
                new ClusterItemDefinitionMock("A", 0.5, 1), 
                new ClusterItemDefinitionMock("B", 0.5, 1) 
            } });

            var result = new RadiusPlacedLayer<string>(new Vector2(500,500));

            fill.FillPolygons(result, new List<TerrainPolygon> { 
                TerrainPolygon.FromRectangle(new TerrainPoint(10, 10), new TerrainPoint(110, 110)),
                TerrainPolygon.FromRectangle(new TerrainPoint(200, 100), new TerrainPoint(300, 200)),
            });

            Assert.Equal(200, result.Count);

            AssertResult(result, "GameRealisticMap.Test.Algorithms.Filling.FillAreaBasic_Large.csv");
        }

    }
}
