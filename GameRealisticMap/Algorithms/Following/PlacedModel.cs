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

        public float RelativeElevation => 0f;

        public float Scale => 1f;

        public TModel Model { get; }
    }
}