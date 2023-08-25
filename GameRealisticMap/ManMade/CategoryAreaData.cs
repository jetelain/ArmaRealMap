using GameRealisticMap.Geometries;
using GameRealisticMap.Nature;
using GeoJSON.Text.Feature;

namespace GameRealisticMap.ManMade
{
    public class CategoryAreaData : INonDefaultArea
    {
        public CategoryAreaData(List<CategoryArea> areas)
        {
            Areas = areas;
        }

        public List<CategoryArea> Areas { get; }

        IEnumerable<TerrainPolygon> INonDefaultArea.Polygons => Areas.SelectMany(a => a.PolyList);
    }
}
