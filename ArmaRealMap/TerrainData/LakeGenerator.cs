using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using ArmaRealMap.Geometries;
using ArmaRealMap.Libraries;
using ArmaRealMap.Osm;
using NetTopologySuite.Geometries;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;

namespace ArmaRealMap
{
    internal static class LakeGenerator
    {
        private static bool IsPointInPolygon(NetTopologySuite.Geometries.Polygon shape, Coordinate point)
        {
            return GeometryHelper.PointInPolygon(shape.ExteriorRing.Coordinates, point) != 0;
        }

        internal static void ProcessLakes(MapData data, MapInfos area, List<OsmShape> shapes)
        {
            var lakes = shapes.Where(s => s.Category == OsmShapeCategory.Lake).ToList();

            var objects = new List<TerrainObject>();

            var waterTile20 = Pond(20);

            //var stats = new StringBuilder("Width\tHeight\r\n");
            var report = new ProgressReport("Lakes", lakes.Count);
            var cellSize = new Vector2(area.CellSize, area.CellSize);
            foreach (var item in lakes)
            {
                var geo = GeometryHelper.LatLngToTerrainPolygon(area, item.Geometry).ToList();
                if (geo.Count > 0)
                {
                    foreach (var g in geo)
                    {
                        if (g.Area < 600)
                        {
                            continue; // too small
                        }

                        var points = g.ExteriorRing.Coordinates.Select(g => new TerrainPoint((float)g.X, (float)g.Y)).ToList();

                        //var box = BoundingBox.Compute(points.ToArray());
                        var waterElevation = points.Min(p => data.Elevation.ElevationAt(p));
                        var ajustedWaterElevation = waterElevation - 0.1f;

                        var min = new TerrainPoint(
                            points.Min(p => p.X),
                            points.Min(p => p.Y));
                        var max = new TerrainPoint(
                            points.Max(p => p.X),
                            points.Max(p => p.Y));
                        
                        
                        var lakeElevation = data.Elevation.PrepareToMutate(min - cellSize, max + cellSize, waterElevation - 2.5f, waterElevation);
                        DrawHelper.FillGeometry(lakeElevation.Image, new SolidBrush(Color.FromRgba(255, 255, 255, 128)), g, lakeElevation.ToPixels);
                        foreach (var scaled in GeometryHelper.Offset(g, -10))
                        {
                            DrawHelper.FillGeometry(lakeElevation.Image, new SolidBrush(Color.FromRgba(128, 128, 128, 192)), scaled, lakeElevation.ToPixels);
                        }
                        foreach (var scaled in GeometryHelper.Offset(g, -20))
                        {
                            DrawHelper.FillGeometry(lakeElevation.Image, new SolidBrush(Color.FromRgba(0, 0, 0, 255)), scaled, lakeElevation.ToPixels);
                        }
                        lakeElevation.Apply();


                        for (float x = min.X; x < max.X; x += waterTile20.Width)
                        {
                            for (float y = min.Y; y < max.Y; y += waterTile20.Depth)
                            {
                                var p1 = new Coordinate(x, y);
                                var p2 = new Coordinate(x + waterTile20.Width, y);
                                var p3 = new Coordinate(x, y + waterTile20.Depth);
                                var p4 = new Coordinate(x + waterTile20.Width, y + waterTile20.Depth);

                                if (IsPointInPolygon(g, p1) || IsPointInPolygon(g, p2) || IsPointInPolygon(g, p3) || IsPointInPolygon(g, p4))
                                {
                                    if (area.IsInside(p1))
                                    {
                                        objects.Add(new TerrainObject(waterTile20, new TerrainPoint(x, y), 0.0f, ajustedWaterElevation));
                                    }
                                }
                            }
                        }


                    }
                }
                report.ReportOneDone();
            }
            report.TaskDone();

            using (var writer = new StreamWriter(new FileStream(data.Config.Target.GetTerrain("watertiles.txt"), FileMode.Create, FileAccess.Write)))
            {
                foreach (var obj in objects)
                {
                    writer.WriteLine(obj.ToString(area));
                }
            }

            data.Elevation.SaveToAsc(data.Config.Target.GetTerrain("elevation-lakes.asc"));
            data.Elevation.SavePreview(data.Config.Target.GetDebug("elevation-lakes.png"));
        }

        private static SingleObjetInfos Pond(int v)
        {
            return new SingleObjetInfos()
            {
                Name = "arm_pond_" + v + "_v2",
                Depth = v,
                Width = v,
                PlacementRadius = v / 2
            };
        }
    }
}
