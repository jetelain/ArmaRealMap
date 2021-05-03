using System.Globalization;
using System.Numerics;
using SixLabors.ImageSharp.PixelFormats;

namespace ArmaRealMap.ElevationModel
{
    public struct Elevation : IPixel<Elevation>
    {
        // 10000m -> Requires 20 bits for centimetric precision, should be fine on float32 (and on Vector4)
        public const float MaxValue = 9000;
        public const float MinValue = -1000;

        public float Value;
        public float Alpha;

        public Elevation(float value)
        {
            Value = value;
            Alpha = 1f;
        }

        public Elevation(double value)
        {
            Value = (float)value;
            Alpha = 1f;
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
            Alpha = vector.W;
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
            return new Vector4(0, 0, (Value - MinValue) / (MaxValue - MinValue), Alpha);
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

        public override string ToString()
        {
            return Value.ToString("0.00", CultureInfo.InvariantCulture);
        }
    }
}
