using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using ArmaRealMap.Buildings;
using ArmaRealMap.Core.ObjectLibraries;
using ArmaRealMap.Geometries;
using ArmaRealMap.GroundTextureDetails;
using ArmaRealMap.Libraries;
using ArmaRealMap.Osm;
using ArmaRealMap.Roads;
using ArmaRealMap.TerrainData;
using ArmaRealMap.TerrainData.DefaultAreas;
using ArmaRealMap.TerrainData.Forests;
using ArmaRealMap.TerrainData.GroundDetailTextures;
using Microsoft.Win32;
using OsmSharp;
using OsmSharp.Complete;
using OsmSharp.Db;
using OsmSharp.Db.Impl;
using OsmSharp.Geo;
using OsmSharp.Streams;

namespace ArmaRealMap
{
    class Program
    {
        static void Main(string[] args)
        {
            EnsureProjectDrive();

            Console.Title = "ArmaRealMap";

            var config = LoadConfig(args[0]);

            Trace.Listeners.Clear();

            Trace.Listeners.Add(new TextWriterTraceListener(config.Target.GetDebug("arm.log")));

            Trace.WriteLine("----------------------------------------------------------------------------------------------------");

            var olibs = new ObjectLibraries();
            olibs.Load(config);
            File.WriteAllText(config.Target.GetTerrain("library.sqf"), olibs.TerrainBuilder.GetAllSqf());

            var data = new MapData();

            data.Config = config;

            var area = MapInfos.Create(config);

            data.MapInfos = area;

            RenderMapInfos(config, area);

            Console.WriteLine("==== Satellite image ====");
            SatelliteRawImage(config, area);

            Console.WriteLine("==== Download OSM data ====");
            if (!DownloadOsmData(config, area))
            {
                return;
            }

            Console.WriteLine("==== Raw elevation grid (from SRTM) ====");
            data.Elevation = ElevationGridBuilder.LoadOrGenerateElevationGrid(data);

            BuildLand(config, data, area, olibs);

            Trace.WriteLine("----------------------------------------------------------------------------------------------------");
            Trace.Flush();
        }

        private static Config LoadConfig(string configFilePath)
        {
            var config = JsonSerializer.Deserialize<Config>(File.ReadAllText(configFilePath), new JsonSerializerOptions()
            {
                Converters = { new JsonStringEnumConverter() },
                ReadCommentHandling = JsonCommentHandling.Skip
            });

            config.Target.Debug = GetExistingPath(configFilePath, config.Target.Debug);
            config.Target.Terrain = GetExistingPath(configFilePath, config.Target.Terrain);
            config.Target.Cache = GetExistingPath(configFilePath, config.Target.Cache);
            config.SharedCache = GetExistingPath(configFilePath, config.SharedCache);
            config.SRTM.Cache = GetExistingPath(configFilePath, config.SRTM.Cache);
            if (config.ORTHO != null)
            {
                config.ORTHO.Path = GetPath(configFilePath, config.ORTHO.Path);
            }
            config.OSM = GetPath(configFilePath, config.OSM);

            if (!Directory.Exists(config.Target.Config))
            {
                Directory.CreateDirectory(config.Target.Config);
            }

            Console.WriteLine($"Target.Debug   = '{config.Target.Debug}'");
            Console.WriteLine($"Target.Terrain = '{config.Target.Terrain}'");
            Console.WriteLine($"Target.Cache   = '{config.Target.Cache}'");
            Console.WriteLine($"Target.Config  = '{config.Target.Config}'");
            Console.WriteLine($"SRTM.Cache     = '{config.SRTM.Cache}'");
            Console.WriteLine($"SharedCache    = '{config.SharedCache}'");
            if (config.ORTHO != null)
            {
                Console.WriteLine($"ORTHO.Path     = '{config.ORTHO.Path}'");
            }
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

                string path = GetArma3ToolsPath();
                if (!string.IsNullOrEmpty(path))
                {
                    var processs = Process.Start(Path.Combine(path, @"WorkDrive\WorkDrive.exe"), "/Mount /y");
                    processs.WaitForExit();
                }
            }
        }
        internal static string GetArma3ToolsPath()
        {
            string path = "";
            using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Steam App 233800"))
            {
                if (key != null)
                {
                    path = (key.GetValue("InstallLocation") as string) ?? path;
                }
            }

            return path;
        }
#pragma warning restore CA1416 // Valider la compatibilité de la plateforme


        private static void SatelliteRawImage(Config config, MapInfos area)
        {
            var rawSat = config.Target.GetTerrain("sat-raw.png");
            if (!File.Exists(rawSat))
            {
                SatelliteImageBuilder.BuildSatImage(area, rawSat, config);
            }
        }

        private static void BuildLand(Config config, MapData data, MapInfos area, ObjectLibraries olibs)
        {
            data.UsedObjects = new HashSet<string>();

            using (var fileStream = File.OpenRead(config.OSM))
            {
                Console.WriteLine("==== Prepare OSM data ====");
                var source = Path.GetExtension(config.OSM) == ".xml" ? (OsmStreamSource)new XmlOsmStreamSource(fileStream) : new PBFOsmStreamSource(fileStream);
                var db = new SnapshotDb(new MemorySnapshotDb(source));
                OsmStreamSource filtered = CropDataToArea(area, source);
                
                RenderCitiesNames(config, area, filtered);
                
                var shapes = config.IsScaled ? null : OsmCategorizer.GetShapes(db, filtered, data.MapInfos);

                Console.WriteLine("==== Roads ====");
                RoadsBuilder.Roads(data, filtered, db, config, olibs, shapes);

                if (config.IsScaled)
                {
                    data.Elevation.SaveToAsc(data.Config.Target.GetTerrain("elevation.asc"));
                }
                else
                {
                   /* Console.WriteLine("==== Lakes ====");
                    LakeGenerator.ProcessLakes(data, area, shapes);

                    Console.WriteLine("==== Elevation ====");
                    ElevationGridBuilder.MakeDetailed(data, shapes, olibs);

                    Console.WriteLine("==== Buildings ====");
                    BuildingsBuilder.PlaceBuildings(data, olibs, shapes);

                    Console.WriteLine("==== GroundRocks ====");
                    ForestBuilder.PrepareGroundRock(data, shapes, olibs);

                    Console.WriteLine("==== Forests ====");
                    ForestBuilder.Prepare(data, shapes, olibs);

                    Console.WriteLine("==== Scrub ====");
                    ForestBuilder.PrepareScrub(data, shapes, olibs);

                    Console.WriteLine("==== DefaultAreas ====");
                    DefaultAreasBuilder.Prepare(data, shapes, olibs);

                    Console.WriteLine("==== Objects ====");
                    PlaceIsolatedObjects(data, olibs, filtered);*/

                    Console.WriteLine("==== Fences and walls ====");
                    PlaceBarrierObjects(data, db, olibs, filtered);
                    /*

                    Console.WriteLine("==== Terrain images ====");
                    TerrainImageBuilder.GenerateTerrainImages(config, area, data, shapes);*/
                }
            }

            var libs = olibs.TerrainBuilder.Libraries.Where(l => data.UsedObjects.Any(o => l.Template.Any(t => t.Name == o))).Distinct().ToList();
            File.WriteAllLines("required_tml.txt", libs.Select(t => t.Name));
        }

        private static void PlaceBarrierObjects(MapData data, SnapshotDb db, ObjectLibraries olibs, OsmStreamSource filtered)
        {
            TerrainObjectLayer result =
                PlaceObjectsOnPath(
                    data,
                    db,
                    filtered,
                    olibs.Libraries.Where(l => l.Category == ObjectCategory.Wall).ToList(),
                    o => OsmCategorizer.Get(o.Tags, "barrier") == "wall");
            result.WriteFile(data.Config.Target.GetTerrain("walls.txt"));

            result =
                PlaceObjectsOnPath(
                    data,
                    db,
                    filtered,
                    olibs.Libraries.Where(l => l.Category == ObjectCategory.Fence).ToList(),
                    o => OsmCategorizer.Get(o.Tags, "barrier") == "fence");
            result.WriteFile(data.Config.Target.GetTerrain("fences.txt"));

            result =
                PlaceObjectsOnPath(
                    data,
                    db,
                    filtered,
                    olibs.Libraries.Where(l => l.Category == ObjectCategory.MilitaryWall).ToList(),
                    o => OsmCategorizer.Get(o.Tags, "barrier") == "city_wall");
            result.WriteFile(data.Config.Target.GetTerrain("milwalls.txt"));
        }

        private static TerrainObjectLayer PlaceObjectsOnPath(MapData data, SnapshotDb db, OsmStreamSource filtered, List<ObjectLibrary> libs, Func<Way, bool> filter)
        {
            var result = new TerrainObjectLayer(data.MapInfos);
            if (libs.Count == 0)
            {
                return result;
            }
            var probLibs = libs.Count == 1 ? libs : libs.SelectMany(l => Enumerable.Repeat(l, (int)((l.Probability ?? 1d) * 100))).ToList();
            var interpret = new DefaultFeatureInterpreter2();
            var nodes = filtered.Where(o => o.Type == OsmGeoType.Way && filter((Way)o)).ToList();
            var cliparea = data.MapInfos.Polygon;
            foreach (Way way in nodes)
            {
                var complete = way.CreateComplete(db);
                foreach (var feature in interpret.Interpret(complete))
                {
                    foreach (var path in TerrainPath.FromGeometry(feature.Geometry, data.MapInfos.LatLngToTerrainPoint))
                    {
                        foreach (var pathSegment in path.ClippedBy(cliparea))
                        {
                            var random = new Random((int)Math.Truncate(pathSegment.FirstPoint.X + pathSegment.FirstPoint.Y));
                            var lib = libs.Count == 1 ? libs[0] : libs[random.Next(0, libs.Count)];
                            FollowPathWithObjects.PlaceOnPath(random, lib, result, pathSegment.Points);
                        }
                    }
                }
            }
            return result;
        }



        private static bool DownloadOsmData(Config config, MapInfos area)
        {
            if (!File.Exists(config.OSM))
            {
                if (Path.GetExtension(config.OSM) != ".xml")
                {
                    Console.Error.WriteLine("OSM path extension must be '.xml' if file is not provided.");
                    return false;
                }

                var dir = Path.GetDirectoryName(config.OSM);
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                var lonWestern = (float)Math.Min(area.SouthWest.Longitude.ToDouble(), area.NorthWest.Longitude.ToDouble());
                var latNorther = (float)Math.Max(area.NorthEast.Latitude.ToDouble(), area.NorthWest.Latitude.ToDouble());
                var lonEastern = (float)Math.Max(area.SouthEast.Longitude.ToDouble(), area.NorthEast.Longitude.ToDouble());
                var latSouthern = (float)Math.Min(area.SouthEast.Latitude.ToDouble(), area.SouthWest.Latitude.ToDouble());
                var uri = FormattableString.Invariant($"https://overpass-api.de/api/map?bbox={lonWestern},{latSouthern},{lonEastern},{latNorther}");
                using (var client = new WebClient())
                {
                    client.DownloadFile(uri, config.OSM);
                }
            }
            return true;
        }


        private static void RenderMapInfos(Config config, MapInfos area)
        {
            var sb = new StringBuilder();

            var center = area.TerrainToLatLong(area.Width / 2, area.Height / 2);

            sb.Append(FormattableString.Invariant(@$"
latitude={center.Latitude.ToDouble():0.00000000};
longitude={center.Longitude.ToDouble():0.0000000};

mapArea[] = {{
    {area.SouthWest.Latitude.ToDouble():0.00000000}, {area.SouthWest.Longitude.ToDouble():0.0000000}, //Bottom Left => SW
	{area.NorthEast.Latitude.ToDouble():0.00000000}, {area.NorthEast.Longitude.ToDouble():0.0000000} //Top Right => NE
}}; 
mapSize={area.Width};
mapZone={area.SouthWest.UTM.LongZone};

class OutsideTerrain
{{
    colorOutside[] = {{0.227451,0.27451,0.384314,1}};
	enableTerrainSynth = 0;
	satellite = ""{config.Target.PboPrefix}\data\satout_ca.paa"";
    class Layers
    {{
		class Layer0
        {{
			nopx    = ""{TerrainMaterial.Default.NoPx(config.Terrain)}"";
			texture = ""{TerrainMaterial.Default.Co(config.Terrain)}""; 
		}};
    }};
}};

class Grid {{
    offsetX = 0;
    offsetY = {area.Height};
    arm_utm = ""{area.StartPointUTM.LongZone}{area.StartPointUTM.LatZone}"";
    arm_mgrs[] = {{
        {{ ""{area.SouthWest.MGRS.LongZone}{area.SouthWest.MGRS.LatZone} {area.SouthWest.MGRS.Digraph}"", {Math.Round(area.SouthWest.MGRS.Easting)}, {Math.Round(area.SouthWest.MGRS.Northing)} }}, // SW
        {{ ""{area.NorthWest.MGRS.LongZone}{area.NorthWest.MGRS.LatZone} {area.NorthWest.MGRS.Digraph}"", {Math.Round(area.NorthWest.MGRS.Easting)}, {Math.Round(area.NorthWest.MGRS.Northing)} }}, // NW
        {{ ""{area.NorthEast.MGRS.LongZone}{area.NorthEast.MGRS.LatZone} {area.NorthEast.MGRS.Digraph}"", {Math.Round(area.NorthEast.MGRS.Easting)}, {Math.Round(area.NorthEast.MGRS.Northing)} }}, // NE
        {{ ""{area.SouthEast.MGRS.LongZone}{area.SouthEast.MGRS.LatZone} {area.SouthEast.MGRS.Digraph}"", {Math.Round(area.SouthEast.MGRS.Easting)}, {Math.Round(area.SouthEast.MGRS.Northing)} }}  // SE
    }};
    class Zoom1 {{
        zoomMax = 0.05;
        format = ""XY"";
        formatX = ""000"";
        formatY = ""000"";
        stepX = 100;
        stepY = -100;
    }};
    class Zoom2 {{
        zoomMax = 0.5;
        format = ""XY"";
        formatX = ""00"";
        formatY = ""00"";
        stepX = 1000;
        stepY = -1000;
    }};
    class Zoom3 {{
        zoomMax = 1e+030;
        format = ""XY"";
        formatX = ""0"";
        formatY = ""0"";
        stepX = 10000;
        stepY = -10000;
    }};
}};
")); 

            File.WriteAllText(Path.Combine(config.Target?.Config ?? string.Empty, "mapinfos.hpp"), sb.ToString());
        }


        private static void RenderCitiesNames(Config config, MapInfos area, OsmStreamSource filtered)
        {
            var places = filtered.Where(o => o.Type == OsmGeoType.Node && o.Tags != null && o.Tags.ContainsKey("place")).ToList();

            var id = 0;
            var sb = new StringBuilder();
            foreach (OsmSharp.Node place in places)
            {
                var kind = ToArmaKind(place.Tags.GetValue("place"));
                if (kind != null && (!config.IsScaled || kind.StartsWith("NameCity")))
                {
                    var name = place.Tags.GetValue("name");
                    var pos = area.LatLngToTerrainPoint(place);

                    if (area.IsInside(pos))
                    {
                        if (config.IsScaled)
                        {
                            pos = new TerrainPoint(pos.X * (float)config.Scale.Value, pos.Y * (float)config.Scale.Value);
                        }

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
            switch (place)
            {
                case "city": return "NameCityCapital";
                case "town": return "NameCity";
                case "village": return "NameVillage";
                case "hamlet": return "NameLocal";
            }
            return null;
        }

        private static OsmStreamSource CropDataToArea(MapInfos area, OsmStreamSource source)
        {
            var left = (float)Math.Min(area.SouthWest.Longitude.ToDouble(), area.NorthWest.Longitude.ToDouble());
            var top = (float)Math.Max(area.NorthEast.Latitude.ToDouble(), area.NorthWest.Latitude.ToDouble());
            var right = (float)Math.Max(area.SouthEast.Longitude.ToDouble(), area.NorthEast.Longitude.ToDouble());
            var bottom = (float)Math.Min(area.SouthEast.Latitude.ToDouble(), area.SouthWest.Latitude.ToDouble());
            return source.FilterBox(left, top, right, bottom, true);
        }

        private static void PlaceIsolatedObjects(MapData data, ObjectLibraries olibs, OsmStreamSource filtered)
        {
            var result = new TerrainObjectLayer(data.MapInfos);

            PlaceObjects(result,
                    data,
                    filtered,
                    olibs.Libraries.FirstOrDefault(l => l.Category == ObjectCategory.IsolatedTree),
                    o => OsmCategorizer.Get(o.Tags, "natural") == "tree");
            PlaceObjects(result,
                    data,
                    filtered,
                    olibs.Libraries.FirstOrDefault(l => l.Category == ObjectCategory.Bench),
                    o => OsmCategorizer.Get(o.Tags, "amenity") == "bench");

            PlaceObjects(result,
                    data,
                    filtered,
                    olibs.Libraries.FirstOrDefault(l => l.Category == ObjectCategory.PicnicTable),
                    o => OsmCategorizer.Get(o.Tags, "leisure") == "picnic_table");

            PlaceObjects(result,
                    data,
                    filtered,
                    olibs.Libraries.FirstOrDefault(l => l.Category == ObjectCategory.WaterWell),
                    o => OsmCategorizer.Get(o.Tags, "man_made") == "water_well");

            result.WriteFile(data.Config.Target.GetTerrain("objects.txt"));
        }

        private static void PlaceObjects(TerrainObjectLayer result, MapData data, OsmStreamSource filtered, ObjectLibrary lib, Func<Node, bool> filter)
        {
            if (lib != null)
            {
                var candidates = lib.Objects;
                var area = data.MapInfos;
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
            }
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
    }
}
