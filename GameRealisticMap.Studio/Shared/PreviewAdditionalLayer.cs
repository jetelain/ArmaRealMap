using System.Collections.Generic;
using GameRealisticMap.Geometries;

namespace GameRealisticMap.Studio.Shared
{
    /// <summary>
    /// An optional overlay layer added on top of a <see cref="PreviewMapData"/> preview.
    /// Carries a named set of polygons, paths, or points (only one kind per instance)
    /// drawn in a distinct colour to highlight specific geometry (e.g. condition test hits).
    /// </summary>
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