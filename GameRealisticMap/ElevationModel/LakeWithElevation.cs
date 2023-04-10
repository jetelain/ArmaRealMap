using GameRealisticMap.Geometries;

namespace GameRealisticMap.ElevationModel
{
    public class LakeWithElevation
    {
        public LakeWithElevation(TerrainPolygon terrainPolygon, float borderElevation, float waterElevation)
        {
            BorderElevation = borderElevation;
            TerrainPolygon= terrainPolygon;
            WaterElevation = waterElevation;
        }

        public float BorderElevation { get; }

        public TerrainPolygon TerrainPolygon { get; }

        public float WaterElevation { get; }
    }
}