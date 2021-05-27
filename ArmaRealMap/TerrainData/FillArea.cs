using System;
using ArmaRealMap.Geometries;
using NetTopologySuite.Geometries;

namespace ArmaRealMap
{
    internal class FillArea
    {
        public TerrainPolygon Polygon { get; set; }
        public Polygon Shape { get; set; }
        public double Area { get; set; }
        public int X1 { get; set; }
        public int X2 { get; set; }
        public int Y1 { get; set; }
        public int Y2 { get; set; }
        public int ItemsToAdd { get; set; }
        public Random Random { get; set; }
    }
}