using GameRealisticMap.Geometries;
using GeoJSON.Text.Feature;
using GeoJSON.Text.Geometry;

namespace GameRealisticMap.Nature.Trees
{
    public class TreeRowsData : IGeoJsonData
    {
        public TreeRowsData(List<TerrainPath> rows)
        {
            Rows = rows;
        }

        public List<TerrainPath> Rows { get; }

        public IEnumerable<Feature> ToGeoJson(Func<TerrainPoint, IPosition> project)
        {
            var properties = new Dictionary<string, object>() {
                {"type", "treerow" }
            };
            return Rows.Select(b => new Feature(b.ToGeoJson(project), properties));
        }
    }
}
