using GameRealisticMap.Geometries;

namespace GameRealisticMap.Nature
{
    public interface INonDefaultArea
    {
        IEnumerable<TerrainPolygon> Polygons { get; }
    }
}
