using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;
using CoordinateSharp;
using NetTopologySuite.Geometries;
using OsmSharp;
using OsmSharp.Complete;
using OsmSharp.Db;
using OsmSharp.Db.Impl;
using OsmSharp.Geo;
using OsmSharp.Streams;
using OsmSharp.Tags;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SRTM;
using SRTM.Sources.NASA;

namespace ArmaRealMap
{
    class Program
    {
        private static readonly EagerLoad eagerUTM = new EagerLoad(false) { UTM_MGRS = true };

        static void Main(string[] args)
        {
            var config = JsonSerializer.Deserialize<Config>(File.ReadAllText("config.json"));

            Trace.Listeners.Clear();
            Trace.Listeners.Add(new TextWriterTraceListener(@"osm.log"));
            Trace.WriteLine("----------------------------------------------------------------------------------------------------");

            var size = config.GridSize;
            var cellSize = config.CellSize;

            var startPointMGRS = new MilitaryGridReferenceSystem(config.BottomLeft.GridZone, config.BottomLeft.D, config.BottomLeft.E, config.BottomLeft.N);

            var area = GetArea(startPointMGRS, size, cellSize);

            //BuildImage(area);

            //BuildElevationGrid(area);

            BuildLand(config, area);

            Trace.WriteLine("----------------------------------------------------------------------------------------------------");
            Trace.Flush();
        }

        private static void BuildLand(Config config,AreaInfos area)
        {
            var startPointUTM = area.StartPointUTM;

            var left = (float)Math.Min(area.SouthWest.Longitude.ToDouble(), area.NorthWest.Longitude.ToDouble());
            var top = (float)Math.Max(area.NorthEast.Latitude.ToDouble(), area.NorthWest.Latitude.ToDouble());
            var right = (float)Math.Max(area.SouthEast.Longitude.ToDouble(), area.NorthEast.Longitude.ToDouble());
            var bottom = (float)Math.Min(area.SouthEast.Latitude.ToDouble(), area.SouthWest.Latitude.ToDouble());

            using (var fileStream = File.OpenRead(config.OSM))
            {
                var source = new PBFOsmStreamSource(fileStream);

                var db = new SnapshotDb(new MemorySnapshotDb(source));

                var filtered = source.FilterBox(left, top, right, bottom, true);

                var toRender = GetShapes(db, filtered);

                var buildings = toRender.Count(b => b.Category == Category.Building);

                foreach(var building in toRender.Where(b => b.Category == Category.Building))
                {
                    var box1 = ComputeBox(ToPixelsPoints(startPointUTM, area.Height, building.Geometry.Coordinates).ToArray());
                    var box2 = ComputeBox(ToTerrainBuilderPoints(startPointUTM, building.Geometry.Coordinates).ToArray());
                }

                //DrawShapes(area, startPointUTM, toRender);
            }
        }

        private static BoudingBox ComputeBox(PointF[] points)
        {
            int i;

            double ax;
            double ay;
            double bx;
            double by;

            double dx;
            double dy;

            double minx;
            double miny;
            double maxx;
            double maxy;

            double theta;
            double minarea;
            double area;

            double cw = 0;
            double ch = 0;
            double cx = 0;
            double cy = 0;
            double ca = 0;

            int j;

            minarea = double.MaxValue;

            ax = points[points.Length - 1].X;
            ay = points[points.Length - 1].Y;

            for (i = 0; i < points.Length; i++)
            {
                bx = points[i].X; by = points[i].Y;
                dx = (ax - bx);
                dy = (ay - by);
                theta = Math.Atan2(dy, dx);
                maxx = double.MinValue;
                maxy = double.MinValue;
                minx = double.MaxValue;
                miny = double.MaxValue;
                for (j = 0; j < points.Length; j++)
                {
                    var rx = ((points[j].X - ax) * Math.Cos(theta)) + ((points[j].Y - ay) * Math.Sin(theta));
                    var ry = ((points[j].Y - ay) * Math.Cos(theta)) - ((points[j].X - ax) * Math.Sin(theta));
                    if (rx > maxx) { maxx = rx; }
                    if (ry > maxy) { maxy = ry; }
                    if (rx < minx) { minx = rx; }
                    if (ry < miny) { miny = ry; }
                }
                area = (maxx - minx) * (maxy - miny);
                if (area < minarea)
                {
                    minarea = area;
                    cw = (maxx - minx);
                    ch = (maxy - miny);
                    ca = theta * 180.0 / Math.PI;
                    cx = ((minx + maxx) * Math.Cos(-theta) / 2) + ((miny + maxy) * Math.Sin(-theta) / 2) + ax;
                    cy = ((miny + maxy) * Math.Cos(-theta) / 2) - ((minx + maxx) * Math.Sin(-theta) / 2) + ay;

                    //var c1xp = (minx * Math.Cos(-theta)) + (miny * Math.Sin(-theta)) + ax;
                    //var c1yp = (miny * Math.Cos(-theta)) - (minx * Math.Sin(-theta)) + ay;
                    //var c2xp = (maxx * Math.Cos(-theta)) + (miny * Math.Sin(-theta)) + ax;
                    //var c2yp = (miny * Math.Cos(-theta)) - (maxx * Math.Sin(-theta)) + ay;
                    //var c3xp = (maxx * Math.Cos(-theta)) + (maxy * Math.Sin(-theta)) + ax;
                    //var c3yp = (maxy * Math.Cos(-theta)) - (maxx * Math.Sin(-theta)) + ay;
                    //var c4xp = (minx * Math.Cos(-theta)) + (maxy * Math.Sin(-theta)) + ax;
                    //var c4yp = (maxy * Math.Cos(-theta)) - (minx * Math.Sin(-theta)) + ay;
                    //cx = (c3xp + c1xp) / 2;
                    //cy = (c3yp + c1yp) / 2;
                }
                ax = bx;
                ay = by;
            }

            return new BoudingBox(cx, cy, cw, ch, ca);
        }

        private static List<CategorizedGeometry> GetShapes(SnapshotDb db, OsmStreamSource filtered)
        {
            var toRender = new List<CategorizedGeometry>();

            var interpret = new DefaultFeatureInterpreter2();
            var list = filtered.Where(osmGeo =>
            (osmGeo.Type == OsmSharp.OsmGeoType.Way || osmGeo.Type == OsmSharp.OsmGeoType.Relation)
            && osmGeo.Tags != null).ToList();

            foreach (OsmGeo osmGeo in list)
            {
                var category = GetCategory(osmGeo.Tags, interpret);
                if (category != null)
                {
                    var complete = osmGeo.CreateComplete(db);
                    var count = 0;
                    foreach (var feature in interpret.Interpret(complete))
                    {
                        toRender.Add(new CategorizedGeometry(category, osmGeo, feature.Geometry));
                        count++;
                    }
                    if (count == 0)
                    {

                    }
                }
            }

            return toRender;
        }

        private static void DrawShapes(AreaInfos area, UniversalTransverseMercator startPointUTM, List<CategorizedGeometry> toRender)
        {
            var shapes = toRender.Count(b => b.Category != Category.Building);

            using (var img = new Image<Rgb24>(area.Size * area.CellSize, area.Size * area.CellSize, Color.LightGreen))
            {
                var done = 0;
                foreach (var item in toRender.Where(b => b.Category != Category.Building).OrderByDescending(e => e.Category.Priority))
                {
                    if (done % 100 == 0)
                    {
                        Console.WriteLine($"Drawing ... {Math.Round(done * 100.0 / shapes, 2)}% done");
                    }
                    DrawGeometry(startPointUTM, img, new SolidBrush(item.Category.Color), item.Geometry);
                    done++;
                }
                img.Save("osm.png");
            }
        }

        private static string Get(TagsCollectionBase tags, string key)
        {
            string value;
            if (tags.TryGetValue(key, out value))
            {
                return value;
            }
            return null;
        }

        private static void DrawGeometry(UniversalTransverseMercator startPointUTM, Image<Rgb24> img, IBrush solidBrush, Geometry geometry)
        {
            if (geometry.OgcGeometryType == OgcGeometryType.MultiPolygon)
            {
                foreach (var geo in ((GeometryCollection)geometry).Geometries)
                {
                    DrawGeometry(startPointUTM, img, solidBrush, geo);
                }
            }
            else if (geometry.OgcGeometryType == OgcGeometryType.Polygon)
            {
                var poly = (Polygon)geometry;
                // TODO : holes
                var points = ToPixelsPoints(startPointUTM, img, poly.Shell.Coordinates).ToArray();
                try
                {
                    img.Mutate(p => p.FillPolygon(solidBrush, points));
                }
                catch
                {

                }
            }
            else if (geometry.OgcGeometryType == OgcGeometryType.LineString)
            {
                var line = (LineString)geometry;
                var points = ToPixelsPoints(startPointUTM, img, line.Coordinates).ToArray();
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



        private static Category GetCategory(TagsCollectionBase tags, FeatureInterpreter interpreter)
        {
            if ( tags.ContainsKey("water") || (tags.ContainsKey("waterway") && !tags.IsFalse("waterway")))
            {
                return Category.Water;
            }
            if (tags.ContainsKey("building") && !tags.IsFalse("building"))
            {
                return Category.Building;
            }

            if (Get(tags, "type") == "boundary")
            {
                return null;
            }

            switch (Get(tags, "surface"))
            {
                case "grass": return Category.Grass;
                case "sand": return Category.Sand;
                case "concrete": return Category.Concrete;
            }

            switch (Get(tags, "landuse"))
            { 
                case "forest": return Category.Forest;
                case "grass": return Category.Grass;
                case "farmland": return Category.FarmLand;
                case "farmyard": return Category.FarmLand;
                case "vineyard": return Category.FarmLand;
                case "orchard": return Category.FarmLand;
                case "meadow": return Category.FarmLand;
                case "industrial": return Category.Concrete;
                case "residential": return Category.Concrete;
                case "cemetery": return Category.Concrete;
                case "railway": return Category.Concrete;
                case "retail": return Category.Concrete;

                case "basin": return Category.Water;
                case "reservoir": return Category.Water;
                case "allotments": return Category.Grass;
                //case "military": return Color.DarkRed;
            }

            switch (Get(tags, "natural"))
            { 
                case "wood": return Category.Forest;
                case "water": return Category.Water;
                case "grass": return Category.Grass;
                case "heath": return Category.Grass;
                case "grassland": return Category.Grass;
                case "scrub": return Category.Grass;
                case "wetland": return Category.WetLand;
                case "tree_row": return Category.Forest;
                case "scree": return Category.Sand;
                case "sand": return Category.Sand;
                case "beach": return Category.Sand;
            }


            if (interpreter.IsPotentiallyArea(tags))
            {
                tags.RemoveKey("source");
                tags.RemoveKey("name");
                tags.RemoveKey("alt_name");
                Trace.WriteLine(tags);
                //Console.WriteLine(tags);
            }
            return null;
        }

        private static IEnumerable<PointF> ToPixelsPoints(UniversalTransverseMercator startPointUTM, Image<Rgb24> img, IEnumerable<NetTopologySuite.Geometries.Coordinate> nodes)
        {
            return ToPixelsPoints(startPointUTM, img.Height, nodes);
        }

        private static IEnumerable<PointF> ToPixelsPoints(UniversalTransverseMercator startPointUTM, float height, IEnumerable<NetTopologySuite.Geometries.Coordinate> nodes)
        {
            return nodes
                .Select(n => new CoordinateSharp.Coordinate(n.Y, n.X, eagerUTM).UTM)
                .Select(u => new PointF(
                    (float)(u.Easting - startPointUTM.Easting),
                    (float)height - (float)(u.Northing - startPointUTM.Northing)
                ));
        }


        private static IEnumerable<PointF> ToTerrainBuilderPoints(UniversalTransverseMercator startPointUTM, IEnumerable<NetTopologySuite.Geometries.Coordinate> nodes)
        {
            return nodes
                .Select(n => new CoordinateSharp.Coordinate(n.Y, n.X, eagerUTM).UTM)
                .Select(u => new PointF(
                    (float)(u.Easting - startPointUTM.Easting) + 200000f,
                    (float)(u.Northing - startPointUTM.Northing)
                ));
        }

        private static AreaInfos GetArea(MilitaryGridReferenceSystem startPointMGRS, int size, int cellSize)
        {
            var southWest = MilitaryGridReferenceSystem.MGRStoLatLong(startPointMGRS);

            var startPointUTM = new UniversalTransverseMercator(
                southWest.UTM.LatZone,
                southWest.UTM.LongZone,
                Math.Round(southWest.UTM.Easting),
                Math.Round(southWest.UTM.Northing));

            var southEast = UniversalTransverseMercator.ConvertUTMtoLatLong(new UniversalTransverseMercator(
                southWest.UTM.LatZone,
                southWest.UTM.LongZone,
                Math.Round(southWest.UTM.Easting) + (size * cellSize),
                Math.Round(southWest.UTM.Northing)));

            var northEast = UniversalTransverseMercator.ConvertUTMtoLatLong(new UniversalTransverseMercator(
                southWest.UTM.LatZone,
                southWest.UTM.LongZone,
                Math.Round(southWest.UTM.Easting) + (size * cellSize),
                Math.Round(southWest.UTM.Northing) + (size * cellSize)));

            var northWest = UniversalTransverseMercator.ConvertUTMtoLatLong(new UniversalTransverseMercator(
                southWest.UTM.LatZone,
                southWest.UTM.LongZone,
                Math.Round(southWest.UTM.Easting),
                Math.Round(southWest.UTM.Northing) + (size * cellSize)));

            return new AreaInfos
            {
                StartPointMGRS = startPointMGRS,
                StartPointUTM = startPointUTM,
                SouthWest = southWest,
                NorthEast = northEast,
                NorthWest = northWest,
                SouthEast = southEast,
                CellSize = cellSize,
                Size = size
            };
        }
    }
}
