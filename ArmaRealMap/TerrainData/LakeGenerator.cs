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
using SixLabors.ImageSharp.Processing;

namespace ArmaRealMap
{
    internal static class LakeGenerator
    {
        internal static void ProcessLakes(MapData data, MapInfos area, List<OsmShape> shapes)
        {
            var initialLakes = shapes.Where(s => s.Category == OsmShapeCategory.Lake).SelectMany(s => s.TerrainPolygons).ToList();

            var objects = new List<TerrainObject>();

            var minimalArea = Math.Pow(5 * data.Config.CellSize, 2); // 5 x 5 nodes minimum

            var embankments = data.RoadsRaw.Where(r => r.SpecialSegment == Roads.RoadSpecialSegment.Embankment).SelectMany(s => s.Path.ToTerrainPolygon(s.Width * 2f )).ToList();

            var lakes = initialLakes.SelectMany(l => l.SubstractAll(embankments)).ToList();

            var report = new ProgressReport("Lakes", lakes.Count);
            var cellSize = new Vector2(area.CellSize, area.CellSize);
            foreach (var g in lakes)
            {
                if (g.Area < minimalArea)
                {
                    continue; // too small
                }

                var points = g.Shell;
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
                lakeElevation.Image.Mutate(d =>
                {
                    DrawHelper.DrawPolygon(d, g, new SolidBrush(Color.FromRgba(255, 255, 255, 128)),  lakeElevation.ToPixels);
                    foreach (var scaled in g.Offset(-10))
                    {
                        DrawHelper.DrawPolygon(d, scaled, new SolidBrush(Color.FromRgba(128, 128, 128, 192)), lakeElevation.ToPixels);
                    }
                    foreach (var scaled in g.Offset(-20))
                    {
                        DrawHelper.DrawPolygon(d, scaled, new SolidBrush(Color.FromRgba(0, 0, 0, 255)), lakeElevation.ToPixels);
                    }
                });
                lakeElevation.Apply();
                GenerateTiles(area, objects, g, ajustedWaterElevation, min, max);
                report.ReportOneDone();
            }
            report.TaskDone();

            using (var writer = new StreamWriter(new FileStream(data.Config.Target.GetTerrain("watertiles.txt"), FileMode.Create, FileAccess.Write)))
            {
                foreach (var obj in objects)
                {
                    writer.WriteLine(obj.ToTerrainBuilderCSV());
                }
            }

            data.Elevation.SaveToAsc(data.Config.Target.GetTerrain("elevation-lakes.asc"));
            //data.Elevation.SavePreview(data.Config.Target.GetDebug("elevation-lakes.png"));
        }

        private static void GenerateTiles(MapInfos area, List<TerrainObject> objects, TerrainPolygon g, float ajustedWaterElevation, TerrainPoint min, TerrainPoint max)
        {
            var w10 = (int)Math.Ceiling((max.X - min.X) / 10);
            var h10 = (int)Math.Ceiling((max.Y - min.Y) / 10);
            var grid10 = new bool[w10, h10];

            for (int x = 0; x < w10; ++x)
            {
                for (int y = 0; y < h10; ++y)
                {
                    var p1 = new TerrainPoint((x * 10) + min.X, (y * 10) + min.Y);
                    var p2 = new TerrainPoint(p1.X + 10, p1.Y);
                    var p3 = new TerrainPoint(p1.X, p1.Y + 10);
                    var p4 = new TerrainPoint(p1.X + 10, p1.Y + 10);
                    if (g.Contains(p1) || g.Contains(p2) || g.Contains(p3) || g.Contains(p4))
                    {
                        if (area.IsInside(p1))
                        {
                            grid10[x, y] = true;
                        }
                    }
                }
            }

            Process(objects, grid10, w10, h10, min, 80, ajustedWaterElevation);
            Process(objects, grid10, w10, h10, min, 40, ajustedWaterElevation);
            Process(objects, grid10, w10, h10, min, 20, ajustedWaterElevation);
            Process(objects, grid10, w10, h10, min, 10, ajustedWaterElevation);
        }

        private static void Process(List<TerrainObject> objects, bool[,] grid10, int w10, int h10, TerrainPoint min, int size, float ajustedWaterElevation)
        {
            var obj = Pond(size);
            var objSize = size / 10;
            var hsize = size / 2;
            for (int x = 0; x < w10; x += objSize)
            {
                for (int y = 0; y < h10; y += objSize)
                {
                    if (x + objSize <= w10 && y + objSize < h10 && Match(grid10, x, y, x + objSize, y + objSize))
                    {
                        Take(grid10, x, y, x + objSize, y + objSize);
                        var ax = (x * 10) + hsize;
                        var ay = (y * 10) + hsize;
                        objects.Add(new TerrainObject(obj, new TerrainPoint(min.X + ax, min.Y + ay), 0.0f, ajustedWaterElevation));
                    }
                }
            }
        }

        private static void Take(bool[,] grid10, int minX, int minY, int maxX, int maxY)
        {
            for (int x = minX; x < maxX; ++x)
            {
                for (int y = minY; y < maxY; ++y)
                {
                    grid10[x, y] = false;
                }
            }
        }

        private static bool Match(bool[,] grid10, int minX, int minY, int maxX, int maxY)
        {
            for (int x = minX; x < maxX; ++x)
            {
                for (int y = minY; y < maxY; ++y)
                {
                    if ( !grid10[x, y] )
                    {
                        return false;
                    }
                }
            }
            return true;
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
    }
}
