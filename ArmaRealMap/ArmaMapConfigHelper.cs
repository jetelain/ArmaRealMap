using System;
using System.IO;
using System.Linq;
using System.Text;
using ArmaRealMap.GroundTextureDetails;
using ArmaRealMap.TerrainData.GroundDetailTextures;
using OsmSharp;
using OsmSharp.Streams;

namespace ArmaRealMap
{
    internal class ArmaMapConfigHelper
    {

		internal static void GenerateConfigCppIfMissing(MapConfig config, string target)
		{
            var configCpp = Path.Combine(target, "config.cpp");
            if (!File.Exists(configCpp))
            {
                Console.WriteLine($"Generate '{configCpp}'.");
                GenerateConfigCpp(config, configCpp);
            }
            else
            {
                Console.WriteLine($"File '{configCpp}' exists.");
            }
		}

        private static void GenerateConfigCpp(MapConfig config, string configCpp)
        {
            var configFile = $@"class CfgPatches
{{
	class arm_{config.WorldName}
	{{
		units[] = {{}};
		weapons[] = {{}};
		requiredVersion = 1;
		requiredAddons[] = {{ ""arm_common"" }};
	}};
}};
class CfgWorldList
{{
	class {config.WorldName}{{}};
}};
class CfgWorlds
{{
	class arm_world_{config.Terrain.ToString().ToLowerInvariant()};
	class {config.WorldName}: arm_world_{config.Terrain.ToString().ToLowerInvariant()}
	{{
		cutscenes[] = {{}};
		description = ""{config.WorldName}, ArmaRealMap"";
		worldName = ""{config.PboPrefix}\{config.WorldName}.wrp"";
		author = """";
		icon = """";
		previewVideo = """";
		pictureShot = """";
		newRoadsShape = ""{config.PboPrefix}\data\roads\roads.shp"";
		ilsDirection[] = {{ 0, 0.08, 1 }};
		ilsPosition[] = {{0, 0}};
		ilsTaxiIn[] = {{}};
		ilsTaxiOff[] = {{}};
		drawTaxiway = 0;
		class SecondaryAirports{{}};
		class ReplaceObjects{{}};
		class Sounds
		{{
			sounds[] = {{}};
		}};
		class Animation
		{{
			vehicles[] = {{}};
		}};
		minTreesInForestSquare = 5;
		minRocksInRockSquare = 10;
		class Subdivision{{}};
		class Names{{
			#include ""names.hpp""
		}};
		#include""mapinfos.hpp""
		loadingTexts[]={{
			""Loading...""
		}};
		pictureMap = ""{config.PboPrefix}\data\picturemap_ca.paa"";
	}};
}};";
            File.WriteAllText(configCpp, configFile);
        }

        internal static void RenderMapInfos(MapConfig config, MapInfos area, TerrainMaterialLibrary terrainMaterialLibrary)
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

centerPosition[]={{{area.Width / 2},{area.Height / 2}}};

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


        internal static void RenderCitiesNames(MapConfig config, MapInfos area, OsmStreamSource filtered)
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
    }
}
