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
    class Category
    {
        internal readonly Color Color;
        internal readonly int Priority;
        public Category(Color color, int priority)
        {
            Color = color;
            Priority = priority;
        }

        internal static Category Water = new Category(Color.Blue, 1);
        internal static Category WetLand = new Category(Color.LightBlue, 2);
        internal static Category Forest = new Category(Color.Green, 3);
        internal static Category Grass = new Category(Color.YellowGreen, 4);
        internal static Category FarmLand = new Category(Color.Yellow, 5);
        internal static Category Sand = new Category(Color.SandyBrown, 6);
        internal static Category Rocks = new Category(Color.DarkGray, 7);
        internal static Category Concrete = new Category(Color.Gray, 8);

        internal static Category Building = new Category(Color.Black, 0);
    }

    class CategorizedGeometry
    {
        internal readonly Category Category;
        internal readonly OsmGeo OsmGeo;
        internal readonly Geometry Geometry;

        public CategorizedGeometry(Category category, OsmGeo osmGeo, Geometry geometry)
        {
            this.Category = category;
            this.OsmGeo = osmGeo;
            this.Geometry = geometry;
        }
    }

    class Program
    {
        private static readonly EagerLoad eagerUTM = new EagerLoad(false) { UTM_MGRS = true };

        static void Main(string[] args)
        {
            var config = JsonSerializer.Deserialize<Config>(File.ReadAllText("config.json"));

            Trace.Listeners.Clear();
            Trace.Listeners.Add(new TextWriterTraceListener(@"osm.log"));
            Trace.WriteLine("----------------------------------------------------------------------------------------------------");

            var size = 4096;
            var cellSize = 5;

            //var startPointMGRS = new MilitaryGridReferenceSystem("32T", "LT", 23000, 70800);
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

                var filtered = source.FilterBox(left,top,right,bottom, true);

                var toRender = new List<CategorizedGeometry>();

                var interpret = new DefaultFeatureInterpreter2();
                var list = filtered.Where(osmGeo =>
                (osmGeo.Type == OsmSharp.OsmGeoType.Way || osmGeo.Type == OsmSharp.OsmGeoType.Relation)
                && osmGeo.Tags != null/*
                && interpret.IsPotentiallyArea(osmGeo.Tags)*/).ToList();

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

                var buildings = toRender.Count(b => b.Category == Category.Building);
                var shapes = toRender.Count(b => b.Category != Category.Building);

                using (var img = new Image<Rgb24>(area.Size * area.CellSize, area.Size * area.CellSize, Color.LightGreen))
                {
                    var done = 0;
                    foreach (var item in toRender.Where(b => b.Category != Category.Building).OrderByDescending(e => e.Category.Priority))
                    {
                        if ( done % 100 == 0)
                        {
                            Console.WriteLine($"Drawing ... {Math.Round(done * 100.0 / shapes, 2)}% done");
                        }
                        DrawGeometry(startPointUTM, img, new SolidBrush(item.Category.Color), item.Geometry);
                        done++;
                    }
                    img.Save("osm.png");
                }
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
                var points = ToMapPoints(startPointUTM, img, poly.Shell.Coordinates).ToArray();
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
                var points = ToMapPoints(startPointUTM, img, line.Coordinates).ToArray();
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

        private static PointF[] ToMapPoints(UniversalTransverseMercator startPointUTM, Image<Rgb24> img, Dictionary<long?, Node> nodes, Way way)
        {
            return ToMapPoints(startPointUTM, img, way.Nodes.Select(n => nodes[n])).ToArray();
        }
        private static IEnumerable<PointF> ToMapPoints(UniversalTransverseMercator startPointUTM, Image<Rgb24> img,  IEnumerable<Node> nodes)
        {
            return nodes
                .Select(n => new CoordinateSharp.Coordinate(n.Latitude.Value, n.Longitude.Value, eagerUTM).UTM)
                .Select(u => new PointF((float)(u.Easting - startPointUTM.Easting), (float)img.Height - (float)(u.Northing - startPointUTM.Northing)));
        }
        private static IEnumerable<PointF> ToMapPoints(UniversalTransverseMercator startPointUTM, Image<Rgb24> img, IEnumerable<NetTopologySuite.Geometries.Coordinate> nodes)
        {
            return nodes
                .Select(n => new CoordinateSharp.Coordinate(n.Y, n.X, eagerUTM).UTM)
                .Select(u => new PointF(
                    (float)(u.Easting - startPointUTM.Easting),
                    (float)img.Height - (float)(u.Northing - startPointUTM.Northing) 
                ));
        }
        


        private static void BuildImage( AreaInfos area)
        {
            var bd = new BdOrtho();

            using (var img = new Image<Rgb24>(area.Size * area.CellSize, area.Size * area.CellSize))
            {
                var startPointUTM = area.StartPointUTM;
                var eager = new EagerLoad(false);
                var sw = Stopwatch.StartNew();
                for (int y = 0; y < img.Height; y++)
                {
                    for (int x = 0; x < img.Width; x++)
                    {
                        var point = new UniversalTransverseMercator(
                            startPointUTM.LatZone,
                            startPointUTM.LongZone,
                            startPointUTM.Easting + (double)x,
                            startPointUTM.Northing + (double)y);

                        var latLong = UniversalTransverseMercator.ConvertUTMtoLatLong(point, eager);

                        img[x, img.Height - y - 1] = bd.GetPixel(latLong.Latitude.ToDouble(), latLong.Longitude.ToDouble());
                    }
                    var value = y + 1;
                    var percentDone = Math.Round((double)value * 100d / img.Height, 2);
                    var milisecondsLeft = sw.ElapsedMilliseconds * (img.Height - value) / value;
                    Console.WriteLine($"{percentDone}% - {Math.Ceiling(milisecondsLeft / 60000d)} min left");
                }
                sw.Stop();
                img.Save("sat.png");
            }


        }

        private static void BuildElevationGrid(ConfigSRTM configSRTM, AreaInfos area)
        {
            var credentials = new NetworkCredential(configSRTM.Login, configSRTM.Password);
            var srtmData = new SRTMData(configSRTM.Cache, new NASASource(credentials));
            var elevationMatrix = new double[area.Size, area.Size];
            var startPointUTM = area.StartPointUTM;
            var sw = Stopwatch.StartNew();
            var eager = new EagerLoad(false);
            var min = 4000d;
            var max = 0d;
            for (int y = 0; y < area.Size; y++)
            {
                for (int x = 0; x < area.Size; x++)
                {
                    var point = new UniversalTransverseMercator(
                            startPointUTM.LatZone,
                            startPointUTM.LongZone,
                            startPointUTM.Easting + (double)(x * area.CellSize) + (double)area.CellSize / 2.0d,
                            startPointUTM.Northing + (double)(y * area.CellSize) + (double)area.CellSize / 2.0d);

                    var latLong = UniversalTransverseMercator.ConvertUTMtoLatLong(point, eager);

                    var elevation = srtmData.GetElevationBilinear(latLong.Latitude.ToDouble(), latLong.Longitude.ToDouble()) ?? double.NaN;
                    elevationMatrix[x, y] = elevation;
                    max = Math.Max(elevation, max);
                    min = Math.Min(elevation, min);
                }

                var value = y + 1;
                var percentDone = Math.Round((double)value * 100d / area.Size, 2);
                var milisecondsLeft = sw.ElapsedMilliseconds * (area.Size - value) / value;
                Console.WriteLine($"{percentDone}% - {Math.Ceiling(milisecondsLeft / 60000d)} min left");
            }
            sw.Stop();

            using (var img = new Image<Rgb24>(area.Size, area.Size))
            {
                for (int y = 0; y < area.Size; y++)
                {
                    for (int x = 0; x < area.Size; x++)
                    {
                        byte value = (byte)((elevationMatrix[x, y] - min) * 255 / (max - min));
                        img[x, area.Size - y - 1] = new Rgb24(value, value, value);
                    }
                }
                img.Save("elevation.png");
            }

            using(var writer = new StreamWriter(new FileStream("elevation.asc", FileMode.Create, FileAccess.Write)))
            {
                writer.WriteLine($"ncols         {area.Size}");
                writer.WriteLine($"nrows         {area.Size}");
                writer.WriteLine($"xllcorner     {startPointUTM.Easting:0}");
                writer.WriteLine($"yllcorner     {startPointUTM.Northing:0}");
                writer.WriteLine($"cellsize      {area.CellSize}");
                writer.WriteLine($"NODATA_value  -9999");

                for (int y = 0; y < area.Size; y++)
                {
                    for (int x = 0; x < area.Size; x++)
                    {
                        writer.Write(elevationMatrix[x, area.Size - y - 1].ToString("0.00", CultureInfo.InvariantCulture));
                        writer.Write(" ");
                    }
                    writer.WriteLine();
                }
            }
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
