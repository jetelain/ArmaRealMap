using GameRealisticMap.Geometries;

namespace GameRealisticMap.Nature
{
    public interface IBasicTerrainData : IGeoJsonData, INonDefaultArea, IPolygonTerrainData
    {
        new List<TerrainPolygon> Polygons { get; }

        IEnumerable<TerrainPolygon> INonDefaultArea.Polygons => Polygons;
    }
}
