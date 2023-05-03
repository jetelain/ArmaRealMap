﻿using System.Text.Json.Serialization;
using GeoJSON.Text.Feature;
using GeoJSON.Text.Geometry;

namespace GameRealisticMap.ManMade.Roads
{
    public class RoadsData : IGeoJsonData
    {
        [JsonConstructor]
        public RoadsData(List<Road> roads)
        { 
            Roads = roads;
        }

        public List<Road> Roads { get; }

        public IEnumerable<Feature> ToGeoJson()
        {
            return Roads.Select(r => new Feature(new MultiPolygon(r.Polygons.Select(p => p.ToGeoJson())), new Dictionary<string, object>() {
                {"type", "road" },
                {"road", r.RoadType.ToString() }
            }));
        }
    }
}