using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GameRealisticMap.Geometries
{
    public static class BoxSideHelper
    {
        private static readonly BoxSide[] sides = new[] { BoxSide.Top, BoxSide.Right, BoxSide.Bottom, BoxSide.Left };

        public static BoxSide GetClosest(BoundingBox box, IEnumerable<TerrainPath> allPaths, float maxDistance)
        {
            return GetClosestList(box, allPaths.Select(path => (path, 1f)), maxDistance).FirstOrDefault();
        }

        public static IEnumerable<BoxSide> GetClosestList(BoundingBox box, IEnumerable<(TerrainPath,float)> allPaths, float maxDistance)
        {
            var envelope = box.WithMargin(maxDistance * 1.5f);

            var paths = allPaths.Where(p => p.Item1.EnveloppeIntersects(envelope)).ToList();

            if ( paths.Count == 0 )
            {
                return Enumerable.Empty<BoxSide>();
            }

            var sidesDistance = GetSidesPoints(box).Select(s => paths.Min(p => p.Item1.Distance(s) * p.Item2)).ToList();

            return sides
                .Select((s, i) => new { Side = s, Distance = sidesDistance[i] })
                .Where(i => i.Distance < maxDistance)
                .OrderBy(i => i.Distance)
                .Select(i => i.Side);
        }

        public static BoxSide GetFurthest(BoundingBox box, IEnumerable<TerrainPath> allPaths, float minDistance)
        {
            return GetFurthestList(box, allPaths, minDistance).FirstOrDefault();
        }

        public static IEnumerable<BoxSide> GetFurthestList(BoundingBox box, IEnumerable<TerrainPath> allPaths, float minDistance)
        {
            var envelope = box.WithMargin(minDistance * 10f);

            var paths = allPaths.Where(p => p.EnveloppeIntersects(envelope));

            return GetFurthestListImpl(box, minDistance, paths.ToList());
        }

        public static IEnumerable<BoxSide> GetFurthestList(BoundingBox box, IEnumerable<TerrainPolygon> allPaths, float minDistance)
        {
            var envelope = box.WithMargin(minDistance * 10f);

            var paths = allPaths.Where(p => p.EnveloppeIntersects(envelope));

            return GetFurthestListImpl(box, minDistance, paths.Select(b => new TerrainPath(b.Shell)).ToList());
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
