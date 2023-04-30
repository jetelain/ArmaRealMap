using System.Text.Json.Serialization;
using GameRealisticMap.Geometries;
using GeoJSON.Text.Feature;

namespace GameRealisticMap.ManMade.Buildings
{
    public class BuildingsData : IGeoJsonData
    {
        [JsonConstructor]
        public BuildingsData(List<Building> buildings)
        {
            Buildings = buildings;
        }

        public List<Building> Buildings { get; }

        public IEnumerable<Feature> ToGeoJson()
        {
            var entrance = new Dictionary<string, object>() {
                    {"type", "buildingEntrance" }
                };

            return Buildings.Select(b => new Feature(b.Box.Polygon.ToGeoJson(), new Dictionary<string, object>() {
                {"type", "building" },
                {"building", b.TypeId.ToString() }
            }))
            .Concat(
                Buildings.Where(e => e.EntranceSide != BoxSide.None).Select(b => new Feature(BoxSideHelper.GetSide(b.Box, b.EntranceSide).ToGeoJson(), entrance))
            );
        }
    }
}
