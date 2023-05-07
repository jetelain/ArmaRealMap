using System.Numerics;
using GameRealisticMap.Algorithms;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.TerrainBuilder;
using GameRealisticMap.Geometries;

namespace GameRealisticMap.Arma3.Test.Assets
{
    public class CompositionTest
    {
        private class ModelPosition : IModelPosition
        {
            public ModelPosition(TerrainPoint center, float angle, float elevation = 0, float scale = 1)
            {
                Center = center; 
                Angle = angle;
                RelativeElevation = elevation;
                Scale = scale;
            }

            public float Angle { get; }

            public TerrainPoint Center { get; }

            public float RelativeElevation { get; }

            public float Scale { get; }
        }

        [Fact]
        public void Composition_SimpleCase()
        {
            var sampleModel = new ModelInfo("modelname", "path", "bundle", Vector3.Zero);

            var composition = Composition.CreateFrom(new[] {
                new TerrainBuilderObject(sampleModel, new TerrainPoint(0, 1), 0, ElevationMode.Relative, 0)
            });

            var obj = Assert.Single(composition.ToTerrainBuilderObjects(new ModelPosition(new TerrainPoint(100, 100), 0)));
            Assert.Equal(new TerrainPoint(100, 101), obj.Point);
            Assert.Equal(0, obj.Yaw);
            Assert.Equal(0, obj.Elevation);

            obj = Assert.Single(composition.ToTerrainBuilderObjects(new ModelPosition(new TerrainPoint(100, 100), -90)));
            Assert.Equal(new TerrainPoint(101, 100), obj.Point);
            Assert.Equal(90, obj.Yaw);
            Assert.Equal(0, obj.Elevation);

            obj = Assert.Single(composition.ToTerrainBuilderObjects(new ModelPosition(new TerrainPoint(100, 100), +90)));
            Assert.Equal(new TerrainPoint(99, 100), obj.Point);
            Assert.Equal(-90, obj.Yaw);
            Assert.Equal(0, obj.Elevation);

            obj = Assert.Single(composition.ToTerrainBuilderObjects(new ModelPosition(new TerrainPoint(100, 100), 180)));
            Assert.Equal(new TerrainPoint(100, 99), obj.Point);
            Assert.Equal(180, obj.Yaw);
            Assert.Equal(0, obj.Elevation);
        }

    }
}
