using GameRealisticMap.Geometries;

namespace GameRealisticMap.ElevationModel
{
    /// <summary>
    /// Associates a lake polygon (<see cref="Nature.Lakes.LakesData"/>) with its
    /// computed water surface elevation in metres. Used during elevation constraint solving
    /// to flatten the grid within each lake to a consistent level.
    /// </summary>
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