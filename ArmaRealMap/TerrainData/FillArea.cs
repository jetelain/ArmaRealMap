using System;
using NetTopologySuite.Geometries;

namespace ArmaRealMap
{
    internal class FillArea
    {
        public int X1 { get; internal set; }
        public Polygon Shape { get; internal set; }
        public Polygon CroppedShape { get; internal set; }
        public double CroppedArea { get; internal set; }
        public int Y1 { get; internal set; }
        public int Y2 { get; internal set; }
        public int X2 { get; internal set; }
        public int ItemsToAdd { get; internal set; }
        public Random Random { get; internal set; }
        public int ProcessZone { get; internal set; }
    }
}