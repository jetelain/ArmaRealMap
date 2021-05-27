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
            using (var img = new Image<Rgb24>(data.MapInfos.Width, data.MapInfos.Height, Color.Black))
            {
                DrawPolygons(data.MapInfos, polygons, count, areasColor, img);
                img.Save(data.Config.Target.GetDebug(filename));
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

        public static void ObjectsInPolygons(MapData data, SimpleSpacialIndex<TerrainObject> objects, List<FillArea> areas, string filename)
        {
            ObjectsInPolygons(data, objects, objects.Count, areas.Select(a => a.Polygon), areas.Count, filename, Color.LightGreen, Color.DarkGreen); 
        }

        public static void ObjectsInPolygons(MapData data, IEnumerable<TerrainObject> objects, int objectsCount, IEnumerable<TerrainPolygon> polygons, int polygonsCount, string filename, Color areasColor, Color objectsColors)
        {
            using (var img = new Image<Rgb24>(data.MapInfos.Width, data.MapInfos.Height, Color.Black))
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
                img.Save(data.Config.Target.GetDebug(filename));
            }
        }

    }
}
