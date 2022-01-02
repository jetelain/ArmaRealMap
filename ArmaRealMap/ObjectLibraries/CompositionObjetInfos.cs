using System;
using System.Numerics;
using System.Text.Json.Serialization;
using ArmaRealMap.Geometries;

namespace ArmaRealMap.Libraries
{
    public class CompositionObjetInfos
    {
        private readonly SingleObjetInfos singleObjetInfos = new SingleObjetInfos();

        public float Width 
        { 
            get { return singleObjetInfos.Width; }
            set { singleObjetInfos.Width = value; } 
        }

        internal TerrainObject ToObject(IBoundingShape box)
        {
            var delta = Vector2.Transform(
                new Vector2(X, Y),
                Matrix3x2.CreateRotation(box.Angle * MathF.PI / 180f)
            );
            var objBox = new BoundingBox(box.Center + delta, Width, Height, box.Angle + Angle);
            return new TerrainObject(singleObjetInfos, objBox, Z);
        }

        public float Depth
        {
            get { return singleObjetInfos.Depth; }
            set { singleObjetInfos.Depth = value; }
        }

        public float Height
        {
            get { return singleObjetInfos.Height; }
            set { singleObjetInfos.Height = value; }
        }

        public float X { get; set; }

        public float Y { get; set; }

        public float Z { get; set; }

        public float Angle { get; set; }

        public string Name
        {
            get { return singleObjetInfos.Name; }
            set { singleObjetInfos.Name = value; }
        }
    }
}