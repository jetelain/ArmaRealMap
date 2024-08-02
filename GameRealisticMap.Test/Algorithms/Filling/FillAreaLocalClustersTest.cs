using System.Numerics;
using GameRealisticMap.Algorithms;
using GameRealisticMap.Algorithms.Filling;
using GameRealisticMap.Geometries;
using Pmad.ProgressTracking;
using Xunit.Abstractions;

namespace GameRealisticMap.Test.Algorithms.Filling
{
    public class FillAreaLocalClustersTest : FillAreaTestBase
    {
        public FillAreaLocalClustersTest(ITestOutputHelper output) 
            : base(output)
        {
        }

        [Fact]
        public void FillAreaLocalClusters_Large()
        {
            var fill = new FillAreaLocalClusters<string>(new NoProgress(), new[] { new ClusterCollectionDefinitionMock(0.01) {
                new ClusterDefinitionMock(0.25)
                {
                    new ClusterItemDefinitionMock("A1", 0.5, 1),
                    new ClusterItemDefinitionMock("A2", 0.5, 1)
                },
                new ClusterDefinitionMock(0.5)
                {
                    new ClusterItemDefinitionMock("B1", 0.5, 1),
                    new ClusterItemDefinitionMock("B2", 0.25, 1),
                    new ClusterItemDefinitionMock("B3", 0.25, 1)
                },
                new ClusterDefinitionMock(0.25)
                {
                    new ClusterItemDefinitionMock("C", 1, 1)
                }
            } });

            var result = new RadiusPlacedLayer<string>(new Vector2(500, 500));

            fill.FillPolygons(result, new List<TerrainPolygon> {
                TerrainPolygon.FromRectangle(new TerrainPoint(10, 10), new TerrainPoint(110, 110)),
                TerrainPolygon.FromRectangle(new TerrainPoint(200, 100), new TerrainPoint(300, 200)),
            });

            Assert.Equal(200, result.Count);

            AssertResult(result, "GameRealisticMap.Test.Algorithms.Filling.FillAreaLocalClusters_Large.csv");
        }
    }
}
