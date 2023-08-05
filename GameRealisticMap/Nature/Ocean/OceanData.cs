using GameRealisticMap.Geometries;
using GeoJSON.Text.Feature;
using GeoJSON.Text.Geometry;

namespace GameRealisticMap.Nature.Ocean
{
    public class OceanData : IBasicTerrainData
    {
        public OceanData(List<TerrainPolygon> polygons, List<TerrainPolygon> land)
        {
            Polygons = polygons;
            Land = land;
        }

        public List<TerrainPolygon> Polygons { get; }

        public List<TerrainPolygon> Land { get; }

        public IEnumerable<Feature> ToGeoJson(Func<TerrainPoint, IPosition> project)
        {
            var properties = new Dictionary<string, object>() {
                {"type", "ocean" }
            };
            return Polygons.Select(b => new Feature(b.ToGeoJson(project), properties));
        }
    }
}
