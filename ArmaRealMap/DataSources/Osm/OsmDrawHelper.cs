using System;
using System.Linq;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace ArmaRealMap.Osm
{
    internal static class OsmDrawHelper
    {
        internal static void Draw(MapInfos mapInfos, Image<Rgb24> img, IBrush solidBrush, OsmShape shape, bool antiAlias = true)
        {
            DrawGeometry(mapInfos, img, solidBrush, shape.Geometry, (float)(6 / mapInfos.ImageryResolution), new DrawingOptions() { GraphicsOptions = new GraphicsOptions() { Antialias = antiAlias } });
            //DrawHelper.FillGeometry(img, solidBrush, shape.Geometry, mapInfos.LatLngToPixelsPoints);
        }

        private static void DrawGeometry(MapInfos mapInfos, Image<Rgb24> img, IBrush solidBrush, IGeometry geometry, float defaultWidth, DrawingOptions shapeGraphicsOptions)
        {
            if (geometry.OgcGeometryType == OgcGeometryType.MultiPolygon)
            {
                foreach (var geo in ((GeometryCollection)geometry).Geometries)
                {
                    DrawGeometry(mapInfos, img, solidBrush, geo, defaultWidth, shapeGraphicsOptions);
                }
            }
            else if (geometry.OgcGeometryType == OgcGeometryType.Polygon)
            {
                var poly = (Polygon)geometry;
                try
                {
                    DrawHelper.FillPolygonWithHoles(img,
                        mapInfos.LatLngToPixelsPoints(poly.Shell.Coordinates).ToList(),
                        poly.Holes.Select(h => mapInfos.LatLngToPixelsPoints(h.Coordinates).Select(p => new PointF(p.X, p.Y)).ToList()).ToList(),
                        solidBrush,
                        shapeGraphicsOptions);
                }
                catch
                {

                }
            }
            else if (geometry.OgcGeometryType == OgcGeometryType.LineString)
            {
                var line = (LineString)geometry;
                var points = mapInfos.LatLngToPixelsPoints(line.Coordinates).ToArray();
                try
                {
                    if (line.IsClosed)
                    {
                        img.Mutate(p => p.FillPolygon(shapeGraphicsOptions, solidBrush, points));
                    }
                    else
                    {
                        img.Mutate(p => p.DrawLines(shapeGraphicsOptions, solidBrush, defaultWidth, points));
                    }
                }
                catch
                {

                }
            }
            else
            {
                Console.WriteLine(geometry.OgcGeometryType);
            }
        }
    }
}
