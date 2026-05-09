using GameRealisticMap.Arma3.Edit;

namespace GameRealisticMap.Arma3.Test.Edit
{
    public class WrpEditBatchTest
    {
        [Fact]
        public void IsComplete_NullPartCount_ReturnsTrue()
        {
            var batch = new WrpEditBatch();
            Assert.True(batch.IsComplete);
        }

        [Fact]
        public void IsComplete_PartCountMatchesIndexCount_ReturnsTrue()
        {
            var batch = new WrpEditBatch();
            batch.PartIndexes.Add(1);
            batch.PartIndexes.Add(2);
            batch.PartCount = 2;
            Assert.True(batch.IsComplete);
        }

        [Fact]
        public void IsComplete_PartCountDoesNotMatchIndexCount_ReturnsFalse()
        {
            var batch = new WrpEditBatch();
            batch.PartIndexes.Add(1);
            batch.PartCount = 3;
            Assert.False(batch.IsComplete);
        }

        [Fact]
        public void PartIndex_NoPartIndexes_ReturnsNull()
        {
            var batch = new WrpEditBatch();
            Assert.Null(batch.PartIndex);
        }

        [Fact]
        public void PartIndex_WithPartIndexes_ReturnsMax()
        {
            var batch = new WrpEditBatch();
            batch.PartIndexes.Add(1);
            batch.PartIndexes.Add(5);
            batch.PartIndexes.Add(3);
            Assert.Equal(5, batch.PartIndex);
        }

        [Fact]
        public void DefaultValues()
        {
            var batch = new WrpEditBatch();
            Assert.Equal(string.Empty, batch.WorldName);
            Assert.Equal(0, batch.WorldSize);
            Assert.Equal(0, batch.Revision);
            Assert.False(batch.ElevationAdjustObjects);
            Assert.Empty(batch.Add);
            Assert.Empty(batch.Remove);
            Assert.Empty(batch.Elevation);
            Assert.Empty(batch.PartIndexes);
            Assert.Null(batch.PartCount);
        }
    }
}
