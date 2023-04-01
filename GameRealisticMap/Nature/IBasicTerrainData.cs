using GameRealisticMap.Geometries;

namespace GameRealisticMap.Nature
{
    internal interface IBasicTerrainData : ITerrainData
    {
        List<TerrainPolygon> Polygons { get; }
    }
}
