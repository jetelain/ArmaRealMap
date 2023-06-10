using GameRealisticMap.Geometries;
using GeoJSON.Text.Feature;
using GeoJSON.Text.Geometry;

namespace GameRealisticMap
{
    public interface IGeoJsonData
    {
        IEnumerable<Feature> ToGeoJson(Func<TerrainPoint, IPosition> project);
    }
}