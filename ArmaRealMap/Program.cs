using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using ArmaRealMap.Buildings;
using ArmaRealMap.Configuration;
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
using CommandLine;
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
        static int Main(string[] args)
        {
            try
            {
                return Parser.Default
                    .ParseArguments<GenerateOptions,UpdateOptions, WrpExportOptions, ShowConfigOptions, ConvertToPaaOptions>(args)
                    .MapResult<GenerateOptions, UpdateOptions, WrpExportOptions, ShowConfigOptions, ConvertToPaaOptions, int>(Run, Update, WrpExport, ShowConfig, ConvertToPaa, _ => 1);
            }
            catch (ApplicationException ex)
            {
                Trace.Flush();
                Console.Error.Write("Error: ");
                Console.Error.WriteLine(ex.Message);
                return 2;
            }
            catch (Exception ex)
            {
                Trace.Flush();
                Console.Error.Write("Unexpected error:");
                Console.Error.WriteLine(ex.ToString());
                return 3;
            }
        }

        private static int ConvertToPaa(ConvertToPaaOptions arg)
        {
            var files = Directory.GetFiles(arg.Directory, "*.png", SearchOption.AllDirectories);

            Arma3ToolsHelper.ImageToPAA(files.ToList());

            return 0;
        }

        private static int ShowConfig(ShowConfigOptions options)
        {
            ConfigLoader.LoadGlobal(options.Global);
            if (!string.IsNullOrEmpty(options.Source))
            {
                ConfigLoader.LoadConfig(options.Source);
            }
            return 0;
        }

        private static int WrpExport(WrpExportOptions options)
        {
            Arma3ToolsHelper.EnsureProjectDrive();

            var global = ConfigLoader.LoadGlobal(options.Global);

            SyncLibraries(global);

            WrpBuilder.WrpExport(options, global);

            return 0;
        }

        private static int Update(UpdateOptions options)
        {
            Arma3ToolsHelper.EnsureProjectDrive();

            var global = ConfigLoader.LoadGlobal(options.Global);

            SyncLibraries(global);

            var library = new ModelInfoLibrary();
            library.LoadAndUpdate(global.ModelsInfoFile);
            library.Save(global.ModelsInfoFile);

            Console.WriteLine($"File '{global.ModelsInfoFile}' was updated.");
            Console.WriteLine("Please upload it to https://arm.pmad.net/Assets/UploadModelsInfo");

            var terrainMaterialLibrary = new TerrainMaterialLibrary();
            terrainMaterialLibrary.LoadFromProjectDrive();
            terrainMaterialLibrary.Save(global.TerrainMaterialFile);

            Console.WriteLine();
            Console.WriteLine($"File '{global.TerrainMaterialFile}' was updated.");

            return 0;
        }

        private static void SyncLibraries(GlobalConfig global)
        {
            ConfigLoader.UpdateFile(global.LibrariesFile, "https://arm.pmad.net/ObjectLibraries/Export");
            ConfigLoader.UpdateFile(global.ModelsInfoFile, "https://arm.pmad.net/Assets/ModelsInfo");
        }

        private static int Run(GenerateOptions options)
        {
            Arma3ToolsHelper.EnsureProjectDrive();

            Console.Title = "ArmaRealMap";

            var global = ConfigLoader.LoadGlobal(options.Global);
            var config = ConfigLoader.LoadConfig(options.Source);

            SetupLogging(config);

            SyncLibraries(global);

            var area = MapInfos.Create(config);

            var olibs = new ObjectLibraries();
            olibs.Load(global.LibrariesFile, config.Terrain);

            var data = new MapData();
            data.Config = config;
            data.GlobalConfig = global;
            data.MapInfos = area;

            var terrainMaterialLibrary = new TerrainMaterialLibrary();
            terrainMaterialLibrary.LoadFromFile(global.TerrainMaterialFile, config.Terrain);

            RenderMapInfos(config, area, terrainMaterialLibrary);

            Console.WriteLine("==== Satellite image ====");
            SatelliteRawImage(config, global, area);

            Console.WriteLine("==== Raw elevation grid (from SRTM) ====");
            data.Elevation = ElevationGridBuilder.LoadOrGenerateElevationGrid(data);

            BuildLand(config, data, area, olibs, options, terrainMaterialLibrary);

            Console.WriteLine("==== Generate WRP ====");
            WrpBuilder.Build(config, data.Elevation, area, global);

            Trace.Flush();
            return 0;
        }

        private static void SetupLogging(MapConfig config)
        {
            var logfile = Path.Combine(config.Target.Debug, $"{DateTime.UtcNow:yyyy-MM-dd_HH-mm-ss}.log");
            Trace.Listeners.Clear();
            Trace.Listeners.Add(new TextWriterTraceListener(logfile));
            Trace.WriteLine(JsonSerializer.Serialize(config, new JsonSerializerOptions() { WriteIndented = true }));
            Trace.Flush();
        }



        private static void SatelliteRawImage(MapConfig config, GlobalConfig global, MapInfos area)
        {
            var rawSat = Path.Combine(config.Target.InputCache, "sat-raw.png");
            if (!File.Exists(rawSat))
            {
                SatelliteImageBuilder.BuildSatImage(area, rawSat, global);
            }
        }

        private static void BuildLand(MapConfig config, MapData data, MapInfos area, ObjectLibraries olibs, GenerateOptions options, TerrainMaterialLibrary terrainMaterialLibrary)
        {
            data.UsedObjects = new HashSet<string>();

            var osmFile = Path.Combine(config.Target.InputCache, "area.osm.xml");

            DownloadOsmData(area, osmFile, options);

            using (var fileStream = File.OpenRead(osmFile))
            {
                Console.WriteLine("==== Prepare OSM data ====");
                var source = new XmlOsmStreamSource(fileStream);
                var db = new SnapshotDb(new MemorySnapshotDb(source));
                var filtered = CropDataToArea(area, source);
                
                RenderCitiesNames(config, area, filtered);
                
                var shapes = OsmCategorizer.GetShapes(db, filtered, data.MapInfos);

                Console.WriteLine("==== Roads ====");
                RoadsBuilder.Roads(data, filtered, db, config, olibs, shapes, options);
                
                Console.WriteLine("==== Lakes ====");
                LakeGenerator.ProcessLakes(data, area, shapes, options);
                    
                Console.WriteLine("==== Elevation ====");
                ElevationGridBuilder.MakeDetailed(data, shapes, olibs, options);

                if (!options.DoNotGenerateObjects)
                {
                    Console.WriteLine("==== Buildings ====");
                    BuildingsBuilder.PlaceBuildings(data, olibs, shapes);

                    Console.WriteLine("==== GroundRocks ====");
                    NatureBuilder.PrepareGroundRock(data, shapes, olibs);

                    Console.WriteLine("==== Forests ====");
                    NatureBuilder.Prepare(data, shapes, olibs);

                    Console.WriteLine("==== Scrub ====");
                    NatureBuilder.PrepareScrub(data, shapes, olibs);

                    Console.WriteLine("==== WaterwayEdges ====");
                    NatureBuilder.PrepareWaterwayEdges(data, shapes, olibs);

                    Console.WriteLine("==== DefaultAreas ====");
                    DefaultAreasBuilder.PrepareDefaultAreas(data, shapes, olibs);

                    Console.WriteLine("==== Objects ====");
                    PlaceIsolatedObjects(data, olibs, filtered);

                    Console.WriteLine("==== Fences and walls ====");
                    PlaceBarrierObjects(data, db, olibs, filtered);
                }

                if (!options.DoNotGenerateImagery)
                {
                    Console.WriteLine("==== Terrain images ====");
                    TerrainImageBuilder.GenerateTerrainImages(config, area, data, shapes, terrainMaterialLibrary);
                }
            }
        }

        private static void PlaceBarrierObjects(MapData data, SnapshotDb db, ObjectLibraries olibs, OsmStreamSource filtered)
        {
            var result = new TerrainObjectLayer(data.MapInfos);

            PlaceObjectsOnPathRightAngle(result,
                data,
                db,
                filtered,
                olibs.Libraries.Where(l => l.Category == ObjectCategory.Wall).ToList(),
                o => OsmCategorizer.Get(o.Tags, "barrier") == "wall");

            PlaceObjectsOnPath(result,
                data,
                db,
                filtered,
                olibs.Libraries.Where(l => l.Category == ObjectCategory.Fence).ToList(),
                o => OsmCategorizer.Get(o.Tags, "barrier") == "fence");

            PlaceObjectsOnPath(result,
                data,
                db,
                filtered,
                olibs.Libraries.Where(l => l.Category == ObjectCategory.MilitaryWall).ToList(),
                o => OsmCategorizer.Get(o.Tags, "barrier") == "city_wall");

            result.WriteFile(Path.Combine(data.Config.Target.Objects, "barriers.rel.txt"));
        }

        private static void PlaceObjectsOnPath(TerrainObjectLayer result, MapData data, SnapshotDb db, OsmStreamSource filtered, List<ObjectLibrary> libs, Func<Way, bool> filter)
        {
            if (libs.Count == 0)
            {
                return;
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
                            FollowPathWithObjects.PlaceOnPathRegular(random, lib, result, pathSegment.Points);
                        }
                    }
                }
            }
        }


        private static void PlaceObjectsOnPathRightAngle(TerrainObjectLayer result, MapData data, SnapshotDb db, OsmStreamSource filtered, List<ObjectLibrary> libs, Func<Way, bool> filter)
        {
            if (libs.Count == 0)
            {
                return;
            }
            // Avoid walls within buildings, but tolerate 25cm collision
            var buildingsExclusionZone = data.Buildings.Select(b => b.Box).OfType<BoundingBox>().SelectMany(c => c.Polygon.Offset(-0.25f)).ToList();

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
                        foreach (var pathSegment in path.ClippedBy(cliparea).SelectMany(s => s.SubstractAll(buildingsExclusionZone)))
                        {
                            var random = new Random((int)Math.Truncate(pathSegment.FirstPoint.X + pathSegment.FirstPoint.Y));
                            var lib = libs.Count == 1 ? libs[0] : libs[random.Next(0, libs.Count)];
                            FollowPathWithObjects.PlaceOnPathRightAngle(lib, result, pathSegment.Points);
                        }
                    }
                }
            }
        }

        private static void DownloadOsmData(MapInfos area, string cacheFileName, GenerateOptions options)
        {
            Console.WriteLine("==== Download OSM data ====");
            if (!File.Exists(cacheFileName) || (File.GetLastWriteTimeUtc(cacheFileName) < DateTime.UtcNow.AddDays(-1) && !options.DoNotUpdateOSM))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(cacheFileName));
                var lonWestern = (float)Math.Min(area.SouthWest.Longitude.ToDouble(), area.NorthWest.Longitude.ToDouble());
                var latNorther = (float)Math.Max(area.NorthEast.Latitude.ToDouble(), area.NorthWest.Latitude.ToDouble());
                var lonEastern = (float)Math.Max(area.SouthEast.Longitude.ToDouble(), area.NorthEast.Longitude.ToDouble());
                var latSouthern = (float)Math.Min(area.SouthEast.Latitude.ToDouble(), area.SouthWest.Latitude.ToDouble());
                var uri = FormattableString.Invariant($"https://overpass-api.de/api/map?bbox={lonWestern},{latSouthern},{lonEastern},{latNorther}");
                Console.WriteLine($"Download file from '{uri}'.");
                using (var client = new WebClient())
                {
                    client.DownloadFile(uri, cacheFileName);
                }
                Console.WriteLine($"Done.");
            }
            else
            {
                Console.WriteLine("Keep existing OSM data (data is kept for 1 day by default).");
            }
        }

        private static string GetGridZone(CoordinateSharp.Coordinate coordinate)
        {
            return $"{coordinate.MGRS.LongZone}{coordinate.MGRS.LatZone} {coordinate.MGRS.Digraph}";
        }


        private static void RenderMapInfos(MapConfig config, MapInfos area, TerrainMaterialLibrary terrainMaterialLibrary)
        {
            var sb = new StringBuilder();

            var center = area.TerrainToLatLong(area.Width / 2, area.Height / 2);
            /*
            var sw = GetGridZone(area.SouthWest);
            var se = GetGridZone(area.SouthEast);
            var nw = GetGridZone(area.NorthWest);
            var ne = GetGridZone(area.NorthEast);
            */

            var material = terrainMaterialLibrary.GetInfo(TerrainMaterial.Default, config.Terrain);

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
	satellite = ""{config.PboPrefix}\data\satout_ca.paa"";
    class Layers
    {{
		class Layer0
        {{
			nopx    = ""{material.NormalTexture}"";
			texture = ""{material.ColorTexture}""; 
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
        zoomMax = {1536d / area.Width:0.0000};
        format = ""XY"";
        formatX = ""000"";
        formatY = ""000"";
        stepX = 100;
        stepY = -100;
    }};
    class Zoom2 {{
        zoomMax = {15360d / area.Width:0.0000};
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

            File.WriteAllText(Path.Combine(config.Target.Cooked, "mapinfos.hpp"), sb.ToString());
        }


        private static void RenderCitiesNames(MapConfig config, MapInfos area, OsmStreamSource filtered)
        {
            var places = filtered.Where(o => o.Type == OsmGeoType.Node && o.Tags != null && o.Tags.ContainsKey("place")).ToList();

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

            File.WriteAllText(Path.Combine(config.Target.Cooked, "names.hpp"), sb.ToString());
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

            result.WriteFile(Path.Combine(data.Config.Target.Objects, "objects.rel.txt"));
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
