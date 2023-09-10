using System.Collections.Generic;
using GameRealisticMap.Geometries;

namespace GameRealisticMap.Studio.Shared
{
    public class PreviewAdditionalLayer
    {
        public PreviewAdditionalLayer(string name, List<TerrainPolygon> polygons)
        {
            Name = name;
            Polygons = polygons;
        }

        public PreviewAdditionalLayer(string name, List<TerrainPath> paths)
        {
            Name = name;
            Paths = paths;
        }

        public PreviewAdditionalLayer(string name, List<TerrainPoint> points)
        {
            Name = name;
            Points = points;
        }

        public string Name { get; }

        public List<TerrainPoint>? Points { get; }

        public List<TerrainPath>? Paths { get; }

        public List<TerrainPolygon>? Polygons { get; }
    }
}