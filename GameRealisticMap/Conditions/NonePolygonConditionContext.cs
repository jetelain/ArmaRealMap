using GameRealisticMap.Geometries;

namespace GameRealisticMap.Conditions
{
    internal class NonePolygonConditionContext : IPolygonConditionContext
    {
        private readonly TerrainPolygon polygon;

        public NonePolygonConditionContext(TerrainPolygon polygon)
        {
            this.polygon = polygon;
        }

        public float Area => (float)polygon.Area;

        public float MinElevation => 100;

        public float MaxElevation => 100;

        public float AvgElevation => 100;
    }
}