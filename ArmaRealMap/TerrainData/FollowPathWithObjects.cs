using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using ArmaRealMap.Geometries;
using ArmaRealMap.Libraries;

namespace ArmaRealMap.TerrainData
{
    class FollowPathWithObjects
    {
        public static void PlaceOnPathRightAngle(ObjectLibrary lib, TerrainObjectLayer layer, List<TerrainPoint> path)
        {
            var max = lib.Objects.Max(t => t.GetPlacementRadius());
            var maxObj = lib.Objects.First(t => t.GetPlacementRadius() == max);
            var width = max * 2;

            var follow = new FollowPath(path);
            follow.KeepRightAngles = true;

            while (follow.Move(width))
            {
                var delta = Vector2.Normalize(follow.Current.Vector - follow.Previous.Vector);
                var angle = (180f + (MathF.Atan2(delta.Y, delta.X) * 180 / MathF.PI)) % 360f;

                var distance = (follow.Previous.Vector - follow.Current.Vector).Length();
                if (Math.Abs(distance - width) < 0.2f)
                {
                    // Almost fit !
                    var center = new TerrainPoint(Vector2.Lerp(follow.Previous.Vector, follow.Current.Vector, 0.5f));
                    layer.Insert(new TerrainObject(maxObj, center, angle));
                }
                else
                {
                    // Does not fit :'(
                    var wantedRadius = distance / 2;
                    var best = lib.Objects.OrderBy(o => o.GetPlacementRadius()).First(o => o.GetPlacementRadius() >= wantedRadius);
                    if (follow.IsLast || follow.IsAfterRightAngle)
                    {
                        // Adjust to Current (last in segment)
                        var center = follow.Current - (delta * best.GetPlacementRadius());
                        layer.Insert(new TerrainObject(best, center, angle));
                    }
                    else
                    {
                        // Adjust to Previous (first in segment)
                        var center = follow.Previous + (delta * best.GetPlacementRadius());
                        layer.Insert(new TerrainObject(best, center, angle));
                    }
                }
            }


            /*var points = GeometryHelper.PointsOnPathRegular(path, MathF.Floor(width));
            foreach (var segment in points.Take(points.Count - 1).Zip(points.Skip(1)))
            {
                var delta = Vector2.Normalize(segment.Second.Vector - segment.First.Vector);
                var center = new TerrainPoint(Vector2.Lerp(segment.First.Vector, segment.Second.Vector, 0.5f));
                var angle = (180f + (MathF.Atan2(delta.Y, delta.X) * 180 / MathF.PI)) % 360f;
                layer.Insert(new TerrainObject(lib.GetObject(rnd), center, angle));
            }*/
        }

        public static void PlaceOnPathRegular(Random rnd, ObjectLibrary lib, TerrainObjectLayer layer, List<TerrainPoint> path)
        {
            var width = lib.Objects.Select(t => t.GetPlacementRadius() * 2).Average();
            var points = GeometryHelper.PointsOnPathRegular(path, width);
            foreach (var segment in points.Take(points.Count - 1).Zip(points.Skip(1)))
            {
                var delta = Vector2.Normalize(segment.Second.Vector - segment.First.Vector);
                var center = new TerrainPoint(Vector2.Lerp(segment.First.Vector, segment.Second.Vector, 0.5f));
                var angle = (180f + (MathF.Atan2(delta.Y, delta.X) * 180 / MathF.PI)) % 360f;
                layer.Insert(new TerrainObject(lib.GetObject(rnd), center, angle));
            }
        }

        public static void PlaceOnPathRegular(SingleObjetInfos template, TerrainObjectLayer layer, List<TerrainPoint> path)
        {
            var points = GeometryHelper.PointsOnPathRegular(path, template.GetPlacementRadius() * 2);
            foreach (var segment in points.Take(points.Count - 1).Zip(points.Skip(1)))
            {
                var delta = Vector2.Normalize(segment.Second.Vector - segment.First.Vector);
                var center = new TerrainPoint(Vector2.Lerp(segment.First.Vector, segment.Second.Vector, 0.5f));
                var angle = (180f + (MathF.Atan2(delta.Y, delta.X) * 180 / MathF.PI)) % 360f;
                layer.Insert(new TerrainObject(template, center, angle));
            }
        }

        public static void PlaceOnEdgeRandomInside(Random rnd, ObjectLibrary lib, TerrainObjectLayer layer, TerrainPolygon polygon, float outsideTolerance = 1f)
        {
            PlaceOnPathRandomInside(rnd, lib, layer, polygon.Shell, outsideTolerance);
            foreach (var hole in polygon.Holes)
            {
                PlaceOnPathRandomInside(rnd, lib, layer, hole, outsideTolerance);
            }
        }

        public static void PlaceOnEdgeRandomOutside(Random rnd, ObjectLibrary lib, TerrainObjectLayer layer, TerrainPolygon polygon, float insideTolerance = 1f)
        {
            PlaceOnPathRandomOutside(rnd, lib, layer, polygon.Shell, insideTolerance);
            foreach (var hole in polygon.Holes)
            {
                PlaceOnPathRandomOutside(rnd, lib, layer, hole, insideTolerance);
            }
        }

        public static void PlaceOnPathRandomOutside(Random rnd, ObjectLibrary lib, TerrainObjectLayer layer, IEnumerable<TerrainPoint> ring, float insideTolerance = 1f)
        {
            PlaceOnPathRandomInside(rnd, lib, layer, ring.Reverse(), insideTolerance);
        }

        public static void PlaceOnPathRandomInside(Random rnd, ObjectLibrary lib, TerrainObjectLayer layer, IEnumerable<TerrainPoint> ring, float outsideTolerance = 1f)
        {
            var spacingFactor = (1 / (lib.Density ?? 0.8f)) - 1;

            var follow = new FollowPath(ring);

            SingleObjetInfos previousObj = null;

            while (true)
            {
                var obj = lib.GetObject(rnd);
                var minimalDelta = obj.GetPlacementRadius() + (previousObj?.GetPlacementRadius() ?? 0f);
                if (!follow.Move(minimalDelta + (float)(rnd.NextDouble() * minimalDelta * spacingFactor)))
                {
                    return;
                }
                var objPoint = follow.Current;
                if (obj.GetPlacementRadius() > outsideTolerance)
                {
                    var dist = obj.GetPlacementRadius() - outsideTolerance;
                    objPoint = objPoint + (follow.Vector90 * (dist + obj.GetPlacementRadius() * (float)rnd.NextDouble()));
                }
                if (layer.MapInfos.IsInside(objPoint))
                {
                    layer.Insert(new TerrainObject(obj, objPoint, (float)rnd.NextDouble() * 360));
                }
                previousObj = obj;
            }
        }

    }
}
