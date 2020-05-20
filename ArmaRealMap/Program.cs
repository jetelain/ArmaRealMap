using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Xsl;
using CoordinateSharp;
using lambertcs;
using NetTopologySuite.Geometries;
using OsmSharp;
using OsmSharp.Complete;
using OsmSharp.Geo;
using OsmSharp.Streams;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.ColorSpaces;
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
            var size = 4096;
            var cellSize = 5;

            var startPointMGRS = new MilitaryGridReferenceSystem("32T", "LT", 23000, 70800);

            var area = GetArea(startPointMGRS, size, cellSize);

            //BuildImage(area);

            //BuildElevationGrid(area);

            BuildLand(area);
        }

        private static void BuildLand(AreaInfos area)
        {
            var startPointUTM = area.StartPointUTM;

            var left = (float)Math.Min(area.SouthWest.Longitude.ToDouble(), area.NorthWest.Longitude.ToDouble());
            var top = (float)Math.Max(area.NorthEast.Latitude.ToDouble(), area.NorthWest.Latitude.ToDouble());
            var right = (float)Math.Max(area.SouthEast.Longitude.ToDouble(), area.NorthEast.Longitude.ToDouble());
            var bottom = (float)Math.Min(area.SouthEast.Latitude.ToDouble(), area.SouthWest.Latitude.ToDouble());

            using (var fileStream = File.OpenRead(@"E:\Carto\area.osm.pbf"))
            {
                var source = new PBFOsmStreamSource(fileStream);


                var filtered = source;//.FilterBox(left,top,right,bottom, true);

                using (var img = new Image<Rgb24>(area.Size * area.CellSize, area.Size * area.CellSize))
                {
                    var nodes = filtered.Where(n => n.Type == OsmSharp.OsmGeoType.Node).Cast<Node>().ToDictionary(n => n.Id, n => n);
                    var ways = filtered.Where(n => n.Type == OsmSharp.OsmGeoType.Way).Cast<Way>().ToDictionary(n => n.Id, n => n);

                    var forestList = filtered.Where(osmGeo => osmGeo.Type == OsmSharp.OsmGeoType.Way && osmGeo.Tags != null && osmGeo.Tags.ContainsKey("landuse")).ToList();
                    var forestPolyList = filtered.Where(osmGeo => osmGeo.Type == OsmSharp.OsmGeoType.Relation && osmGeo.Tags != null && osmGeo.Tags.ContainsKey("landuse")).ToList();

                    foreach (Relation land in forestPolyList)
                    {
                        var landuse = GetColor(land.Tags["landuse"]);

                        if (landuse != null)
                        {
                            var points = ProcessPoly(startPointUTM, img, nodes, ways, land);
                            if (points.Any())
                            {
                                img.Mutate(p => p.FillPolygon(new SolidBrush(landuse.Value), points.ToArray()));
                            }
                        }
                    }
                    foreach (Way land in forestList)
                    {
                        var landuse = GetColor(land.Tags["landuse"]);
                        if (landuse != null)
                        {
                            var points = ToMapPoints(startPointUTM, img, nodes, land);
                            img.Mutate(p => p.FillPolygon(new SolidBrush(landuse.Value), points));
                        }
                    }

                    img.Save("osm.png");
                }
            }
        }

        private static Color? GetColor(string landuse)
        {
            switch (landuse)
            { // TODO: les zones peuvent avoir plusieurs landuse, il faut faire plusieurs passes pour prioriser
                case "forest": return Color.Green;
                case "grass": return Color.YellowGreen;
                case "farmland": return Color.Yellow;
                case "farmyard": return Color.Yellow;
                case "vineyard": return Color.Yellow;
                case "orchard": return Color.Yellow;
                case "meadow": return Color.Yellow;
                case "industrial": return Color.Gray;
                case "residential": return Color.Gray;
                case "cemetery": return Color.Gray;
                case "retail": return Color.Gray;
            }
            Console.WriteLine(landuse);
            return null;
        }

        private static List<PointF> ProcessPoly(UniversalTransverseMercator startPointUTM, Image<Rgb24> img, Dictionary<long?, Node> nodes, Dictionary<long?, Way> ways, Relation forest)
        {
            List<PointF> points = new List<PointF>();


            var currentOuter = new List<Node>();
            var currentInner = new List<Node>();
            var inners = new List<List<Node>>();

            foreach (var member in forest.Members)
            {
                if (member.Role == "outer")
                {
                    if (currentOuter.Count > 0 && currentOuter.First() == currentOuter.Last())
                    {
                        FlushOuter(startPointUTM, img, points, currentOuter, currentInner, inners);
                    }
                    currentOuter.AddRange(ways[member.Id].Nodes.Select(id => nodes[id]));
                }
                else if (member.Role == "inner")
                {
                    if (currentInner.Count > 0 && currentInner.First() == currentInner.Last())
                    {
                        inners.Add(currentInner.ToList());
                        currentInner.Clear();
                    }
                    currentInner.AddRange(ways[member.Id].Nodes.Select(id => nodes[id]));
                }
            }

            FlushOuter(startPointUTM, img, points, currentOuter, currentInner, inners);
            return points;
        }

        private static void FlushOuter(UniversalTransverseMercator startPointUTM, Image<Rgb24> img, List<PointF> points, List<Node> currentOuter, List<Node> currentInner, List<List<Node>> inners)
        {
            if (currentInner.Count > 0)
            {
                inners.Add(currentInner.ToList());
                currentInner.Clear();
            }
            points.AddRange(ToMapPoints(startPointUTM, img, currentOuter));

            foreach (var inner in inners)
            {
                if (points.Any())
                {
                    var loopBack = points.Last();
                    points.AddRange(ToMapPoints(startPointUTM, img, inner));
                    points.Add(loopBack);
                }
                else
                {
                    points.AddRange(ToMapPoints(startPointUTM, img, inner));
                }
            }
            
            inners.Clear();
            currentOuter.Clear();
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

        private static void BuildElevationGrid(AreaInfos area)
        {
            var credentials = new NetworkCredential("", "");
            var srtmData = new SRTMData(@"E:\Carto\SRTMv3", new NASASource(credentials));
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
                writer.WriteLine($"nrows         {area.Size}");
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
