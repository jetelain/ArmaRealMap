using System.Numerics;
using System.Text.Json.Serialization;

namespace GameRealisticMap.Arma3.Assets.Detection
{
    public class RectangleBasedPlacement
    {
        public RectangleBasedPlacement(Vector2 center, Vector2 size)
        {
            Center = center;
            Size = size;
        }

        public Vector2 Center { get; }

        public Vector2 Size { get; }

        [JsonIgnore]
        public Vector2 Min => Center - (Size / 2);

        [JsonIgnore]
        public Vector2 Max => Center + (Size / 2);
    }
}
