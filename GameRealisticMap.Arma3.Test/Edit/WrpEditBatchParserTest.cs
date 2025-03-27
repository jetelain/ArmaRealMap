using System.Numerics;
using GameRealisticMap.Arma3.Edit;
using GameRealisticMap.Arma3.Test.GameEngine;
using GameRealisticMap.Reporting;

namespace GameRealisticMap.Arma3.Test.Edit
{
    public class WrpEditBatchParserTest
    {
        private readonly WrpEditBatchParser _parser;
        public WrpEditBatchParserTest()
        {
            _parser = new WrpEditBatchParser(new NoProgressSystem(), new GameFileSystemMock());
        }

        [Fact]
        public void ParseFromText_ShouldReturnWrpEditBatch()
        {
            var text = @"
                ["".map"", ""WorldName"", 1024, 1]
                ["".hide"", ""\path\to\model"", [], [ 0, 0, 0 ],[ 0, 1, 0 ],[ 0, 0, 1 ],[ 0, 1, 0 ], 1, 1]
                ["".class"", ""modelName"", ""\path\to\model""]
                ["".add"", ""modelName"", [], [ 0, 0, 0 ],[ 0, 1, 0 ],[ 0, 0, 1 ],[ 0, 1, 0 ], 1]
                ["".dhmap"", [[0, 0, 1.0], [1, 1, 2.0]]]
                ["".part"", 1, 10]
            ";
            var result = _parser.ParseFromText(text);

            Assert.Equal("WorldName", result.WorldName);
            Assert.Equal(1024, result.WorldSize);
            Assert.Equal(1, result.Revision);
            Assert.Single(result.Remove);
            Assert.Equal("path\\to\\model.p3d", result.Remove[0].Model);
            Assert.Equal(1, result.Remove[0].ObjectId);
            Assert.Single(result.Add);
            Assert.Equal("path\\to\\model.p3d", result.Add[0].Model);
            Assert.True(result.ElevationAdjustObjects);
            Assert.Equal(2, result.Elevation.Count);
            Assert.Single(result.PartIndexes);
            Assert.Equal(1, result.PartIndexes[0]);
            Assert.Equal(10, result.PartCount);
        }

        [Fact]
        public void ParseFromFile_ShouldReturnWrpEditBatch()
        {
            var filePath = "testFile.txt";
            File.WriteAllText(filePath, @"
["".map"", ""WorldName"", 1024, 1]
["".hide"", ""\path\to\model"", [], [ 0, 0, 0 ],[ 0, 1, 0 ],[ 0, 0, 1 ],[ 0, 1, 0 ], 1, 1]
["".class"", ""modelName"", ""\path\to\model""]
["".add"", ""modelName"", [], [ 0, 0, 0 ],[ 0, 1, 0 ],[ 0, 0, 1 ],[ 0, 1, 0 ], 1]
["".dhmap"", [[0, 0, 1.0], [1, 1, 2.0]]]
["".part"", 1, 10]
            ");
            var result = _parser.ParseFromFile(filePath);

            Assert.Equal("WorldName", result.WorldName);
            Assert.Equal(1024, result.WorldSize);
            Assert.Equal(1, result.Revision);
            Assert.Single(result.Remove);
            Assert.Equal("path\\to\\model.p3d", result.Remove[0].Model);
            Assert.Equal(1, result.Remove[0].ObjectId);
            Assert.Single(result.Add);
            Assert.Equal("path\\to\\model.p3d", result.Add[0].Model);
            Assert.True(result.ElevationAdjustObjects);
            Assert.Equal(2, result.Elevation.Count);
            Assert.Single(result.PartIndexes);
            Assert.Equal(1, result.PartIndexes[0]);
            Assert.Equal(10, result.PartCount);

            File.Delete(filePath);
        }

        [Fact]
        public void Parse_MapEntry_ShouldSetWorldNameSizeAndRevision()
        {
            var entries = new List<string> { @"["".map"", ""WorldName"", 1024, 1]" };
            var result = _parser.Parse(entries);

            Assert.Equal("WorldName", result.WorldName);
            Assert.Equal(1024, result.WorldSize);
            Assert.Equal(1, result.Revision);
        }

        [Fact]
        public void Parse_HideEntry_ShouldAddToRemoveList()
        {
            var entries = new List<string> { @"["".hide"", ""\path\to\model"", [], [ 0, 0, 0 ],[ 0, 1, 0 ],[ 0, 0, 1 ],[ 0, 1, 0 ], 1, 1]" };
            var result = _parser.Parse(entries);

            Assert.Single(result.Remove);
            Assert.Equal("path\\to\\model.p3d", result.Remove[0].Model);
            Assert.Equal(1, result.Remove[0].ObjectId);
        }

        [Fact]
        public void Parse_ClassEntry_ShouldAddToModelsDictionary()
        {
            var entries = new List<string> { @"["".class"", ""modelName"", ""\path\to\model""]" };
            var result = _parser.Parse(entries);

            // No direct way to test private dictionary, so we test indirectly via .add
            entries.Add(@"["".add"", ""modelName"", [], [ 0, 0, 0 ],[ 0, 1, 0 ],[ 0, 0, 1 ],[ 0, 1, 0 ], 1]");
            result = _parser.Parse(entries);

            Assert.Single(result.Add);
            Assert.Equal("path\\to\\model.p3d", result.Add[0].Model);
        }

        [Fact]
        public void Parse_DhmapEntry_ShouldSetElevationAdjustObjectsToTrue()
        {
            var entries = new List<string> { @"["".dhmap"", [[0, 0, 1.0], [1, 1, 2.0]]]" };
            var result = _parser.Parse(entries);

            Assert.True(result.ElevationAdjustObjects);
        }

        [Fact]
        public void Parse_HmapEntry_ShouldSetElevationAdjustObjectsToFalse()
        {
            var entries = new List<string> { @"["".hmap"", [[0, 0, 1.0], [1, 1, 2.0]]]" };
            var result = _parser.Parse(entries);

            Assert.False(result.ElevationAdjustObjects);
        }

        [Fact]
        public void Parse_PartEntry_ShouldAddToPartIndexesAndSetPartCount()
        {
            var entries = new List<string> { @"["".part"", 1, 10]" };
            var result = _parser.Parse(entries);

            Assert.Single(result.PartIndexes);
            Assert.Equal(1, result.PartIndexes[0]);
            Assert.Equal(10, result.PartCount);
        }

        [Fact]
        public void Parse_ElevationData_ShouldAddElevationGrids()
        {
            var entries = new List<string> { @"["".dhmap"", [[0, 0, 1.0], [1, 1, 2.0]]]" };
            var result = _parser.Parse(entries);

            Assert.Equal(2, result.Elevation.Count);
            Assert.Equal(0, result.Elevation[0].X);
            Assert.Equal(0, result.Elevation[0].Y);
            Assert.Equal(1.0f, result.Elevation[0].Elevation);
            Assert.Equal(1, result.Elevation[1].X);
            Assert.Equal(1, result.Elevation[1].Y);
            Assert.Equal(2.0f, result.Elevation[1].Elevation);
        }

        [Fact]
        public void Parse_EmptyElevationData_ShouldNotAddElevationGrids()
        {
            var entries = new List<string> { @"["".dhmap"", []]" };
            var result = _parser.Parse(entries);

            Assert.Empty(result.Elevation);
        }

        [Fact]
        public void Parse_InvalidElevationData_ShouldNotAddElevationGrids()
        {
            var entries = new List<string> { @"["".dhmap"", [""invalid""]]" };
            var result = _parser.Parse(entries);

            Assert.Empty(result.Elevation);
        }

        [Fact]
        public void GetTransform_ShouldReturnCorrectMatrixForDefaultObject()
        {
            var array = new object[]
            {
                null, null, null,
                new object[] { 0, 0, 0 },
                new object[] { 0, 1, 0 },
                new object[] { 0, 0, 1 },
                new object[] { 0, 1, 0 },
                1
            };
            var model = "defaultModel.p3d";
            var result = _parser.GetTransform(array, model);
            Assert.Equal(new Matrix4x4(-1, 0, 0, 0,
                                       0, -0, 1, 0,
                                       0, 1, 0, 0,
                                       0, 0, 0, 1), result);
        }
    }
}
