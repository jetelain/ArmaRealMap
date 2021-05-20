using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

            PreviewRoads(data);

            SaveRoadsShp(data, config);
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
                Console.WriteLine("SavePNG");
                img.Save("roads.png");
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
                var kind = OsmCategorizer.ToRoadType(road.Tags.GetValue("highway"));
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
    }
}
