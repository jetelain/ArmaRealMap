using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetTopologySuite.Geometries;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace ArmaRealMap.Osm
{
    internal static class OsmDrawHelper
    {
        internal static void Draw(MapInfos mapInfos, Image<Rgb24> img, IBrush solidBrush, OsmShape shape)
        {
            DrawGeometry(mapInfos, img, solidBrush, shape.Geometry);
        }

        private static void DrawGeometry(MapInfos mapInfos, Image<Rgb24> img, IBrush solidBrush, Geometry geometry)
        {
            if (geometry.OgcGeometryType == OgcGeometryType.MultiPolygon)
            {
                foreach (var geo in ((GeometryCollection)geometry).Geometries)
                {
                    DrawGeometry(mapInfos, img, solidBrush, geo);
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
                        solidBrush);
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
                        img.Mutate(p => p.FillPolygon(solidBrush, points));
                    }
                    else
                    {
                        img.Mutate(p => p.DrawLines(solidBrush, 6.0f, points));
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
