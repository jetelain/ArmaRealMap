using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameRealisticMap.Geometries;
using GeoJSON.Text.Feature;

namespace GameRealisticMap.Nature.Scrubs
{
    public class ScrubRadialData : IBasicTerrainData
    {
        public ScrubRadialData(List<TerrainPolygon> polygons)
        {
            Polygons = polygons;
        }

        public List<TerrainPolygon> Polygons { get; }

        public IEnumerable<Feature> ToGeoJson()
        {
            var properties = new Dictionary<string, object>() {
                {"type", "scrubRadial" }
            };
            return Polygons.Select(b => new Feature(b.ToGeoJson(), properties));
        }
    }
}
