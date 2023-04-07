using GameRealisticMap.Geometries;

namespace GameRealisticMap.ElevationModel
{
    public class LakeWithElevation
    {
        public LakeWithElevation(TerrainPolygon terrainPolygon, float borderElevation)
        {
            BorderElevation = borderElevation;
            TerrainPolygon= terrainPolygon;
            WaterElevation = borderElevation - 0.1f;
        }

        public float BorderElevation { get; }

        public TerrainPolygon TerrainPolygon { get; }

        public float WaterElevation { get; }
    }
}