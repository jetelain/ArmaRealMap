using System.Numerics;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.ManMade;
using GameRealisticMap.Arma3.TerrainBuilder;
using GameRealisticMap.ElevationModel;
using GameRealisticMap.Geometries;

namespace GameRealisticMap.Arma3.Test.ManMade
{
    public class BridgeGeneratorTest
    {
        private readonly SlopeElevationGrid slopeElevationGrid = new SlopeElevationGrid(5, new Vector2(0.5f, 0f)); // 50% slope

        private readonly FlatElevationGrid flatElevationGrid = new FlatElevationGrid(5);

        private readonly BridgeDefinition definition = new BridgeDefinition(
            new StraightSegmentDefinition(new Composition(new CompositionObject(new ModelInfo("Single", "Single", Vector3.Zero), Matrix4x4.Identity)), 15),
            new StraightSegmentDefinition(new Composition(new CompositionObject(new ModelInfo("Start", "Start", Vector3.Zero), Matrix4x4.Identity)), 5),
            new StraightSegmentDefinition(new Composition(new CompositionObject(new ModelInfo("Middle", "Middle", Vector3.Zero), Matrix4x4.Identity)), 10),
            new StraightSegmentDefinition(new Composition(new CompositionObject(new ModelInfo("End", "End", Vector3.Zero), Matrix4x4.Identity)), 5)
            );

        [Fact]
        public void BridgeGenerator_Flat_SinglePart()
        {
            var result = new List<TerrainBuilderObject>();
            BridgeGenerator.ProgessBridge(result, flatElevationGrid, definition, new TerrainPath(new(10, 10), new(25, 10)));
            var obj = Assert.Single(result);
            Assert.Equal("Single", obj.Model.Name);
            Assert.Equal(new TerrainPoint(17.5f,10), obj.Point);
            Assert.Equal(90, obj.Yaw);
            Assert.Equal(0, obj.Pitch);
            Assert.Equal(0, obj.Roll);
        }

        [Fact]
        public void BridgeGenerator_Flat_ThreeParts()
        {
            var result = new List<TerrainBuilderObject>();
            BridgeGenerator.ProgessBridge(result, flatElevationGrid, definition, new TerrainPath(new(10, 10), new(30, 10)));
            Assert.Equal(3, result.Count);
            var obj = result[0];
            Assert.Equal("Start", obj.Model.Name);
            Assert.Equal(new TerrainPoint(12.5f, 10), obj.Point);
            Assert.Equal(90, obj.Yaw);
            Assert.Equal(0, obj.Pitch);
            Assert.Equal(0, obj.Roll);

            obj = result[1];
            Assert.Equal("Middle", obj.Model.Name);
            Assert.Equal(new TerrainPoint(20f, 10), obj.Point);
            Assert.Equal(90, obj.Yaw);
            Assert.Equal(0, obj.Pitch);
            Assert.Equal(0, obj.Roll);

            obj = result[2];
            Assert.Equal("End", obj.Model.Name);
            Assert.Equal(new TerrainPoint(27.5f, 10), obj.Point);
            Assert.Equal(90, obj.Yaw);
            Assert.Equal(0, obj.Pitch);
            Assert.Equal(0, obj.Roll);
        }

        [Fact]
        public void BridgeGenerator_Flat_ThreeParts_Unajusted()
        {
            var result = new List<TerrainBuilderObject>();
            BridgeGenerator.ProgessBridge(result, flatElevationGrid, definition, new TerrainPath(new(10, 10), new(28, 10)));
            Assert.Equal(3, result.Count);
            var obj = result[0];
            Assert.Equal("Start", obj.Model.Name);
            Assert.Equal(new TerrainPoint(12.5f, 10), obj.Point);
            Assert.Equal(90, obj.Yaw);
            Assert.Equal(0, obj.Pitch);
            Assert.Equal(0, obj.Roll);

            obj = result[1];
            Assert.Equal("Middle", obj.Model.Name);
            Assert.Equal(new TerrainPoint(19f, 10), obj.Point);
            Assert.Equal(90, obj.Yaw);
            Assert.Equal(0, obj.Pitch);
            Assert.Equal(0, obj.Roll);

            obj = result[2];
            Assert.Equal("End", obj.Model.Name);
            Assert.Equal(new TerrainPoint(25.5f, 10), obj.Point);
            Assert.Equal(90, obj.Yaw);
            Assert.Equal(0, obj.Pitch);
            Assert.Equal(0, obj.Roll);
        }

        [Fact]
        public void BridgeGenerator_Flat_FourParts()
        {
            var result = new List<TerrainBuilderObject>();
            BridgeGenerator.ProgessBridge(result, flatElevationGrid, definition, new TerrainPath(new(10, 10), new(40, 10)));
            Assert.Equal(4, result.Count);
            var obj = result[0];
            Assert.Equal("Start", obj.Model.Name);
            Assert.Equal(new TerrainPoint(12.5f, 10), obj.Point);
            Assert.Equal(90, obj.Yaw);
            Assert.Equal(0, obj.Pitch);
            Assert.Equal(0, obj.Roll);

            obj = result[1];
            Assert.Equal("Middle", obj.Model.Name);
            Assert.Equal(new TerrainPoint(20f, 10), obj.Point);
            Assert.Equal(90, obj.Yaw);
            Assert.Equal(0, obj.Pitch);
            Assert.Equal(0, obj.Roll);

            obj = result[2];
            Assert.Equal("Middle", obj.Model.Name);
            Assert.Equal(new TerrainPoint(30f, 10), obj.Point);
            Assert.Equal(90, obj.Yaw);
            Assert.Equal(0, obj.Pitch);
            Assert.Equal(0, obj.Roll);

            obj = result[3];
            Assert.Equal("End", obj.Model.Name);
            Assert.Equal(new TerrainPoint(37.5f, 10), obj.Point);
            Assert.Equal(90, obj.Yaw);
            Assert.Equal(0, obj.Pitch);
            Assert.Equal(0, obj.Roll);

        }

        [Fact]
        public void BridgeGenerator_Flat_FourParts_Unajusted()
        {
            var result = new List<TerrainBuilderObject>();
            BridgeGenerator.ProgessBridge(result, flatElevationGrid, definition, new TerrainPath(new(10, 10), new(38, 10)));
            Assert.Equal(4, result.Count);
            var obj = result[0];
            Assert.Equal("Start", obj.Model.Name);
            Assert.Equal(new TerrainPoint(12.5f, 10), obj.Point);
            Assert.Equal(90, obj.Yaw);
            Assert.Equal(0, obj.Pitch);
            Assert.Equal(0, obj.Roll);

            obj = result[1];
            Assert.Equal("Middle", obj.Model.Name);
            Assert.Equal(new TerrainPoint(19.5f, 10), obj.Point);
            Assert.Equal(90, obj.Yaw);
            Assert.Equal(0, obj.Pitch);
            Assert.Equal(0, obj.Roll);

            obj = result[2];
            Assert.Equal("Middle", obj.Model.Name);
            Assert.Equal(new TerrainPoint(28.5f, 10), obj.Point);
            Assert.Equal(90, obj.Yaw);
            Assert.Equal(0, obj.Pitch);
            Assert.Equal(0, obj.Roll);

            obj = result[3];
            Assert.Equal("End", obj.Model.Name);
            Assert.Equal(new TerrainPoint(35.5f, 10), obj.Point);
            Assert.Equal(90, obj.Yaw);
            Assert.Equal(0, obj.Pitch);
            Assert.Equal(0, obj.Roll);

        }

        [Fact]
        public void BridgeGenerator_Flat_FiveParts()
        {
            var result = new List<TerrainBuilderObject>();
            BridgeGenerator.ProgessBridge(result, flatElevationGrid, definition, new TerrainPath(new(10, 10), new(50, 10)));
            Assert.Equal(5, result.Count);
            var obj = result[0];
            Assert.Equal("Start", obj.Model.Name);
            Assert.Equal(new TerrainPoint(12.5f, 10), obj.Point);
            Assert.Equal(90, obj.Yaw);
            Assert.Equal(0, obj.Pitch);
            Assert.Equal(0, obj.Roll);

            obj = result[1];
            Assert.Equal("Middle", obj.Model.Name);
            Assert.Equal(new TerrainPoint(20f, 10), obj.Point);
            Assert.Equal(90, obj.Yaw);
            Assert.Equal(0, obj.Pitch);
            Assert.Equal(0, obj.Roll);

            obj = result[2];
            Assert.Equal("Middle", obj.Model.Name);
            Assert.Equal(new TerrainPoint(30f, 10), obj.Point);
            Assert.Equal(90, obj.Yaw);
            Assert.Equal(0, obj.Pitch);
            Assert.Equal(0, obj.Roll);

            obj = result[3];
            Assert.Equal("Middle", obj.Model.Name);
            Assert.Equal(new TerrainPoint(40f, 10), obj.Point);
            Assert.Equal(90, obj.Yaw);
            Assert.Equal(0, obj.Pitch);
            Assert.Equal(0, obj.Roll);

            obj = result[4];
            Assert.Equal("End", obj.Model.Name);
            Assert.Equal(new TerrainPoint(47.5f, 10), obj.Point);
            Assert.Equal(90, obj.Yaw);
            Assert.Equal(0, obj.Pitch);
            Assert.Equal(0, obj.Roll);
        }


        [Fact]
        public void BridgeGenerator_Slope_SinglePart()
        {
            var result = new List<TerrainBuilderObject>();
            BridgeGenerator.ProgessBridge(result, slopeElevationGrid, definition, new TerrainPath(new(10, 10), new(20, 10)));
            var obj = Assert.Single(result);
            Assert.Equal("Single", obj.Model.Name);
            Assert.Equal(new TerrainPoint(15f, 10), obj.Point);
            Assert.Equal(90, obj.Yaw);
            Assert.Equal(0, MathF.Round(obj.Pitch));
            Assert.Equal(-27, MathF.Round(obj.Roll)); // 50% slope => 26.57°
        }


        [Fact]
        public void BridgeGenerator_Slope_ThreeParts()
        {
            var result = new List<TerrainBuilderObject>();
            BridgeGenerator.ProgessBridge(result, slopeElevationGrid, definition, new TerrainPath(new(10, 10), new(25, 10)));

            Assert.Equal(3, result.Count);
            var obj = result[0];
            Assert.Equal("Start", obj.Model.Name);
            Assert.Equal(new TerrainPoint(12.236f, 10), obj.Point.ToIntPointPrecision());
            Assert.Equal(90, obj.Yaw);
            Assert.Equal(0, MathF.Round(obj.Pitch));
            Assert.Equal(-27, MathF.Round(obj.Roll)); // 50% slope => 26.57°

            obj = result[1];
            Assert.Equal("Middle", obj.Model.Name);
            Assert.Equal(new TerrainPoint(17.5f, 10), obj.Point);
            Assert.Equal(90, obj.Yaw);
            Assert.Equal(0, MathF.Round(obj.Pitch));
            Assert.Equal(-27, MathF.Round(obj.Roll)); // 50% slope => 26.57°

            obj = result[2];
            Assert.Equal("End", obj.Model.Name);
            Assert.Equal(new TerrainPoint(22.763f, 10), obj.Point.ToIntPointPrecision());
            Assert.Equal(90, obj.Yaw);
            Assert.Equal(0, MathF.Round(obj.Pitch));
            Assert.Equal(-27, MathF.Round(obj.Roll)); // 50% slope => 26.57°
        }
    }
}
