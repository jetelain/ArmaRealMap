using System;
using System.Text.Json.Serialization;
using ArmaRealMap.Geometries;

namespace ArmaRealMap.Libraries
{
    public abstract class ObjetInfosBase
    {
        public float Width { get; set; }
        public float Depth { get; set; }
        public float Height { get; set; }

        public float CX { get; set; }
        public float CY { get; set; }
        public float CZ { get; set; }

        [JsonIgnore]
        public float Surface { get { return Width * Depth; } }

        internal bool Fits(BoundingBox box, float minFactor, float maxFactor)
        {
            if (Width * minFactor <= box.Width &&
                Width * maxFactor >= box.Width &&
                Depth * minFactor <= box.Height &&
                Depth * maxFactor >= box.Height) // 0° rotated
            {
                return true;
            }
            if (Depth * minFactor <= box.Width &&
                Depth * maxFactor >= box.Width &&
                Width * minFactor <= box.Height &&
                Width * maxFactor >= box.Height) // 90° rotated
            {
                return true;
            }
            return false;
        }

        internal float RotateToFit(BoundingBox box, float minFactor, float maxFactor)
        {
            if (Width * minFactor <= box.Width &&
                Width * maxFactor >= box.Width &&
                Depth * minFactor <= box.Height &&
                Depth * maxFactor >= box.Height) // 0° rotated
            {
                return 0.0f;
            }
            if (Depth * minFactor <= box.Width &&
                Depth * maxFactor >= box.Width &&
                Width * minFactor <= box.Height &&
                Width * maxFactor >= box.Height) // 90° rotated
            {
                return 90.0f ;
            }
            throw new ArgumentException();
        }
    }
}