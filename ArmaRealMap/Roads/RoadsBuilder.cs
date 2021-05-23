using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using ArmaRealMap.ElevationModel;
using ArmaRealMap.Geometries;
using ArmaRealMap.GroundTextureDetails;
using ArmaRealMap.Osm;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using OsmSharp;
using OsmSharp.Complete;
using OsmSharp.Db;
using OsmSharp.Geo;
using OsmSharp.Streams;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace ArmaRealMap.Roads
{
    internal static class RoadsBuilder
    {
        internal static void Roads(MapData data, OsmStreamSource filtered, SnapshotDb db, Config config)
        {
            PrepareRoads(data, filtered, db);

            SaveRoadsShp(data, config);

            AdjustElevationGrid(data);

            PreviewRoads(data);
        }

        private static void AdjustElevationGrid(MapData data)
        {
            var sortedRoads = data.Roads.Where(r => r.RoadType < RoadType.SingleLaneDirtRoad)/*.OrderByDescending(r => (int)r.RoadType)*/.ToList();
            var report = new ProgressReport("ElevationRoads", sortedRoads.Count );
            var gridConstraints = new int[data.MapInfos.Size, data.MapInfos.Size];
            var rotate1 = Matrix3x2.CreateRotation(1.570796f);
            var rotate2 = Matrix3x2.CreateRotation(-1.570796f);
            var margin = new Vector2(15);

            foreach (var road in sortedRoads)
            {
                var points = GeometryHelper.PointsOnPath(road.Path.Points, 5f).ToList();

                var segments = new List<RoadSegment>(); //points.Take(points.Count - 1).Zip(points.Skip(1));

                foreach (var segment in points.Take(points.Count - 1).Zip(points.Skip(1)))
                {
                    var delta = segment.Second.Vector - segment.First.Vector;
                    var vector = Vector2.Normalize(delta);
                    var vector1 = Vector2.Transform(vector, rotate1);
                    var vector2 = Vector2.Transform(vector, rotate2);
                    var center = new TerrainPoint(Vector2.Lerp(segment.First.Vector, segment.Second.Vector, 0.5f));
                    var a = center + vector1 * road.Width;
                    var b = center + vector2 * road.Width;
                    var elevationA = data.Elevation.ElevationAt(a);
                    var elevationB = data.Elevation.ElevationAt(b);
                    segments.Add(new RoadSegment()
                    {
                        Length = delta.Length(),
                        First = segment.First,
                        Second = segment.Second,
                        Center = center,
                        A = a,
                        B = b,
                        Elevation = data.Elevation.ElevationAt(center),
                        Adjust = MathF.Abs(elevationA - elevationB) > 0.75f
                    });
                }

                if (segments.Any(s => s.Adjust))
                {
                    var medium = new float[segments.Count];
                    for(int i = 0; i < segments.Count; ++i)
                    {
                        medium[i] = segments.Skip(Math.Max(0, i - 2)).Take(5).Average(s => s.Elevation);
                    }
                    for (int i = 0; i < segments.Count; ++i)
                    {
                        segments[i].Elevation = medium[i];
                    }

                    foreach (var segment in segments.Where(s => s.Adjust))
                    {
                        var x1 = MathF.Min(segment.A.X, segment.B.X);
                        var x2 = MathF.Max(segment.A.X, segment.B.X);
                        var y1 = MathF.Min(segment.A.Y, segment.B.Y);
                        var y2 = MathF.Max(segment.A.Y, segment.B.Y);

                        var x = data.Elevation.PrepareToMutate(new TerrainPoint(x1, y1)-margin, new TerrainPoint(x2, y2)+margin, segment.Elevation - 10, segment.Elevation + 10);
                        x.Image.Mutate(p =>
                        {
                            var pixelLength = ((Vector2)(x.ToPixel(segment.First) - x.ToPixel(segment.Second))).Length();
                            p.DrawLines(x.ElevationToColor(segment.Elevation).WithAlpha(0.5f), pixelLength * 2.5f, x.ToPixel(segment.A), x.ToPixel(segment.B));
                            p.DrawLines(x.ElevationToColor(segment.Elevation), pixelLength * 1.5f, x.ToPixel(segment.A), x.ToPixel(segment.B));
                        });
                        x.Apply();
                    }
                }
                report.ReportOneDone();
            }
            
            report.TaskDone();

            data.Elevation.SaveToAsc("elevation-roads.asc");
            data.Elevation.SavePreview("elevation-roads.bmp");
        }

        private static void Pass(MapData data, Image<Rgba32> img, Road road, IEnumerable<TerrainPoint> allPoints, ElevationGridArea b, float coef)
        {
            img.Mutate(p =>
            {
                var projected =
                    b.ToPixels(allPoints)
                    .Zip(allPoints.Select(p => data.Elevation.ElevationAt(p)).Select(p => b.ElevationToColor(p))).ToList();
                var previous = projected.First();
                foreach (var point in projected.Skip(1))
                {
                    var c1 = previous.Second;
                    var c2 = point.Second;
                    var brush = new LinearGradientBrush(previous.First, point.First, GradientRepetitionMode.None, new[] { new ColorStop(0, c1), new ColorStop(1, c2) });
                    p.DrawLines(brush, road.Width * coef / data.MapInfos.CellSize, previous.First, point.First);
                    previous = point;
                }
            });
        }

        private static void SaveRoadsShp(MapData data, Config config)
        {
            var features = new List<Feature>();
            foreach (var road in data.Roads)
            {
                var attributesTable = new AttributesTable();
                attributesTable.Add("ID", (int)road.RoadType);
                // Why x+200000 ? nobody really knows...
                features.Add(new Feature(road.Path.ToLineString(p => new Coordinate(p.X + 200000, p.Y)), attributesTable));
            }
            var header = ShapefileDataWriter.GetHeader(features.First(), features.Count);
            var shapeWriter = new ShapefileDataWriter(Path.Combine(config.Target?.Roads ?? string.Empty, "roads.shp"), new GeometryFactory())
            {
                Header = header
            };
            shapeWriter.Write(features);
        }

        private static void PreviewRoads(MapData data)
        {
            var report = new ProgressReport("PreviewRoads", data.Roads.Count);
            using (var img = new Image<Rgb24>(data.MapInfos.Width, data.MapInfos.Height, TerrainMaterial.GrassShort.Color))
            {
                img.Mutate(d =>
                {
                    var brush = new SolidBrush(OsmShapeCategory.Road.GroundTextureColorCode);
                    foreach (var road in data.Roads)
                    {
                        DrawHelper.DrawPath(d, road.Path, road.Width, brush, data.MapInfos);
                        report.ReportOneDone();
                    }
                });
                report.TaskDone();
                Console.WriteLine("SaveBMP");
                img.Save("roads.bmp");
            }
        }

        private static void PrepareRoads(MapData data, OsmStreamSource filtered, SnapshotDb db)
        {
            var interpret = new DefaultFeatureInterpreter2();
            var osmRoads = filtered.Where(o => o.Type == OsmGeoType.Way && o.Tags.ContainsKey("highway")).ToList();
            var area = TerrainPolygon.FromRectangle(data.MapInfos.P1, data.MapInfos.P2);
            data.Roads = new List<Road>();
            var report = new ProgressReport("PrepareRoads", osmRoads.Count);
            foreach (var road in osmRoads)
            {
                var kind = OsmCategorizer.ToRoadType(road.Tags);
                if (kind != null)
                {
                    var complete = road.CreateComplete(db);
                    var count = 0;
                    foreach (var feature in interpret.Interpret(complete))
                    {
                        foreach (var path in TerrainPath.FromGeometry(feature.Geometry, data.MapInfos.LatLngToTerrainPoint))
                        {
                            if (path.Length >= 3)
                            {
                                foreach (var pathSegment in path.ClippedBy(area))
                                {
                                    data.Roads.Add(new Road()
                                    {
                                        Path = pathSegment,
                                        RoadType = kind.Value
                                    });
                                }
                            }
                        }
                        count++;
                    }
                    if (count == 0)
                    {
                        Trace.TraceWarning($"NO GEOMETRY FOR {road.Tags}");
                    }

                }
                report.ReportOneDone();
            }
            report.TaskDone();
        }

        private class RoadSegment
        {
            internal bool Adjust;

            public float Length { get; set; }
            public TerrainPoint First { get; set; }
            public TerrainPoint Second { get; set; }
            public TerrainPoint Center { get; set; }
            public TerrainPoint A { get; set; }
            public TerrainPoint B { get; set; }
            public float Elevation { get; set; }
        }
    }
}
