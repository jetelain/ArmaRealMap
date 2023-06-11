using GameRealisticMap.Geometries;

namespace GameRealisticMap.Nature
{
    public interface IBasicTerrainData : IGeoJsonData
    {
        List<TerrainPolygon> Polygons { get; }
    }
}
