using System.Numerics;
using BIS.WRP;
using GameRealisticMap.Arma3.TerrainBuilder;

namespace GameRealisticMap.Arma3.Edit
{
    public class WrpAddObject
    {
        public WrpAddObject(Matrix4x4 transform, string model)
        {
            Transform = transform;
            Model = model;
        }

        public Matrix4x4 Transform { get; }

        public string Model { get; }

        public EditableWrpObject ToWrp()
        {
            return new EditableWrpObject()
            {
                Model = Model,
                Transform = new BIS.Core.Math.Matrix4P(Transform)
            };
        }

        public TerrainBuilderObject ToTerrainBuilder(IModelInfoLibrary library)
        {
            return new TerrainBuilderObject(ToWrp(), library);
        }
    }
}
