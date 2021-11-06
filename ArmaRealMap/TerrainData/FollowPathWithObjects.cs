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
        public static void PlaceOnPath(Random rnd, ObjectLibrary lib, TerrainObjectLayer layer, List<TerrainPoint> path)
        {
            var width = lib.Objects.Select(t => t.Width).Average();
            var points = GeometryHelper.PointsOnPathRegular(path, MathF.Floor(width));
            foreach (var segment in points.Take(points.Count - 1).Zip(points.Skip(1)))
            {
                var delta = Vector2.Normalize(segment.Second.Vector - segment.First.Vector);
                var center = new TerrainPoint(Vector2.Lerp(segment.First.Vector, segment.Second.Vector, 0.5f));
                var angle = (180f + (MathF.Atan2(delta.Y, delta.X) * 180 / MathF.PI)) % 360f;
                layer.Insert(new TerrainObject(lib.GetObject(rnd), center, angle));
            }
        }

        public static void PlaceOnPath(SingleObjetInfos template, TerrainObjectLayer layer, List<TerrainPoint> path)
        {
            var points = GeometryHelper.PointsOnPathRegular(path, MathF.Floor(template.Width));
            foreach (var segment in points.Take(points.Count - 1).Zip(points.Skip(1)))
            {
                var delta = Vector2.Normalize(segment.Second.Vector - segment.First.Vector);
                var center = new TerrainPoint(Vector2.Lerp(segment.First.Vector, segment.Second.Vector, 0.5f));
                var angle = (180f + (MathF.Atan2(delta.Y, delta.X) * 180 / MathF.PI)) % 360f;
                layer.Insert(new TerrainObject(template, center, angle));
            }
        }
    }
}
