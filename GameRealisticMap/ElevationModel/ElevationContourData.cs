using GameRealisticMap.Geometries;
using GeoJSON.Text.Feature;
using GeoJSON.Text.Geometry;

namespace GameRealisticMap.ElevationModel
{
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
