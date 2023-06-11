using GameRealisticMap.Geometries;
using GeoJSON.Text.Feature;
using GeoJSON.Text.Geometry;

namespace GameRealisticMap.ElevationModel
{
    public class ElevationWithLakesData : IGeoJsonData
    {

        public ElevationWithLakesData(ElevationGrid elevation, List<LakeWithElevation> lakes)
        {
            Elevation = elevation;
            Lakes = lakes;
        }

        public ElevationGrid Elevation { get; }

        public List<LakeWithElevation> Lakes { get; }

        public IEnumerable<Feature> ToGeoJson(Func<TerrainPoint, IPosition> project)
        {
            var properties = new Dictionary<string, object>();
            properties.Add("type", "realLake");
            return Lakes.Select(l => new Feature(l.TerrainPolygon.ToGeoJson(project), properties));
        }
    }
}
