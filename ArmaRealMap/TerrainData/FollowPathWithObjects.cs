using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using ArmaRealMap.Geometries;
using ArmaRealMap.Libraries;

namespace ArmaRealMap.TerrainData
{
    class FollowPathWithObjects
    {
        private static readonly Matrix3x2 rotate90 = Matrix3x2.CreateRotation(1.570796f); // +90°

        public static void PlaceOnPathRegular(Random rnd, ObjectLibrary lib, TerrainObjectLayer layer, List<TerrainPoint> path)
        {
            var width = lib.Objects.Select(t => t.GetPlacementRadius() * 2).Average();
            var points = GeometryHelper.PointsOnPathRegular(path, MathF.Floor(width));
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
            var points = GeometryHelper.PointsOnPathRegular(path, MathF.Floor(template.GetPlacementRadius() * 2));
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
        /*
        public static void PlaceOnPathRandomInside(Random rnd, ObjectLibrary lib, TerrainObjectLayer layer, IEnumerable<TerrainPoint> ring, float outsideTolerance = 1f)
        {
            var spacingFactor = (1 / (lib.Density ?? 0.8f)) - 1;

            var previous = ring.First();
            var previousObj = lib.GetObject(rnd);
            var remainLength = previousObj.GetPlacementRadius();
            layer.Insert(new TerrainObject(previousObj, previous, (float)rnd.NextDouble() * 360));
            foreach (var point in ring.Skip(1))
            {
                var delta = point.Vector - previous.Vector;
                var length = delta.Length();
                var normalDelta = Vector2.Transform(Vector2.Normalize(delta), rotate90);
                var positionOnSegment = remainLength;
                while (positionOnSegment <= length)
                {
                    var obj = lib.GetObject(rnd);
                    var objPoint = new TerrainPoint(Vector2.Lerp(previous.Vector, point.Vector, positionOnSegment / length));
                    if (obj.GetPlacementRadius() > outsideTolerance)
                    {
                        var dist = (obj.GetPlacementRadius() - outsideTolerance);
                        objPoint = objPoint + (normalDelta * (dist + obj.GetPlacementRadius() * (float)rnd.NextDouble()));
                    }
                    if (layer.MapInfos.IsInside(objPoint))
                    {
                        layer.Insert(new TerrainObject(obj, objPoint, (float)rnd.NextDouble() * 360));
                    }
                    var minimalDelta = (obj.GetPlacementRadius() + previousObj.GetPlacementRadius());
                    positionOnSegment += minimalDelta + (float)(rnd.NextDouble() * minimalDelta * spacingFactor);
                    previousObj = obj;
                }
                remainLength = positionOnSegment - length;
                previous = point;
            }
        }*/

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



            /*
            var previous = ring.First();
            SingleObjetInfos previousObj = null; //lib.GetObject(rnd);
            var remainLength = 0f; //previousObj.GetPlacementRadius();
            //layer.Insert(new TerrainObject(previousObj, previous, (float)rnd.NextDouble() * 360));
            foreach (var point in ring.Skip(1))
            {
                var delta = point.Vector - previous.Vector;
                var length = delta.Length();
                var normalDelta = Vector2.Transform(Vector2.Normalize(delta), rotate90);
                var positionOnSegment = remainLength;
                while (positionOnSegment <= length)
                {
                    var obj = lib.GetObject(rnd);
                    var minimalDelta = obj.GetPlacementRadius() + (previousObj?.GetPlacementRadius() ?? 0f);
                    positionOnSegment += minimalDelta + (float)(rnd.NextDouble() * minimalDelta * spacingFactor);
                    var objPoint = new TerrainPoint(Vector2.Lerp(previous.Vector, point.Vector, positionOnSegment / length));
                    if (obj.GetPlacementRadius() > outsideTolerance)
                    {
                        var dist = obj.GetPlacementRadius() - outsideTolerance;
                        objPoint = objPoint + (normalDelta * (dist + obj.GetPlacementRadius() * (float)rnd.NextDouble()));
                    }
                    if (layer.MapInfos.IsInside(objPoint))
                    {
                        layer.Insert(new TerrainObject(obj, objPoint, (float)rnd.NextDouble() * 360));
                    }
                    previousObj = obj;
                }
                remainLength = positionOnSegment - length;
                previous = point;
            }*/
        }

    }
}
