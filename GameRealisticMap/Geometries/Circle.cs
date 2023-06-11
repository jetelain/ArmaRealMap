using System.Numerics;

namespace GameRealisticMap.Geometries
{
    public sealed class Circle
    {
        public Vector2 Center { get; }

        public float Radius { get; }

        public Circle(Vector2 center, float radius)
        {
            Center = center;
            Radius = radius;
        }

        public bool Contains(Vector2 point)
        {
            return (Center - point).Length() <= Radius;
        }

        public static Circle FromTwoPoints(Vector2 a, Vector2 b)
        {
            return new Circle((a + b) / 2, (b - a).Length() / 2);
        }

        public static Circle FromThreePoints(Vector2 a, Vector2 b, Vector2 c)
        {
            var o = (Vector2.Min(Vector2.Min(a, b), c) + Vector2.Max(Vector2.Max(a, b), c)) / 2;
            var da = a - o;
            var db = b - o;
            var dc = c - o;
            var d = (da.X * (db.Y - dc.Y) + db.X * (dc.Y - da.Y) + dc.X * (da.Y - db.Y)) * 2;
            if (d == 0)
            {
                // var ab = (a - b).Length();
                // var bc = (b - c).Length();
                // var ac = (a - c).Length();
                // XXX: Fallback to FromTwoPoints ?
                return new Circle(Vector2.Zero, 0f);
            }
            var x = ((da.X * da.X + da.Y * da.Y) * (db.Y - dc.Y) + (db.X * db.X + db.Y * db.Y) * (dc.Y - da.Y) + (dc.X * dc.X + dc.Y * dc.Y) * (da.Y - db.Y)) / d;
            var y = ((da.X * da.X + da.Y * da.Y) * (dc.X - db.X) + (db.X * db.X + db.Y * db.Y) * (da.X - dc.X) + (dc.X * dc.X + dc.Y * dc.Y) * (db.X - da.X)) / d;
            var p = o + new Vector2(x, y);
            var sqR = MathF.Max((p - a).LengthSquared(), MathF.Max((p - b).LengthSquared(), (p - c).LengthSquared()));
            return new Circle(p, MathF.Sqrt(sqR));
        }

        private static Circle Create(IList<Vector2> _positions)
        {
            switch (_positions.Count)
            {
                case 0:
                    return new Circle(Vector2.Zero, 0f);
                case 1:
                    return new Circle(_positions[0], 0f);
                case 2:
                    return FromTwoPoints(_positions[0], _positions[1]);
                case 3:
                    return FromThreePoints(_positions[0], _positions[1], _positions[2]);
                default:
                    throw new ArgumentException();
            }
        }

        public static Circle CreateFromWelzl(IEnumerable<Vector2> points)
        {
            var shuffle = points.OrderBy(_ => Random.Shared.NextDouble()).ToList();
            return CreateFromWelzl(shuffle, new List<Vector2>());
        }

        public static Circle CreateFromWelzlStable(IEnumerable<Vector2> points)
        {
            return CreateFromWelzl(points.ToList(), new List<Vector2>());
        }

        private static Circle CreateFromWelzl(List<Vector2> points, List<Vector2> boundary)
        {
            if (points.Count == 0 || boundary.Count == 3)
            {
                return Create(boundary);
            }

            var lastIndex = points.Count - 1;
            var removed = points[lastIndex];
            points.RemoveAt(lastIndex);

            var circle = CreateFromWelzl(points, boundary);
            if (!circle.Contains(removed))
            {
                boundary.Add(removed);
                circle = CreateFromWelzl(points, boundary);
                boundary.RemoveAt(boundary.Count - 1);
            }

            points.Add(removed);
            return circle;
        }

        public static Circle CreateFromRitter(List<Vector2> points)
        {
            return CreateFromRitter(points, points[Random.Shared.Next(0, points.Count)]);
        }

        public static Circle CreateFromRitterStable(IEnumerable<Vector2> points)
        {
            return CreateFromRitter(points, points.OrderBy(e => e.LengthSquared()).First());
        }

        private static Circle CreateFromRitter(IEnumerable<Vector2> points, Vector2 p)
        {
            var othersByDistance = points.Where(e => e != p).OrderByDescending(e => (e - p).LengthSquared()).ToList();
            var q = othersByDistance[0];
            var c = FromTwoPoints(p, q);
            foreach (var e in othersByDistance.Skip(1))
            {
                if (!c.Contains(e))
                {
                    c = new Circle(c.Center, (e - c.Center).Length());
                }
            }
            return c;
        }

        public static Circle CreateFrom(List<Vector2> points, int attempts = 10)
        {
            bool canWelzl = points.Count < 4000; // May stackoverflow otherwise
            var results = new List<Circle>((attempts * 2) + 2);
            results.Add(CreateFromRitterStable(points));
            if (canWelzl)
            {
                results.Add(CreateFromWelzlStable(points));
            }
            for (int i = 0; i < attempts; ++i)
            {
                results.Add(CreateFromRitter(points));
                if (canWelzl)
                {
                    results.Add(CreateFromWelzl(points));
                }
            }
            return results.Where(c => c.Radius > 0).OrderBy(c => c.Radius).First();
        }



    }
}
