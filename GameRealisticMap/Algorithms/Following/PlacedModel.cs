using GameRealisticMap.Geometries;

namespace GameRealisticMap.Algorithms.Following
{
    public class PlacedModel<TModel> : IModelPosition
    {
        public PlacedModel(TModel model, TerrainPoint center, float angle)
        {
            Model = model;
            Center = center;
            Angle = angle;
        }

        public float Angle { get; }

        public TerrainPoint Center { get; }

        public float RelativeElevation { get; }

        public float Scale { get; }

        public TModel Model { get; }
    }
}