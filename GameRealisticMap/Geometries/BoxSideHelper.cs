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
            var envelope = box.WithMargin(maxDistance * 1.5f);

            var path = allPaths.Where(p => p.EnveloppeIntersects(envelope));

            var sidesDistance = GetSidesPoints(box).Select(s => allPaths.Min(p => p.Distance(s))).ToList();

            var distance = maxDistance;
            var side = BoxSide.None;

            for (int i = 0; i < sides.Length; ++i)
            {
                if (sidesDistance[i] < distance)
                {
                    side = sides[i];
                    distance = sidesDistance[i];
                }
            }

            return side;
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
