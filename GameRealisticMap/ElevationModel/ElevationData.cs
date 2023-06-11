using GameRealisticMap.Geometries;
using GeoJSON.Text.Feature;
using GeoJSON.Text.Geometry;

namespace GameRealisticMap.ElevationModel
{
    public class ElevationData : IGeoJsonData
    {
        public ElevationData(ElevationGrid elevation, IEnumerable<TerrainPath>? contours)
        {
            Elevation = elevation;
            Contours = contours;
        }

        public ElevationGrid Elevation { get; }

        internal IEnumerable<TerrainPath>? Contours { get; }

        public IEnumerable<Feature> ToGeoJson(Func<TerrainPoint, IPosition> project)
        {
            if (Contours == null)
            {
                return Enumerable.Empty<Feature>();
            }
            var properties = new Dictionary<string, object>();
            properties.Add("type", "contour");
            return Contours.Select(g => new Feature(g.ToGeoJson(project), properties));
        }
    }
}
