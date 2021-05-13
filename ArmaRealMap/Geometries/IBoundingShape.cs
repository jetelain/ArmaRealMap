using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using NetTopologySuite.Geometries;

namespace ArmaRealMap.Geometries
{
    public interface IBoundingShape
    {
        TerrainPoint Center { get; }

        float Angle { get; }

        Polygon Poly { get; }

        Vector2 StartPoint { get; }

        Vector2 EndPoint { get; }
    }
}
