using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using GameRealisticMap.Geometries;
using GameRealisticMap.ManMade.Railways;
using GameRealisticMap.Nature;
using GeoJSON.Text.Feature;
using GeoJSON.Text.Geometry;

namespace GameRealisticMap.ManMade.Cutlines
{
    public class CutlinesData : IGeoJsonData//, INonDefaultArea
    {
        [JsonConstructor]
        public CutlinesData(List<TerrainPolygon> polygons)
        {
            Polygons = polygons;
        }

        public List<TerrainPolygon> Polygons { get; }

        public IEnumerable<Feature> ToGeoJson(Func<TerrainPoint, IPosition> project)
        {
            var properties = new Dictionary<string, object>() {
                {"type", "cutline" }
            };
            return Polygons.Select(r => new Feature(r.ToGeoJson(project), properties));
        }
    }
}
