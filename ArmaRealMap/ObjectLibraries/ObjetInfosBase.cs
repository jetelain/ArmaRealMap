using System;
using System.Numerics;
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
        public Vector2 Size2D => new Vector2(Width, Depth);

        [JsonIgnore]
        public float Surface { get { return Width * Depth; } }

        // Width = 10
        // box.Width = 15

        // Width/box.Width >= minFactor * box.Width
        //
        // // && Width/box.Width <= maxFactor




        internal bool Fits(BoundingBox box, float minFactor, float maxFactor)
        {
            var fWidth = Width / box.Width;
            var fDepth = Depth / box.Height;

            if ( minFactor <= fWidth &&
                 maxFactor >= fWidth &&
                 minFactor <= fDepth &&
                 maxFactor >= fDepth) // 0° rotated
            {
                return true;
            }

            fWidth = Width / box.Height;
            fDepth = Depth / box.Width;
            if (minFactor <= fWidth &&
                 maxFactor >= fWidth &&
                 minFactor <= fDepth &&
                 maxFactor >= fDepth) // 90° rotated
            {
                return true;
            }
            return false;
        }

        internal float RotateToFit(BoundingBox box, float minFactor, float maxFactor)
        {
            var fWidth = Width / box.Width;
            var fDepth = Depth / box.Height;

            if (minFactor <= fWidth &&
                 maxFactor >= fWidth &&
                 minFactor <= fDepth &&
                 maxFactor >= fDepth) // 0° rotated
            {
                return 0.0f;
            }
            fWidth = Width / box.Height;
            fDepth = Depth / box.Width;
            if (minFactor <= fWidth &&
                 maxFactor >= fWidth &&
                 minFactor <= fDepth &&
                 maxFactor >= fDepth) // 90° rotated
            {
                return 90.0f ;
            }
            throw new ArgumentException();
        }
    }
}