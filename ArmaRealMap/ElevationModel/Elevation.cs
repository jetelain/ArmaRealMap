using System;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using SixLabors.ImageSharp.PixelFormats;

namespace ArmaRealMap.ElevationModel
{
    public struct Elevation : IPixel<Elevation>
    {
        public const float MaxValue = 9216;
        public const float MinValue = -7168;

        public float Value;

        public Elevation(float value)
        {
            Value = value;
        }

        public bool Equals(Elevation other)
        {
            return Value == other.Value;
        }

        public PixelOperations<Elevation> CreatePixelOperations()
        {
            return new PixelOperations<Elevation>();
        }

        public void FromArgb32(Argb32 source)
        {
            FromVector4(source.ToVector4());
        }

        public void FromBgr24(Bgr24 source)
        {
            FromVector4(source.ToVector4());
        }

        public void FromBgra32(Bgra32 source)
        {
            FromVector4(source.ToVector4());
        }

        public void FromBgra5551(Bgra5551 source)
        {
            FromVector4(source.ToVector4());
        }

        public void FromL16(L16 source)
        {
            FromVector4(source.ToVector4());
        }

        public void FromL8(L8 source)
        {
            FromVector4(source.ToVector4());
        }

        public void FromLa16(La16 source)
        {
            FromVector4(source.ToVector4());
        }

        public void FromLa32(La32 source)
        {
            FromVector4(source.ToVector4());
        }

        public void FromRgb24(Rgb24 source)
        {
            FromVector4(source.ToVector4());
        }

        public void FromRgb48(Rgb48 source)
        {
            FromVector4(source.ToVector4());
        }

        public void FromRgba32(Rgba32 source)
        {
            FromVector4(source.ToVector4());
        }

        public void FromRgba64(Rgba64 source)
        {
            FromVector4(source.ToVector4());
        }

        public void FromScaledVector4(Vector4 vector)
        {
            FromVector4(vector);
        }

        public void FromVector4(Vector4 vector)
        {
            Value = (vector.Z * (MaxValue - MinValue)) + MinValue;
        }

        public void ToRgba32(ref Rgba32 dest)
        {
            dest.FromVector4(ToVector4());
        }

        public Vector4 ToScaledVector4()
        {
            return ToVector4();
        }

        public Vector4 ToVector4()
        {
            return new Vector4(0, 0, (Value - MinValue) / (MaxValue - MinValue), 1f);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is Elevation elevation)
            {
                return Equals(elevation);
            }
            return false;
        }
    }
}
