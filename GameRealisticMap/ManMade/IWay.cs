using GameRealisticMap.Geometries;

namespace GameRealisticMap.ManMade
{
    internal interface IWay : ITerrainEnvelope
    {
        float Width { get; }

        float ClearWidth { get; }

        IEnumerable<TerrainPolygon> Polygons { get; }

        IEnumerable<TerrainPolygon> ClearPolygons { get; }

        WaySpecialSegment SpecialSegment { get; }

        TerrainPath Path { get; }
    }

}