using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArmaRealMap.Geometries;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace ArmaRealMap
{
    internal static class DebugHelper
    {
        //public static void Polygons(MapData data, string filename, params TerrainPolygon[] polygons)
        //{
        //    Polygons(data, filename, polygons.ToList());
        //}
        private static readonly Color[] Colors = new[] {
        Color.Red,
        Color.Green,
        Color.Blue,
        Color.Yellow,
        Color.Purple,
        Color.Cyan
        };

        public static void Polygons(MapData data, string filename, float scale, params TerrainPolygon[] polygons)
        {
            Polygons(data, filename, scale, polygons.ToList(), (_, idx) =>
            {
                if (idx == 0)
                {
                    return new Tuple<IBrush, IBrush>(null, new SolidBrush(Color.Gray));
                }
                return new Tuple<IBrush, IBrush>(new SolidBrush(Colors[(idx + 1) % Colors.Length]), null);
            });
        }
        public static void Polygons(MapData data, string filename, float scale, IEnumerable<TerrainPolygon> polygons)
        {
            Polygons(data, filename, scale, polygons.ToList(), (_, idx) =>
            {
                if (idx == 0)
                {
                    return new Tuple<IBrush, IBrush>(null, new SolidBrush(Color.Gray));
                }
                return new Tuple<IBrush, IBrush>(new SolidBrush(Colors[(idx + 1) % Colors.Length]), null);
            });
        }
        //public static void Polygons(MapData data, string filename, List<TerrainPolygon> polygons)
        //{
        //    Polygons(data, filename, 1f / (float)data.MapInfos.ImageryResolution, polygons);
        //}

        public static void Polygons(MapData data, string filename, float scale, List<TerrainPolygon> polygons, Func<TerrainPolygon, int,Tuple<IBrush,IBrush>> getBrush)
        {
            var min = new TerrainPoint(MathF.Floor(polygons.Min(p => p.MinPoint.X)), MathF.Floor(polygons.Min(p => p.MinPoint.Y)));
            var max = new TerrainPoint(MathF.Ceiling(polygons.Max(p => p.MaxPoint.X)), MathF.Ceiling(polygons.Max(p => p.MaxPoint.Y)));
            var size = max.Vector - min.Vector;
            Func<TerrainPoint, PointF> projectPoint = (p) => new PointF((p.X - min.X) * scale, (max.Y - p.Y) * scale);
            using (var img = new Image<Rgb24>((int)MathF.Round(size.X * scale), (int)MathF.Round(size.Y * scale), Color.Black))
            {
                img.Mutate(p =>
                {
                    var index = 0;
                    foreach (var polygon in polygons)
                    {
                        var brush = getBrush(polygon, index);
                        if (brush.Item2 != null)
                        {
                            DrawHelper.FillPolygonWithHoles(p,
                                polygon.Shell.Select(projectPoint),
                                polygon.Holes.Select(l => l.Select(projectPoint)).ToList(),
                                brush.Item2,
                                new DrawingOptions());
                        }
                        else
                        {
                            DrawHelper.DrawPolygonEdgesWithHoles(p,
                                polygon.Shell.Select(projectPoint),
                                polygon.Holes.Select(l => l.Select(projectPoint)).ToList(),
                                brush.Item1,
                                new DrawingOptions(), 
                                1f);
                        }
                        index++;
                    }
                });
                img.Save(System.IO.Path.Combine(data.Config.Target.Debug, filename));
            }
        }

        public static void Polygons(MapData data, List<FillArea> areas, string filename)
        {
            Polygons(data, areas.Select(a => a.Polygon), areas.Count, filename, Color.Green);
        }
        public static void Polygons(MapData data, List<TerrainPolygon> polygons, string filename)
        {
            Polygons(data, polygons, polygons.Count, filename, Color.Green);
        }

        public static void Polygons(MapData data, IEnumerable<TerrainPolygon> polygons, int count, string filename, Color areasColor)
        {
            using (var img = new Image<Rgb24>(data.MapInfos.ImageryWidth, data.MapInfos.ImageryHeight, Color.Black))
            {
                DrawPolygons(data.MapInfos, polygons, count, areasColor, img);
                img.Save(System.IO.Path.Combine(data.Config.Target.Debug, filename));
            }
        }

        private static void DrawPolygons(MapInfos area, IEnumerable<TerrainPolygon> polygons, int count, Color areasColor, Image<Rgb24> img)
        {
            var report = new ProgressReport("Shapes", count);
            img.Mutate(p =>
            {
                foreach (var item in polygons)
                {
                    DrawHelper.DrawPolygon(p, item, new SolidBrush(areasColor), area);
                    report.ReportOneDone();
                }
            });
            report.TaskDone();
        }

        public static void ObjectsInPolygons(MapData data, SimpleSpacialIndex<TerrainObject> objects, List<TerrainPolygon> areas, string filename)
        {
            ObjectsInPolygons(data, objects, objects.Count, areas, areas.Count, filename, Color.LightGreen, Color.DarkGreen);
        }
        public static void ObjectsInPolygons(MapData data, ICollection<TerrainObject> objects, List<TerrainPolygon> areas, string filename)
        {
            ObjectsInPolygons(data, objects, objects.Count, areas, areas.Count, filename, Color.LightGreen, Color.DarkGreen);
        }
        public static void ObjectsInPolygons(MapData data, SimpleSpacialIndex<TerrainObject> objects, List<FillArea> areas, string filename)
        {
            ObjectsInPolygons(data, objects, objects.Count, areas.Select(a => a.Polygon), areas.Count, filename, Color.LightGreen, Color.DarkGreen); 
        }

        public static void ObjectsInPolygons(MapData data, IEnumerable<TerrainObject> objects, int objectsCount, IEnumerable<TerrainPolygon> polygons, int polygonsCount, string filename, Color areasColor, Color objectsColors)
        {
            using (var img = new Image<Rgb24>(data.MapInfos.ImageryWidth, data.MapInfos.ImageryHeight, Color.Black))
            {
                DrawPolygons(data.MapInfos, polygons, polygonsCount, areasColor, img);
                img.Mutate(p =>
                {
                    var report = new ProgressReport("Objects", objectsCount);
                    foreach (var obj in objects)
                    {
                        p.Fill(objectsColors, new EllipsePolygon(data.MapInfos.TerrainToPixelsPoint(obj.Center), obj.Object.GetPlacementRadius()));
                        report.ReportOneDone();
                    }
                    report.TaskDone();
                });
                img.Save(System.IO.Path.Combine(data.Config.Target.Debug, filename));
            }
        }

    }
}
