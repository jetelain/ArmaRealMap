using System.Collections.Generic;
using System.Numerics;

namespace ArmaRealMap.Geometries
{
    public class FollowPath
    {
        private static readonly Matrix3x2 Rotate90 = Matrix3x2.CreateRotation(1.570796f);
        private static readonly Matrix3x2 RotateM90 = Matrix3x2.CreateRotation(-1.570796f);

        private readonly IEnumerator<TerrainPoint> enumerator;
        private TerrainPoint previous;
        private TerrainPoint point;
        private TerrainPoint position;
        private Vector2 delta;
        private float length;
        private float positionOnSegment;
        private bool hasReachedEnd;

        public FollowPath(params TerrainPoint[] points)
            : this((IEnumerable<TerrainPoint>)points)
        {

        }

        public FollowPath(IEnumerable<TerrainPoint> points)
        {
            enumerator = points.GetEnumerator();
            Init();
        }

        public virtual void Reset()
        {
            enumerator.Reset();
            Init();
        }
        private void Init()
        {
            length = 0f;
            positionOnSegment = 0f;
            previous = null;
            if (enumerator.MoveNext())
            {
                position = point = enumerator.Current;
                delta = Vector2.Zero;
                hasReachedEnd = false;
                MoveNextPoint();
            }
            else
            {
                hasReachedEnd = true;
            }
        }

        private bool MoveNextPoint()
        {
            previous = point;
            if (!enumerator.MoveNext())
            {
                point = null;
                length = 0f;
                positionOnSegment = 0f;
                return false;
            }
            point = enumerator.Current;
            delta = point.Vector - previous.Vector;
            length = delta.Length();
            positionOnSegment = 0f;
            return true;
        }

        public TerrainPoint Current => position;

        public Vector2 Vector => Vector2.Normalize(delta);

        public Vector2 Vector90 => Vector2.Transform(Vector, Rotate90);

        public Vector2 VectorM90 => Vector2.Transform(Vector, RotateM90);

        public bool Move(float step)
        {
            if (hasReachedEnd)
            {
                return false;
            }
            var remainLength = step;
            while (remainLength + positionOnSegment > length)
            {
                remainLength -= length - positionOnSegment;
                if (!MoveNextPoint())
                {
                    hasReachedEnd = true;
                    if (position != previous)
                    {
                        position = previous;
                        return true;
                    }
                    return false;
                }
            }
            positionOnSegment += remainLength;
            position = new TerrainPoint(Vector2.Lerp(previous.Vector, point.Vector, positionOnSegment / length));
            return true;
        }

    }
}
