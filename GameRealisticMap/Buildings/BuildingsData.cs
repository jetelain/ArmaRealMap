using GeoJSON.Text.Feature;

namespace GameRealisticMap.Buildings
{
    public class BuildingsData : ITerrainData
    {
        public BuildingsData(List<Building> buildings)
        {
            Buildings = buildings;
        }

        public List<Building> Buildings { get; }

        public IEnumerable<Feature> ToGeoJson()
        {
            return Buildings.Select(b => new Feature(b.Box.Polygon.ToGeoJson(), new Dictionary<string, object>() {
                {"type", "building" },
                {"building", b.TypeId.ToString() }
            }));
        }
    }
}
