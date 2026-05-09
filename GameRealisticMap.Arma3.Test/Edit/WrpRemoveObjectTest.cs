using System.Numerics;
using BIS.Core.Math;
using BIS.WRP;
using GameRealisticMap.Arma3.Edit;

namespace GameRealisticMap.Arma3.Test.Edit
{
    public class WrpRemoveObjectTest
    {
        private static WrpRemoveObject CreateRemoveObject(float x, float y, float z, string model = "path\\to\\model.p3d", int objectId = 1)
        {
            return new WrpRemoveObject(Matrix4x4.CreateTranslation(x, y, z), model, objectId);
        }

        private static EditableWrpObject CreateWrpObject(float x, float y, float z, string model = "path\\to\\model.p3d")
        {
            return new EditableWrpObject()
            {
                Model = model,
                Transform = new Matrix4P(Matrix4x4.CreateTranslation(x, y, z))
            };
        }

        [Fact]
        public void Constructor_SetsProperties()
        {
            var transform = Matrix4x4.CreateTranslation(10f, 5f, 20f);
            var obj = new WrpRemoveObject(transform, "path\\to\\model.p3d", 42);

            Assert.Equal(transform, obj.Transform);
            Assert.Equal("path\\to\\model.p3d", obj.Model);
            Assert.Equal("model", obj.ModelName);
            Assert.Equal(42, obj.ObjectId);
            Assert.Equal(new Vector3(10f, 5f, 20f), obj.WorldPos);
            Assert.Equal(new Vector2(10f, 20f), obj.Pos2D);
        }

        [Fact]
        public void Match_SameModelAndClosePosition_ReturnsTrue()
        {
            var removeObj = CreateRemoveObject(10f, 5f, 20f);
            var wrpObj = CreateWrpObject(10f, 5f, 20f);

            Assert.True(removeObj.Match(wrpObj));
        }

        [Fact]
        public void Match_SameModelPositionSlightlyDifferent_ReturnsTrue()
        {
            // LengthSquared threshold is 0.02f; offset of 0.05 in one axis gives 0.0025 < 0.02
            var removeObj = CreateRemoveObject(10f, 5f, 20f);
            var wrpObj = CreateWrpObject(10.05f, 5.05f, 20.05f);

            Assert.True(removeObj.Match(wrpObj));
        }

        [Fact]
        public void Match_SameModelFarPosition_ReturnsFalse()
        {
            var removeObj = CreateRemoveObject(10f, 5f, 20f);
            var wrpObj = CreateWrpObject(100f, 5f, 20f);

            Assert.False(removeObj.Match(wrpObj));
        }

        [Fact]
        public void Match_DifferentModel_ReturnsFalse()
        {
            var removeObj = CreateRemoveObject(10f, 5f, 20f, "path\\to\\model_a.p3d");
            var wrpObj = CreateWrpObject(10f, 5f, 20f, "path\\to\\model_b.p3d");

            Assert.False(removeObj.Match(wrpObj));
        }

        [Fact]
        public void Match_ModelDifferentCase_ReturnsTrue()
        {
            var removeObj = CreateRemoveObject(10f, 5f, 20f, "path\\to\\MODEL.p3d");
            var wrpObj = CreateWrpObject(10f, 5f, 20f, "path\\to\\model.p3d");

            Assert.True(removeObj.Match(wrpObj));
        }

        [Fact]
        public void MatchRelaxed_ModelNameContainedAndClosePosition_ReturnsTrue()
        {
            // MatchRelaxed checks: removeObj.ModelName.Contains(wrpObj's model name)
            // So the wrp object's model name must be a substring of the remove object's model name
            var removeObj = CreateRemoveObject(10f, 5f, 20f, "path\\to\\model_v2.p3d");
            var wrpObj = CreateWrpObject(10f, 5f, 20f, "path\\to\\model.p3d");

            Assert.True(removeObj.MatchRelaxed(wrpObj));
        }

        [Fact]
        public void MatchRelaxed_ModelNameNotContained_ReturnsFalse()
        {
            var removeObj = CreateRemoveObject(10f, 5f, 20f, "path\\to\\modelA.p3d");
            var wrpObj = CreateWrpObject(10f, 5f, 20f, "path\\to\\modelB.p3d");

            Assert.False(removeObj.MatchRelaxed(wrpObj));
        }

        [Fact]
        public void MatchRelaxed_FarPosition_ReturnsFalse()
        {
            var removeObj = CreateRemoveObject(10f, 5f, 20f, "path\\to\\model.p3d");
            var wrpObj = CreateWrpObject(100f, 5f, 20f, "path\\to\\model_v2.p3d");

            Assert.False(removeObj.MatchRelaxed(wrpObj));
        }
    }
}
