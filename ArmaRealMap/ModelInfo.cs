using System;
using System.Numerics;
using ArmaRealMap.ElevationModel;
using ArmaRealMap.Geometries;

namespace ArmaRealMap
{
    public class ModelInfo
    {
        public string Name { get; set; }

        public string Path { get; set; }

        public float CenterToElevation { get; set; }

        public Vector2 MinPoint { get; set; } = new Vector2(-1, -1);

        public Vector2 MaxPoint { get; set; } = new Vector2(1, 1);

        internal float GetElevation(ElevationGrid grid, TerrainPoint point)
        {
            return grid.ElevationAt(point) + CenterToElevation;
        }
    }
}