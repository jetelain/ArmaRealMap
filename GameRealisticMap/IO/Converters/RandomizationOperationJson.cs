using System.Numerics;
using System.Text.Json.Serialization;
using GameRealisticMap.Algorithms.Randomizations;

namespace GameRealisticMap.IO.Converters
{
    public sealed class RandomizationOperationJson
    {
        public RandomizationOperationJson(RandomizationOperation type, float min, float max, Vector3? centerPoint = null)
        {
            Type = type;
            Min = min;
            Max = max;
            CenterPoint = centerPoint;
        }

        public RandomizationOperation Type { get; }

        public float Min { get; }

        public float Max { get; }

        [JsonConverter(typeof(Vector3Converter))]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Vector3? CenterPoint { get; }

        public IRandomizationOperation ToRandomizationOperation()
        {
            switch (Type)
            {
                case RandomizationOperation.RotateX: return new RotateXRandomization(Min, Max, CenterPoint ?? throw new InvalidOperationException());
                case RandomizationOperation.RotateY: return new RotateYRandomization(Min, Max, CenterPoint ?? throw new InvalidOperationException());
                case RandomizationOperation.RotateZ: return new RotateZRandomization(Min, Max, CenterPoint ?? throw new InvalidOperationException());
                case RandomizationOperation.TranslateX: return new TranslateXRandomization(Min, Max);
                case RandomizationOperation.TranslateY: return new TranslateYRandomization(Min, Max);
                case RandomizationOperation.TranslateZ: return new TranslateZRandomization(Min, Max);
                case RandomizationOperation.ScaleUniform: return new ScaleUniformRandomization(Min, Max, CenterPoint ?? throw new InvalidOperationException());
                case RandomizationOperation.TranslateRadiusXY: return new TranslateRadiusXYRandomization(Min, Max);
                case RandomizationOperation.TranslateRadiusXZ: return new TranslateRadiusXZRandomization(Min, Max);
                case RandomizationOperation.TranslateRadiusYZ: return new TranslateRadiusYZRandomization(Min, Max);
                default: throw new InvalidOperationException();
            }
        }

        public static RandomizationOperationJson From(IRandomizationOperation operation)
        {
            switch (operation)
            {
                case RotateXRandomization o: return new RandomizationOperationJson(RandomizationOperation.RotateX, o.Min, o.Max, o.CenterPoint);
                case RotateYRandomization o: return new RandomizationOperationJson(RandomizationOperation.RotateY, o.Min, o.Max, o.CenterPoint);
                case RotateZRandomization o: return new RandomizationOperationJson(RandomizationOperation.RotateZ, o.Min, o.Max, o.CenterPoint);
                case TranslateXRandomization o: return new RandomizationOperationJson(RandomizationOperation.TranslateX, o.Min, o.Max);
                case TranslateYRandomization o: return new RandomizationOperationJson(RandomizationOperation.TranslateY, o.Min, o.Max);
                case TranslateZRandomization o: return new RandomizationOperationJson(RandomizationOperation.TranslateZ, o.Min, o.Max);
                case ScaleUniformRandomization o: return new RandomizationOperationJson(RandomizationOperation.ScaleUniform, o.Min, o.Max, o.CenterPoint);
                case TranslateRadiusXYRandomization o: return new RandomizationOperationJson(RandomizationOperation.TranslateRadiusXY, o.Min, o.Max);
                case TranslateRadiusXZRandomization o: return new RandomizationOperationJson(RandomizationOperation.TranslateRadiusXZ, o.Min, o.Max);
                case TranslateRadiusYZRandomization o: return new RandomizationOperationJson(RandomizationOperation.TranslateRadiusYZ, o.Min, o.Max);
                default: throw new ArgumentException();
            }
        }
    }
}
