using GameRealisticMap.Geometries;

namespace GameRealisticMap.Algorithms.Following
{
    public sealed class PlacedModel<TModel> : IPlacedModel<TModel>
    {
        public PlacedModel(TModel model, TerrainPoint center, float angle, float relativeElevation = 0f, float scale = 1f)
        {
            Model = model;
            Center = center;
            Angle = angle;
            RelativeElevation = relativeElevation;
            Scale = scale;
        }

        public float Angle { get; }

        public TerrainPoint Center { get; }

        public float RelativeElevation { get; }

        public float Scale { get; }

        public TModel Model { get; }
    }
}