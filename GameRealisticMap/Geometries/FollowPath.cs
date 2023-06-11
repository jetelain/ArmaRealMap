using System.Numerics;

namespace GameRealisticMap.Geometries
{
    public class FollowPath
    {
        private readonly IEnumerator<TerrainPoint> enumerator;
        private TerrainPoint? previousPoint;
        private TerrainPoint? point;
        private TerrainPoint? previousPosition;
        private TerrainPoint? position;
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
            IsAfterRightAngle = false;
            previousPosition = null;
            length = 0f;
            positionOnSegment = 0f;
            previousPoint = null;
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
            previousPoint = point;
            if (!enumerator.MoveNext())
            {
                point = null;
                length = 0f;
                positionOnSegment = 0f;
                return false;
            }
            point = enumerator.Current;
            delta = point.Vector - previousPoint.Vector;
            length = delta.Length();
            positionOnSegment = 0f;
            return true;
        }

        public TerrainPoint Current => position;

        public TerrainPoint Previous => previousPosition;

        public Vector2 Vector => Vector2.Normalize(delta);

        public Vector2 Vector90 => Vector2.Transform(Vector, GeometryHelper.Rotate90);

        public Vector2 VectorM90 => Vector2.Transform(Vector, GeometryHelper.RotateM90);

        public bool KeepRightAngles { get; set; }

        public bool IsAfterRightAngle { get; private set; }

        public bool IsLast => hasReachedEnd;

        public bool Move(float step)
        {
            IsAfterRightAngle = false;
            if (hasReachedEnd)
            {
                return false;
            }
            var remainLength = step;
            while (remainLength + positionOnSegment > length)
            {
                remainLength -= length - positionOnSegment;
                var previousDelta = delta;
                if (!MoveNextPoint())
                {
                    hasReachedEnd = true;
                    if (!TerrainPoint.Equals(position,previousPoint))
                    {
                        previousPosition = position;
                        position = previousPoint;
                        return true;
                    }
                    return false;
                }
                if (KeepRightAngles)
                {
                    var angle = Math.Abs(Math.Abs(Math.Acos(Vector2.Dot(Vector2.Normalize(delta), Vector2.Normalize(previousDelta)))) - (MathF.PI/2)); 
                    if ( angle < 0.1d )
                    {
                        previousPosition = position;
                        position = previousPoint;
                        positionOnSegment = 0;
                        IsAfterRightAngle = true;
                        return true;
                    }
                }
            }
            positionOnSegment += remainLength;
            previousPosition = position;
            position = new TerrainPoint(Vector2.Lerp(previousPoint.Vector, point.Vector, positionOnSegment / length));
            return true;
        }

    }
}
