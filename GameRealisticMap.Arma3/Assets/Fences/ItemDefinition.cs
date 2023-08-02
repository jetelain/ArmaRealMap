using System.Text.Json.Serialization;
using GameRealisticMap.Algorithms.Definitions;

namespace GameRealisticMap.Arma3.Assets.Fences
{
    public class ItemDefinition : IItemDefinition<Composition>
    {
        [JsonConstructor]
        public ItemDefinition(float radius, Composition model, float? maxZ, float? minZ, double probability, float? maxScale, float? minScale)
        {
            Radius = radius;
            Model = model;
            Probability = probability;

            if (maxScale != null && minScale != null)
            {
                MaxScale = MathF.Max(maxScale.Value, minScale.Value);
                MinScale = MathF.Min(maxScale.Value, minScale.Value);
            }

            if (maxZ != null && minZ != null)
            {
                MaxZ = MathF.Max(maxZ.Value, minZ.Value);
                MinZ = MathF.Min(maxZ.Value, minZ.Value);
            }
        }

        /// <summary>
        /// Radius between objects on path
        /// </summary>
        public float Radius { get; }

        public Composition Model { get; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public float? MaxZ { get; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public float? MinZ { get; }

        public double Probability { get; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public float? MaxScale { get; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public float? MinScale { get; }
    }
}
