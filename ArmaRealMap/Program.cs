using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using ArmaRealMap.Buildings;
using ArmaRealMap.ElevationModel;
using ArmaRealMap.Geometries;
using ArmaRealMap.GroundTextureDetails;
using ArmaRealMap.Libraries;
using ArmaRealMap.Osm;
using ArmaRealMap.Roads;
using ArmaRealMap.TerrainData.Forests;
using ClipperLib;
using Microsoft.Win32;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using OsmSharp;
using OsmSharp.Complete;
using OsmSharp.Db;
using OsmSharp.Db.Impl;
using OsmSharp.Geo;
using OsmSharp.Streams;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace ArmaRealMap
{
    class Program
    {
        static void Main(string[] args)
        {
            /*var clipper = new ClipperOffset();
            clipper.AddPath(new List<IntPoint>()
            {
                new IntPoint(0,0),
                new IntPoint(100,100),
                new IntPoint(200,100),
            }, JoinType.jtSquare, EndType.etOpenSquare);
            var tree = new List<List<IntPoint>>();
            clipper.Execute(ref tree, 1);*/
            /*
            var img = (Image<Rgba32>)Image.Load(@"C:\Users\Julien\source\repos\jetelain\Mali\mapdata\MALI_HM.png");
            var img2 = (Image<Rgba32>)Image.Load(@"C:\Users\Julien\source\repos\jetelain\Mali\mapdata\MALI_HM_CPLT.png");

            var grid = new ElevationGrid(new MapInfos()
            {
                CellSize = 5,
                Size = 8192,
                StartPointUTM = new CoordinateSharp.UniversalTransverseMercator("31N", 200000, 0)
            });

            var minElevation = 1f;
            var maxElevation = 300f;

            for(int x = 0; x< img.Width; ++x)
            {
                for (int y = 0; y < img.Height; ++y)
                {
                    var c2 = img2[x, y];
                    bool isCloseWater = c2.R > 250 && c2.G < 128 && c2.B < 128;
                    bool isWater = c2.B > 250 && c2.G < 128 && c2.R < 128;
                    var theory = minElevation + (img[x, y].R * maxElevation / 255);
                    if ( isWater)
                    {
                        grid.elevationGrid[x, img.Height - y - 1] = -1;
                    }
                    else if(isCloseWater && theory < 1)
                    {
                        grid.elevationGrid[x, img.Height - y - 1] = 1;
                    }
                    else
                    {
                        grid.elevationGrid[x, img.Height - y - 1] = theory;
                    }
                }
            }

            for (int x = 1; x < img.Width-1; ++x)
            {
                for (int y = 1; y < img.Height-1; ++y)
                {
                    var c2 = img2[x, y];
                    bool isCloseWater = c2.R > 250 && c2.G < 128 && c2.B < 128;
                    bool isWater = c2.B > 250 && c2.G < 128 && c2.R < 128;
                    if (isWater || isCloseWater)
                    {
                        var a = grid.elevationGrid[x-1, img.Height - y - 1];
                        var b = grid.elevationGrid[x+1, img.Height - y - 1];
                        var c = grid.elevationGrid[x, img.Height - y - 1];
                        var d = grid.elevationGrid[x, img.Height - y - 2];
                        var e = grid.elevationGrid[x, img.Height - y];
                        grid.elevationGrid[x, img.Height - y - 1] = (a+b+c+d+e)/5f;
                    }
                }
            }

            grid.SavePreviewToPng(@"C:\Users\Julien\source\repos\jetelain\Mali\mapdata\mali_hm_preview.png");
            grid.SaveToAsc(@"C:\Users\Julien\source\repos\jetelain\Mali\mapdata\mali.asc");*/

            EnsureProjectDrive();

            Console.Title = "ArmaRealMap";

            var config = LoadConfig(Path.GetFullPath("config.json"));

            Trace.Listeners.Clear();

            Trace.Listeners.Add(new TextWriterTraceListener(config.Target.GetDebug("arm.log")));

            Trace.WriteLine("----------------------------------------------------------------------------------------------------");

            var olibs = new ObjectLibraries();
            olibs.Load(config);
            File.WriteAllText(config.Target.GetTerrain("library.sqf"), olibs.TerrainBuilder.GetAllSqf());

            //GDTConfigBuilder.PrepareGDT(config);

            var data = new MapData();


            data.Config = config;

            var area = MapInfos.Create(config);

            data.MapInfos = area;

            data.Elevation = ElevationGridBuilder.LoadOrGenerateElevationGrid(data);

            //SatelliteRawImage(config, area);

            BuildLand(config, data, area, olibs);

            Trace.WriteLine("----------------------------------------------------------------------------------------------------");
            Trace.Flush();
        }

        private static Config LoadConfig(string configFilePath)
        {
            var config = JsonSerializer.Deserialize<Config>(File.ReadAllText(configFilePath));

            config.Target.Debug = GetExistingPath(configFilePath, config.Target.Debug);
            config.Target.Terrain = GetExistingPath(configFilePath, config.Target.Terrain);
            config.Target.Cache = GetExistingPath(configFilePath, config.Target.Cache);
            config.SRTM.Cache = GetExistingPath(configFilePath, config.SRTM.Cache);
            config.ORTHO.Path = GetPath(configFilePath, config.ORTHO.Path);
            config.OSM = GetPath(configFilePath, config.OSM);

            Console.WriteLine($"Target.Debug   = '{config.Target.Debug}'");
            Console.WriteLine($"Target.Terrain = '{config.Target.Terrain}'");
            Console.WriteLine($"Target.Cache   = '{config.Target.Cache}'");
            Console.WriteLine($"Target.Config  = '{config.Target.Config}'");
            Console.WriteLine($"SRTM.Cache     = '{config.SRTM.Cache}'");
            Console.WriteLine($"ORTHO.Path     = '{config.ORTHO.Path}'");
            Console.WriteLine($"OSM            = '{config.OSM}'");

            return config;
        }

        private static string GetPath(string configFilePath, string value)
        {
            return Path.Combine(Path.GetDirectoryName(configFilePath), Environment.ExpandEnvironmentVariables(value ?? string.Empty));
        }

        private static string GetExistingPath(string configFilePath, string value)
        {
            var fullPath = GetPath(configFilePath, value);
            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
            }
            return fullPath;
        }

#pragma warning disable CA1416 // Valider la compatibilité de la plateforme
        private static void EnsureProjectDrive()
        {
            if (!Directory.Exists("P:\\"))
            {
                Console.WriteLine("Mount project drive");
                string path = "";
                using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Steam App 233800"))
                {
                    if (key != null)
                    {
                        path = (key.GetValue("InstallLocation") as string) ?? path;
                    }
                }
                if (!string.IsNullOrEmpty(path))
                {
                    var processs = Process.Start(Path.Combine(path, @"WorkDrive\WorkDrive.exe"), "/Mount /y");
                    processs.WaitForExit();
                }
            }
        }
#pragma warning restore CA1416 // Valider la compatibilité de la plateforme


        private static void SatelliteRawImage(Config config, MapInfos area)
        {
            var rawSat = Path.Combine(config.Target?.Terrain ?? string.Empty, "sat-raw.png");
            if (!File.Exists(rawSat))
            {
                SatelliteImageBuilder.BuildSatImage(area, rawSat);
            }
        }

        private static void BuildLand(Config config, MapData data, MapInfos area, ObjectLibraries olibs)
        {
            data.UsedObjects = new HashSet<string>();

            using (var fileStream = File.OpenRead(config.OSM))
            {
                Console.WriteLine("Load OSM data...");
                var source = new PBFOsmStreamSource(fileStream);
                var db = new SnapshotDb(new MemorySnapshotDb(source));

                Console.WriteLine("Crop OSM data...");
                OsmStreamSource filtered = CropDataToArea(area, source);

                //RenderCitiesNames(config, area, filtered);

                RoadsBuilder.Roads(data, filtered, db, config, olibs);

                var shapes = OsmCategorizer.GetShapes(db, filtered);

                BuildingsBuilder.PlaceBuildings(data, olibs, shapes);

                ForestBuilder.Prepare(data, shapes, olibs);

                //new FillShapeWithObjects(area, olibs.Libraries.FirstOrDefault(l => l.Category == ObjectCategory.ForestTree))
                //    .MakeForest(shapes, filtered, db);

                PlaceIsolatedObjects(data, olibs, filtered);


                //MakeLakeDeeper(data, shapes);

                //DrawShapes(area, shapes);
            }


            var libs = olibs.TerrainBuilder.Libraries.Where(l => data.UsedObjects.Any(o => l.Template.Any(t => t.Name == o))).Distinct().ToList();
            File.WriteAllLines("required_tml.txt", libs.Select(t => t.Name));
        }

        private static void DownloadOsmData(Config config, MapInfos area)
        {
            if (!File.Exists(config.OsmXml))
            {
                var lonWestern = (float)Math.Min(area.SouthWest.Longitude.ToDouble(), area.NorthWest.Longitude.ToDouble());
                var latNorther = (float)Math.Max(area.NorthEast.Latitude.ToDouble(), area.NorthWest.Latitude.ToDouble());
                var lonEastern = (float)Math.Max(area.SouthEast.Longitude.ToDouble(), area.NorthEast.Longitude.ToDouble());
                var latSouthern = (float)Math.Min(area.SouthEast.Latitude.ToDouble(), area.SouthWest.Latitude.ToDouble());
                var uri = FormattableString.Invariant($"https://overpass-api.de/api/map?bbox={lonWestern},{latSouthern},{lonEastern},{latNorther}");
                using (var client = new WebClient())
                {
                    client.DownloadFile(uri, config.OsmXml);
                }
            }
        }

        private static void RenderCitiesNames(Config config, MapInfos area, OsmStreamSource filtered)
        {
            var places = filtered.Where(o => o.Type == OsmGeoType.Node && o.Tags.ContainsKey("place")).ToList();

            var id = 0;
            var sb = new StringBuilder();
            foreach (OsmSharp.Node place in places)
            {
                var kind = ToArmaKind(place.Tags.GetValue("place"));
                if (kind != null)
                {
                    var name = place.Tags.GetValue("name");
                    var pos = area.LatLngToTerrainPoint(place);

                    if (area.IsInside(pos))
                    {
                        sb.AppendLine(FormattableString.Invariant($@"class Item{id}
{{
    name = ""{name}"";
	position[]={{{pos.X:0.00},{pos.Y:0.00}}};
	type=""{kind}"";
	radiusA=500;
	radiusB=500;
	angle=0;
}};"));
                        id++;
                    }
                }
            }

            File.WriteAllText(Path.Combine(config.Target?.Config ?? string.Empty, "names.hpp"), sb.ToString());
        }

        private static string ToArmaKind(string place)
        {
            switch(place)
            {
                case "city": return "NameCityCapital";
                case "town": return "NameCity";
                case "village": return "NameVillage";
                case "hamlet": return "NameLocal";
            }
            return null;
        }

        private static OsmStreamSource CropDataToArea(MapInfos area, PBFOsmStreamSource source)
        {
            var left = (float)Math.Min(area.SouthWest.Longitude.ToDouble(), area.NorthWest.Longitude.ToDouble());
            var top = (float)Math.Max(area.NorthEast.Latitude.ToDouble(), area.NorthWest.Latitude.ToDouble());
            var right = (float)Math.Max(area.SouthEast.Longitude.ToDouble(), area.NorthEast.Longitude.ToDouble());
            var bottom = (float)Math.Min(area.SouthEast.Latitude.ToDouble(), area.SouthWest.Latitude.ToDouble());
            return source.FilterBox(left, top, right, bottom, true);
        }

        private static void PlaceIsolatedObjects(MapData data, ObjectLibraries olibs, OsmStreamSource filtered)
        {
            TerrainObjectLayer result = 
                PlaceObjects(
                    data, 
                    filtered, 
                    olibs.Libraries.FirstOrDefault(l => l.Category == ObjectCategory.IsolatedTree), 
                    o => OsmCategorizer.Get(o.Tags, "natural") == "tree");
            result.WriteFile(data.Config.Target.GetTerrain("trees.txt"));

            result =
                PlaceObjects(
                    data,
                    filtered,
                    olibs.Libraries.FirstOrDefault(l => l.Category == ObjectCategory.Bench),
                    o => OsmCategorizer.Get(o.Tags, "amenity") == "bench");
            result.WriteFile(data.Config.Target.GetTerrain("benches.txt"));

            result =
                PlaceObjects(
                    data,
                    filtered,
                    olibs.Libraries.FirstOrDefault(l => l.Category == ObjectCategory.PicnicTable),
                    o => OsmCategorizer.Get(o.Tags, "leisure") == "picnic_table");
            result.WriteFile(data.Config.Target.GetTerrain("picnictables.txt"));
        }

        private static TerrainObjectLayer PlaceObjects(MapData data, OsmStreamSource filtered, ObjectLibrary lib, Func<Node, bool> filter)
        {
            var candidates = lib.Objects;
            var area = data.MapInfos;
            var result = new TerrainObjectLayer(area);
            var nodes = filtered.Where(o => o.Type == OsmGeoType.Node && filter((Node)o)).ToList();
            foreach (Node node in nodes)
            {
                var pos = area.LatLngToTerrainPoint(node);
                if (area.IsInside(pos))
                {
                    var random = new Random((int)Math.Truncate(pos.X + pos.Y));
                    var obj = candidates[random.Next(0, candidates.Count)];
                    var angle = GetAngle(node, () => (float)(random.NextDouble() * 360.0));
                    result.Insert(new TerrainObject(obj, pos, angle));
                    data.UsedObjects.Add(obj.Name);
                }
            }
            return result;
        }

        private static float GetAngle(Node node, Func<float> defaultValue)
        {
            var dir = OsmCategorizer.Get(node.Tags, "direction");
            if (!string.IsNullOrEmpty(dir))
            {
                switch (dir.ToUpperInvariant())
                {
                    case "N": return 0f; 
                    case "NNE": return 22.5f; 
                    case "NE": return 45f; 
                    case "ENE": return 67.5f; 
                    case "E": return 90; 
                    case "ESE": return 112.5f; 
                    case "SE": return 135f; 
                    case "SSE": return 157.5f; 
                    case "S": return 180; 
                    case "SSW": return 202.5f; 
                    case "SW": return 225; 
                    case "WSW": return 247.5f; 
                    case "W": return 270; 
                    case "WNW": return 292.5f; 
                    case "NW": return 315;
                    case "NNW": return 337.5f;
                }
                float angle;
                if (float.TryParse(dir, out angle))
                {
                    return angle;
                }
            }
            return defaultValue();
        }

        private static void DrawShapes(MapInfos area, List<OsmShape> toRender)
        {
            var shapes = toRender.Count;
            var report = new ProgressReport("DrawShapes", shapes);
            using (var img = new Image<Rgb24>(area.Size * area.CellSize, area.Size * area.CellSize, TerrainMaterial.GrassShort.Color))
            {
                foreach (var item in toRender.OrderByDescending(e => e.Category.GroundTexturePriority))
                {
                    OsmDrawHelper.Draw(area, img, new SolidBrush(item.Category.GroundTextureColorCode), item);
                    report.ReportOneDone();
                }
                report.TaskDone();
                Console.WriteLine("SavePNG");
                img.Save("osm4.png");
            }
        }


    }
}
