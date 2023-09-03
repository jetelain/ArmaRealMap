using System.Numerics;

namespace GameRealisticMap.Geometries
{
    public sealed class TerrainPathSegment
    {
        public TerrainPathSegment(List<TerrainPoint> points, float angleWithNext = float.NaN)
        {
            Points = points;
            AngleWithNext = angleWithNext;
            Length = TerrainPath.GetLength(points);
        }

        public IReadOnlyList<TerrainPoint> Points { get; }

        public bool HasNext => !float.IsNaN(AngleWithNext);

        public float AngleWithNext { get; }

        public float Length { get; }

        public bool IsClosed => Points[0].Equals(Points[Points.Count - 1]);

        /// <summary>
        /// Splits a path into segments when a path makes an angle above specified threshold
        /// </summary>
        /// <param name="path"></param>
        /// <param name="threshold"></param>
        /// <returns></returns>
        public static List<TerrainPathSegment> FromPath(TerrainPath path, float threshold = 45)
        {
            return FromPath(path.Points, threshold);
        }

        /// <summary>
        /// Splits a path into segments when a path makes an angle above specified threshold
        /// </summary>
        /// <param name="points"></param>
        /// <param name="threshold"></param>
        /// <returns></returns>
        public static List<TerrainPathSegment> FromPath(IReadOnlyList<TerrainPoint> points, float threshold = 45)
        {
            var segments = new List<TerrainPathSegment>();
            var currentSegment = new List<TerrainPoint>() { points[0] };
            var previousDeltaAngle = 0f;
            var previousPoint = points[0];
            foreach (var point in points.Skip(1))
            {
                var delta = Vector2.Normalize(point.Vector - previousPoint.Vector);
                var deltaAngle = MathF.Atan2(delta.Y, delta.X) * 180 / MathF.PI;
                if (currentSegment.Count > 1)
                {
                    var angle = NormalizeAngle((deltaAngle - previousDeltaAngle) % 360);
                    if (Math.Abs(angle) > threshold)
                    {
                        // Adding this point creates an angle > threshold
                        // Ends current segment, and starts a new one
                        segments.Add(new TerrainPathSegment(points: currentSegment, angleWithNext: angle));
                        currentSegment = new List<TerrainPoint>() { previousPoint, point };
                    }
                    else
                    {
                        currentSegment.Add(point);
                    }
                }
                else
                {
                    currentSegment.Add(point);
                }
                previousPoint = point;
                previousDeltaAngle = deltaAngle;
            }
            if (currentSegment.Count > 1)
            {
                if (segments.Count > 0 && points[0].Equals(points[points.Count - 1]))
                {
                    // It's a loop, compute angle with first segment
                    var delta = Vector2.Normalize(points[1].Vector - points[0].Vector);
                    var deltaAngle = MathF.Atan2(delta.Y, delta.X) * 180 / MathF.PI;
                    var angle = NormalizeAngle((deltaAngle - previousDeltaAngle) % 360);
                    if (Math.Abs(angle) > threshold)
                    {
                        segments.Add(new TerrainPathSegment(points: currentSegment, angleWithNext: angle));
                    }
                    else
                    {
                        // edit first segment
                        var first = segments[0];
                        currentSegment.AddRange(first.Points.Skip(1));
                        segments[0] = new TerrainPathSegment(currentSegment, first.AngleWithNext);
                    }
                }
                else
                {
                    // Not a loop
                    segments.Add(new TerrainPathSegment(currentSegment));
                }
            }
            return segments;
        }

        private static float NormalizeAngle(float angle)
        {
            if (angle > 180)
            {
                return angle - 360;
            }
            if (angle <= -180)
            {
                return angle + 360;
            }
            return angle;
        }
    }
}
