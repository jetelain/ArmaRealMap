using System.Diagnostics;
using System.Numerics;
using GameRealisticMap.Arma3.TerrainBuilder;

namespace GameRealisticMap.Arma3.Assets
{
    internal class CompositionObject
    {
        public CompositionObject(ModelInfo model, Matrix4x4 transform)
        {
            Model = model;
            Transform = transform;
        }

        public ModelInfo Model { get; }

        public Matrix4x4 Transform { get; }

        internal TerrainBuilderObject ToTerrainBuilderObject(Matrix4x4 matrix, ElevationMode mode)
        {
            return new TerrainBuilderObject(Model, Transform * matrix, mode);
        }
    }
}