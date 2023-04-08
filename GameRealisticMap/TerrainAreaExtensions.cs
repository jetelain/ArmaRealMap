using GameRealisticMap.Geometries;

namespace GameRealisticMap
{
    public static class TerrainAreaExtensions
    {
        public static bool IsInside(this ITerrainArea area, TerrainPoint point)
        {
            return point.X >= 0 && 
                point.Y >= 0 && 
                point.X < area.SizeInMeters && 
                point.Y < area.SizeInMeters;
        }
    }
}
