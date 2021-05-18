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
    internal static class UnconclusiveLakeGenerator
    {
        private static bool IsPointInPolygon(NetTopologySuite.Geometries.Polygon shape, Coordinate point)
        {
            return GeometryHelper.PointInPolygon(shape.ExteriorRing.Coordinates, point) != 0;
        }

        private static void ProcessLakes(MapData data, MapInfos area, List<OsmShape> shapes)
        {
            var lakes = shapes.Where(s => s.Category == OsmShapeCategory.Lake).ToList();

            var objects = new List<TerrainObject>();


            var waterTiles = Enumerable.Range(3, 12).Select(i => Pond(i * 10)).ToList();
            var waterRounds = Enumerable.Range(3, 12).Select(i => PondRound(i * 10)).ToList();

            var waterTile20 = Pond(30);

            //var stats = new StringBuilder("Width\tHeight\r\n");
            int needBetter = 0;
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

                        bool isAuto = TrySingleTile(data, objects, waterTiles, waterRounds, points, ajustedWaterElevation);

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

                        if (!isAuto)
                        {
                            needBetter++;
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
                }
                report.ReportOneDone();
            }
            report.TaskDone();

            using (var writer = new StreamWriter(new FileStream("watertiles2.txt", FileMode.Create, FileAccess.Write)))
            {
                foreach (var obj in objects)
                {
                    writer.WriteLine(obj.ToString(area));
                }
            }

            data.Elevation.SaveToAsc("elevation-lakes2.asc");
            data.Elevation.SavePreviewToPng("elevation-lakes2.png");
        }

        private static bool TrySingleTile(MapData data, List<TerrainObject> objects, List<SingleObjetInfos> waterTiles, List<SingleObjetInfos> waterRounds, List<TerrainPoint> points, float ajustedWaterElevation)
        {
            var x1 = points.Min(p => p.X);
            var x2 = points.Max(p => p.X);
            var y1 = points.Min(p => p.Y);
            var y2 = points.Max(p => p.Y);

            var w = x2 - x1;
            var h = y2 - y1;

            var center = new Vector2((x1 + x2) / 2, (y1 + y2) / 2);

            var maxTile = waterTiles.Max(t => t.Width);

            var maxS = MathF.Max(w, h);
            var minS = MathF.Min(w, h);

            if (maxS / minS > 1.75)
            {
                var baseTile = waterTiles.Where(t => minS <= t.Width * 1.05).FirstOrDefault();
                if (baseTile != null)
                {
                    var tileCount = (int)MathF.Ceiling(maxS / baseTile.Width);
                    var dx = maxS == w ? 1 : 0;
                    var dy = maxS == h ? 1 : 0;
                    var asize = new Vector2(
                        baseTile.Size2D.X + (dx * (tileCount - 1) * baseTile.Size2D.X),
                        baseTile.Size2D.Y + (dy * (tileCount - 1) * baseTile.Size2D.Y));

                    var tilePoints = GeometryHelper.RotatedRectangleDegrees(center, asize, 0);
                    var minTileElevation = GeometryHelper.PointsOnPath(tilePoints.Select(p => new TerrainPoint(p))).Min(p => data.Elevation.ElevationAt(p));
                    if (minTileElevation > ajustedWaterElevation)
                    {
                        var deltaPos = new Vector2(dx * baseTile.Size2D.X, dy * baseTile.Size2D.Y);
                        var point = new Vector2(
                            center.X - ((dx * (tileCount - 1) * baseTile.Size2D.X) / 2),
                            center.Y - ((dy * (tileCount - 1) * baseTile.Size2D.Y) / 2));
                        for (int i = 0; i < tileCount; ++i)
                        {
                            objects.Add(new TerrainObject(baseTile, new BoundingBox(new TerrainPoint(point), baseTile.Width, baseTile.Depth, 0, GeometryHelper.RotatedRectangleDegrees(point, baseTile.Size2D, 0).Select(p => new TerrainPoint(p)).ToArray()), ajustedWaterElevation));
                            point += deltaPos;
                        }
                        return true;
                    }
                }
            }

            var rectangleTile = waterTiles.Where(t => w <= t.Width * 1.05 && h <= t.Width * 1.05).FirstOrDefault();
            if (rectangleTile != null)
            {
                var index = waterTiles.IndexOf(rectangleTile);
                var round = waterRounds[index];
                var roundPlus10 = index + 1 < waterRounds.Count ? waterRounds[index + 1] : null;

                var posList = new[]
                {
                    // Center
                    center,
                    // Bottom Left
                    new Vector2(x1 + (rectangleTile.Width / 2), y1 + (rectangleTile.Depth / 2)),
                    // Bottom Right
                    new Vector2(x1 + (rectangleTile.Width / 2), y2 - (rectangleTile.Depth / 2)),
                    // Top Right
                    new Vector2(x2 - (rectangleTile.Width / 2), y2 - (rectangleTile.Depth / 2)),
                    // Top Left
                    new Vector2(x1 + (rectangleTile.Width / 2), y2 - (rectangleTile.Depth / 2))
                };

                foreach (var pos in posList)
                {
                    if (TryRound(data, objects, round, pos, points, ajustedWaterElevation))
                    {
                        return true;
                    }
                    if (roundPlus10 != null && TryRound(data, objects, roundPlus10, pos, points, ajustedWaterElevation))
                    {
                        return true;
                    }
                    if (TryRectangle(data, objects, rectangleTile, pos, ajustedWaterElevation))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private static bool TryRectangle(MapData data, List<TerrainObject> objects, SingleObjetInfos tile, Vector2 center, float ajustedWaterElevation)
        {
            var tilePoints = GeometryHelper.RotatedRectangleDegrees(center, tile.Size2D, 0);
            return TryObject(data, objects, tile, center, ajustedWaterElevation, tilePoints);
        }

        private static bool TryRound(MapData data, List<TerrainObject> objects, SingleObjetInfos tile, Vector2 center, List<TerrainPoint> points, float ajustedWaterElevation)
        {
            var radius = tile.PlacementRadius.Value;
            var maxDistanceFromCenter = points.Select(c => (c.Vector - center).Length()).Max();
            if (maxDistanceFromCenter <= radius)
            {
                var tilePoints = GeometryHelper.SimpleCircle(center, radius);
                return TryObject(data, objects, tile, center, ajustedWaterElevation, tilePoints);
            }
            return false;
        }

        private static bool TryObject(MapData data, List<TerrainObject> objects, SingleObjetInfos tile, Vector2 center, float ajustedWaterElevation, Vector2[] tilePoints)
        {
            var minTileElevation = GeometryHelper.PointsOnPath(tilePoints.Select(p => new TerrainPoint(p))).Min(p => data.Elevation.ElevationAt(p));
            if (minTileElevation > ajustedWaterElevation)
            {
                objects.Add(new TerrainObject(tile,
                    new BoundingBox(new TerrainPoint(center), tile.Width, tile.Depth, 0, tilePoints.Select(p => new TerrainPoint(p)).ToArray()), ajustedWaterElevation));
                return true;
            }
            return false;
        }

        private static SingleObjetInfos Pond(int v)
        {
            return new SingleObjetInfos()
            {
                Name = "arm_pond_" + v,
                Depth = v,
                Width = v,
                PlacementRadius = v / 2
            };
        }
        private static SingleObjetInfos PondRound(int v)
        {
            return new SingleObjetInfos()
            {
                Name = "arm_pond_" + v + "_round",
                Depth = v,
                Width = v,
                PlacementRadius = v / 2
            };
        }
    }
}
