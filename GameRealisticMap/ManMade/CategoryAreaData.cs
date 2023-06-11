using GeoJSON.Text.Feature;

namespace GameRealisticMap.ManMade
{
    public class CategoryAreaData
    {
        public CategoryAreaData(List<CategoryArea> areas)
        {
            Areas = areas;
        }

        public List<CategoryArea> Areas { get; }
    }
}
