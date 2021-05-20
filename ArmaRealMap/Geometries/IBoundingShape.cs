using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using NetTopologySuite.Geometries;

namespace ArmaRealMap.Geometries
{
    public interface IBoundingShape : ITerrainGeometry
    {
        TerrainPoint Center { get; }

        float Angle { get; }

        Polygon Poly { get; }
    }
}
