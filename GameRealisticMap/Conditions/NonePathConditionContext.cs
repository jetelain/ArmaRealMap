using GameRealisticMap.Geometries;

namespace GameRealisticMap.Conditions
{
    internal class NonePathConditionContext : IPathConditionContext
    {
        private readonly TerrainPath path;

        public NonePathConditionContext(TerrainPath path)
        {
            this.path = path;
        }

        public float Length => path.Length;

        public float MinElevation => 100;

        public float MaxElevation => 100;

        public float AvgElevation => 100;
    }
}