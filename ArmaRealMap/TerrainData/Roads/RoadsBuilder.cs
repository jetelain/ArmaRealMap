using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using ArmaRealMap.Core.ObjectLibraries;
using ArmaRealMap.ElevationModel;
using ArmaRealMap.Geometries;
using ArmaRealMap.GroundTextureDetails;
using ArmaRealMap.Libraries;
using ArmaRealMap.Osm;
using ArmaRealMap.TerrainData;
using ArmaRealMap.TerrainData.Forests;
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
        internal static void Roads(MapData data, OsmStreamSource filtered, SnapshotDb db, MapConfig config, ObjectLibraries libs, List<OsmShape> osmShapes, GenerateOptions options)
        {
            PrepareRoads(data, filtered, db);

            MergeRoads(data);

            if (!options.DoNotGenerateObjects)
            {
                RoadWalls(data, libs);

                SideWalks(data, libs, osmShapes);
            }

            var ignoreBridges = libs.Libraries.Any(l => l.Category == ObjectCategory.BridgePrimaryRoad);

            SaveRoadsShp(data, config, ignoreBridges);
        }

        private static void SideWalks(MapData data, ObjectLibraries libs, List<OsmShape> osmShapes)
        {
            var template = libs.Libraries.Where(l => l.Category == ObjectCategory.RoadSideWalk).SelectMany(l => l.Objects).FirstOrDefault();
            if (template != null)
            {
                var residentials = TerrainPolygon.MergeAll(osmShapes.Where(s => s.Category == OsmShapeCategory.Residential || s.Category == OsmShapeCategory.Retail || s.Category == OsmShapeCategory.Commercial).SelectMany(s => s.TerrainPolygons).ToList());
                data.UsedObjects.Add(template.Name);
                var layer = new TerrainObjectLayer(data.MapInfos);
                var report = new ProgressReport("SideWalks", residentials.Count);
                foreach (var residential in residentials)
                {
                    var areaRoads = data.Roads.Where(r => r.RoadType > RoadType.TwoLanesMotorway && r.RoadType < RoadType.SingleLaneDirtRoad && r.Path.EnveloppeIntersects(residential)).ToList();
                    var areaRoadsPolys = areaRoads.SelectMany(r => r.Path.ToTerrainPolygon(r.Width + template.Depth)).ToList();
                    var areaRoadsMerged = TerrainPolygon.MergeAll(areaRoadsPolys);
                    var areaRoadsClipped = areaRoadsMerged.SelectMany(r => r.Intersection(residential)).ToList();

                    foreach (var poly in areaRoadsClipped)
                    {
                        FollowPathWithObjects.PlaceOnPathRegular(template, layer, poly.Shell);
                        foreach (var hole in poly.Holes)
                        {
                            FollowPathWithObjects.PlaceOnPathRegular(template, layer, hole);
                        }
                    }
                    report.ReportOneDone();
                }
                report.TaskDone();
                NatureBuilder.Remove(layer, data.Roads.Where(r => r.RoadType < RoadType.SingleLaneDirtRoad), (road, tree) => tree.Poly.Centroid.Distance(road.Path.AsLineString) <= road.Width / 2);
                layer.WriteFile(Path.Combine(data.Config.Target.Objects, "sidewalks.rel.txt"));
            }
        }

        private static void RoadWalls(MapData data, ObjectLibraries libs)
        {
            var template = libs.Libraries.Where(l => l.Category == ObjectCategory.RoadConcreteWall).SelectMany(l => l.Objects).FirstOrDefault();
            if (template != null)
            {
                data.UsedObjects.Add(template.Name);
                var layer = new TerrainObjectLayer(data.MapInfos);

                var polys = data.Roads.Where(r => r.RoadType == RoadType.TwoLanesMotorway).SelectMany(r => r.Path.ToTerrainPolygon(r.Width)).ToList();

                var merged = TerrainPolygon.MergeAll(polys);

                foreach (var poly in merged)
                {
                    var points = GeometryHelper.PointsOnPathRegular(poly.Shell, MathF.Floor(template.Width));
                    foreach (var segment in points.Take(points.Count - 1).Zip(points.Skip(1)))
                    {
                        var delta = Vector2.Normalize(segment.Second.Vector - segment.First.Vector);
                        var center = new TerrainPoint(Vector2.Lerp(segment.First.Vector, segment.Second.Vector, 0.5f));
                        var angle = MathF.Atan2(delta.Y, delta.X) * 180 / MathF.PI;
                        layer.Insert(new TerrainObject(template, center, angle));
                    }
                }

                layer.WriteFile(Path.Combine(data.Config.Target.Objects, "roadwalls.rel.txt"));
            }
        }

        private static void MergeRoads(MapData data)
        {
            var report = new ProgressReport("MergeRoads", data.Roads.Count);
            var todo = new HashSet<Road>(data.Roads);
            var roadsPass2 = new List<Road>();
            foreach (var road in todo.ToList())
            {
                if (todo.Contains(road))
                {
                    todo.Remove(road);
                    roadsPass2.Add(road);

                    var connectedAtLast = ConnectedAt(road, todo, road.Path.LastPoint);
                    if (connectedAtLast.Count == 1)
                    {
                        todo.Remove(connectedAtLast[0]);
                        MergeInto(road, connectedAtLast[0]);
                    }

                    var connectedAtFirst = ConnectedAt(road, todo, road.Path.FirstPoint);
                    if (connectedAtFirst.Count == 1)
                    {
                        todo.Remove(connectedAtFirst[0]);
                        MergeInto(road, connectedAtFirst[0]);
                    }
                }
                report.ReportOneDone();
            }
            report.TaskDone();
            data.Roads = roadsPass2;
        }

        private static void MergeInto(Road target, Road source)
        {
            if (target.Path.LastPoint == source.Path.FirstPoint)
            {
                var a = target.Path.Points.ToList();
                var b = source.Path.Points.ToList();
                target.Path = new TerrainPath(a.Concat(b.Skip(1)).ToList());
            }
            else if (target.Path.FirstPoint == source.Path.LastPoint)
            {
                var a = source.Path.Points.ToList();
                var b = target.Path.Points.ToList();
                target.Path = new TerrainPath(a.Concat(b.Skip(1)).ToList());
            }
            else if (target.Path.LastPoint == source.Path.LastPoint)
            {
                var a = target.Path.Points.ToList();
                var b = Enumerable.Reverse(source.Path.Points).ToList();
                target.Path = new TerrainPath(a.Concat(b.Skip(1)).ToList());
            }
            else if (target.Path.FirstPoint == source.Path.FirstPoint)
            {
                var a = Enumerable.Reverse(target.Path.Points).ToList();
                var b = source.Path.Points.ToList();
                target.Path = new TerrainPath(a.Concat(b.Skip(1)).ToList());
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        private static List<Road> ConnectedAt(Road self, IEnumerable<Road> roads, TerrainPoint point)
        {
            return roads.Where(r => r != self && r.RoadType == self.RoadType && r.SpecialSegment == self.SpecialSegment && (r.Path.FirstPoint == point || r.Path.LastPoint == point)).ToList();
        }

        private static void SaveRoadsShp(MapData data, MapConfig config, bool ignoreBridges)
        {
            var features = new List<Feature>();
            foreach (var road in data.Roads)
            {
                if ( road.SpecialSegment == RoadSpecialSegment.Bridge && ignoreBridges )
                {
                    continue;
                }

                var attributesTable = new AttributesTable();
                attributesTable.Add("ID", (int)road.RoadType);
                features.Add(new Feature(road.Path.ToLineString(p => new Coordinate(p.X + 200000, p.Y)), attributesTable));
            }
            var header = ShapefileDataWriter.GetHeader(features.First(), features.Count);

            var roads = Path.Combine(config.Target.Cooked, "data", "roads");
            Directory.CreateDirectory(roads);
            var shapeWriter = new ShapefileDataWriter(Path.Combine(roads, "roads.shp"), new GeometryFactory())
            {
                Header = header
            };
            shapeWriter.Write(features);
        }

        private static void PreviewRoads(MapData data, string file = "roads.bmp")
        {
            var report = new ProgressReport("PreviewRoads", data.Roads.Count);
            using (var img = new Image<Rgb24>(data.MapInfos.ImageryWidth, data.MapInfos.ImageryHeight, TerrainMaterial.GrassShort.GetColor(data.Config.Terrain)))
            {
                img.Mutate(d =>
                {
                    var brush = new SolidBrush(OsmShapeCategory.Road.TerrainMaterial.GetColor(data.Config.Terrain));
                    foreach (var road in data.Roads)
                    {
                        DrawHelper.DrawPath(d, road.Path, road.Width, brush, data.MapInfos);
                        report.ReportOneDone();
                    }
                });
                report.TaskDone();
                Console.WriteLine("SaveBMP");
                img.Save(file);
            }
        }

        private static void PrepareRoads(MapData data, OsmStreamSource filtered, SnapshotDb db)
        {
            var interpret = new DefaultFeatureInterpreter2();
            var osmRoads = filtered.Where(o => o.Type == OsmGeoType.Way && o.Tags != null && o.Tags.ContainsKey("highway")).ToList();
            var area = data.MapInfos.Polygon;
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
                                        RoadType = kind.Value,
                                        SpecialSegment = OsmCategorizer.ToRoadSpecialSegment(road.Tags)
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
