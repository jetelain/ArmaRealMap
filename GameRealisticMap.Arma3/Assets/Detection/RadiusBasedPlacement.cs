using System.Numerics;

namespace GameRealisticMap.Arma3.Assets.Detection
{
    public class RadiusBasedPlacement
    {
        public RadiusBasedPlacement(Vector2 center, float radius)
        {
            Center = center;
            Radius = radius;
        }

        public Vector2 Center { get; }

        public float Radius { get; }
    }
}
