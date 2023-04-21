using GameRealisticMap.Geometries;

namespace GameRealisticMap.Nature
{
    internal interface IBasicTerrainData : IGeoJsonData
    {
        List<TerrainPolygon> Polygons { get; }
    }
}
