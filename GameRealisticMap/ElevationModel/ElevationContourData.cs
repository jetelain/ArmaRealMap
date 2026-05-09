using GameRealisticMap.Geometries;
using GeoJSON.Text.Feature;
using GeoJSON.Text.Geometry;

namespace GameRealisticMap.ElevationModel
{
    /// <summary>
    /// Contour line paths derived from <see cref="ElevationData"/> at a fixed contour interval.
    /// Used for map overview rendering (e.g. topographic preview images), not written to WRP.
    /// </summary>
    public sealed class ElevationContourData : IGeoJsonData
    {
        public ElevationContourData(List<TerrainPath> contours)
        {
            Contours = contours;
        }

        public List<TerrainPath> Contours { get; }

        public IEnumerable<Feature> ToGeoJson(Func<TerrainPoint, IPosition> project)
        {
            var properties = new Dictionary<string, object>();
            properties.Add("type", "contour");
            return Contours.Select(g => new Feature(g.ToGeoJson(project), properties));
        }
    }
}
