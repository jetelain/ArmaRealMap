using System.Numerics;

namespace GameRealisticMap.Geometries
{
    public static class BoxSideHelper
    {
        private static readonly BoxSide[] sides = new[] { BoxSide.Top, BoxSide.Right, BoxSide.Bottom, BoxSide.Left };

        public static BoxSide GetClosest(BoundingBox box, TerrainSpacialIndex<TerrainPath> all, float maxDistance)
        {
            return GetClosestList(box, all, maxDistance, p=> p, p =>1).FirstOrDefault();
        }

        public static IEnumerable<BoxSide> GetClosestList<T>(BoundingBox box, TerrainSpacialIndex<T> all, float maxDistance, Func<T,TerrainPath> path, Func<T,float> factor)
            where T : class, ITerrainEnvelope
        {
            var envelope = box.WithMargin(maxDistance * 1.5f);

            var paths = all.Where(envelope, p => path(p).EnveloppeIntersects(envelope)).ToList();

            if (paths.Count == 0)
            {
                return Enumerable.Empty<BoxSide>();
            }

            var sidesDistance = GetSidesPoints(box).Select(s => paths.Min(p => path(p).Distance(s) * factor(p))).ToList();

            return sides
                .Select((s, i) => new { Side = s, Distance = sidesDistance[i] })
                .Where(i => i.Distance < maxDistance)
                .OrderBy(i => i.Distance)
                .Select(i => i.Side);
        }

        public static BoxSide GetFurthest(BoundingBox box, TerrainSpacialIndex<TerrainPath> allPaths, float minDistance)
        {
            return GetFurthestList(box, allPaths, minDistance, p => p, p => true).FirstOrDefault();
        }

        public static IEnumerable<BoxSide> GetFurthestList<T>(BoundingBox box, TerrainSpacialIndex<T> allPaths, float minDistance, Func<T,TerrainPath> getPath, Func<T,bool> filter)
             where T : class, ITerrainEnvelope
        {
            var envelope = box.WithMargin(minDistance * 10f);

            var paths = allPaths.Where(envelope, p => p.EnveloppeIntersects(envelope) && filter(p));

            return GetFurthestListImpl(box, minDistance, paths.Select(b => getPath(b)).ToList());
        }

        private static IEnumerable<BoxSide> GetFurthestListImpl(BoundingBox box, float minDistance, List<TerrainPath> paths)
        {
            if (paths.Count == 0)
            {
                return sides; // All
            }

            var sidesDistance = GetSidesPoints(box).Select(s => paths.Min(p => p.Distance(s))).ToList();

            return sides
                .Select((s, i) => new { Side = s, Distance = sidesDistance[i] })
                .Where(i => i.Distance >= minDistance)
                .OrderByDescending(i => i.Distance)
                .Select(i => i.Side);
        }

        private static TerrainPoint[] GetSidesPoints(BoundingBox box)
        {
            var rotate = Matrix3x2.CreateRotation(box.Angle * MathF.PI / 180f);
            return new[]
            {
                box.Center + Vector2.Transform(new Vector2(0, +box.Height/2), rotate),
                box.Center + Vector2.Transform(new Vector2(+box.Width/2, 0), rotate),
                box.Center + Vector2.Transform(new Vector2(0, -box.Height/2), rotate),
                box.Center + Vector2.Transform(new Vector2(-box.Width/2, 0), rotate)
            };
        }

        /*
        private static TerrainPoint GetPoint(BoundingBox box, Matrix3x2 rotate, BoxSide side)
        {
            switch (side)
            {
                case BoxSide.Top:
                    return box.Center + Vector2.Transform(new Vector2(0, +box.Height / 2), rotate);
                case BoxSide.Right:
                    return box.Center + Vector2.Transform(new Vector2(+box.Width / 2, 0), rotate);
                case BoxSide.Bottom:
                    return box.Center + Vector2.Transform(new Vector2(0, -box.Height / 2), rotate);
                case BoxSide.Left:
                    return box.Center + Vector2.Transform(new Vector2(-box.Width / 2, 0), rotate);
            }
            throw new ArgumentException();
        }
        */

        public static TerrainPath GetSide(BoundingBox box, BoxSide side)
        {
            var rotate = Matrix3x2.CreateRotation(box.Angle * MathF.PI / 180f);
            switch (side)
            {
                case BoxSide.Top:
                    return new TerrainPath( 
                        box.Center + Vector2.Transform(new Vector2(-box.Width/2, +box.Height/2), rotate) ,
                        box.Center + Vector2.Transform(new Vector2(+box.Width/2, +box.Height/2), rotate));
                case BoxSide.Right:
                    return new TerrainPath(
                        box.Center + Vector2.Transform(new Vector2(+box.Width / 2, +box.Height / 2), rotate),
                        box.Center + Vector2.Transform(new Vector2(+box.Width / 2, -box.Height / 2), rotate));
                case BoxSide.Bottom:
                    return new TerrainPath(
                        box.Center + Vector2.Transform(new Vector2(+box.Width / 2, -box.Height / 2), rotate),
                        box.Center + Vector2.Transform(new Vector2(-box.Width / 2, -box.Height / 2), rotate));
                case BoxSide.Left:
                    return new TerrainPath(
                        box.Center + Vector2.Transform(new Vector2(-box.Width / 2, -box.Height / 2), rotate),
                        box.Center + Vector2.Transform(new Vector2(-box.Width / 2, +box.Height / 2), rotate));
            }
            throw new ArgumentException();
        }
    }
}
