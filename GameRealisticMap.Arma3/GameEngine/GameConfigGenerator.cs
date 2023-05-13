﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.IO;
using GameRealisticMap.Reporting;

namespace GameRealisticMap.Arma3.GameEngine
{
    internal class GameConfigGenerator
    {
        private readonly IArma3RegionAssets assets;
        private readonly IGameFileSystemWriter gameFileSystemWriter;

		public GameConfigGenerator(IArma3RegionAssets assets, IGameFileSystemWriter gameFileSystemWriter)
        {
            this.assets = assets;
            this.gameFileSystemWriter = gameFileSystemWriter;
        }

        public void Generate(IArma3MapConfig config, IContext context, ITerrainArea area)
        {
            var configCpp = $"{config.PboPrefix}\\config.cpp";

            if (!gameFileSystemWriter.FileExists(configCpp))
            {
                gameFileSystemWriter.WriteTextFile(configCpp, GenerateConfigCpp(config));
            }

            gameFileSystemWriter.WriteTextFile($"{config.PboPrefix}\\mapinfos.hpp", GenerateMapInfos(config, area));

            gameFileSystemWriter.WriteTextFile($"{config.PboPrefix}\\names.hpp", string.Empty); // TODO !
        }

        private string GenerateMapInfos(IArma3MapConfig config, ITerrainArea area)
        {
            var center = area.TerrainPointToLatLng(new Geometries.TerrainPoint(config.SizeInMeters / 2, config.SizeInMeters / 2));
            var southWest = area.TerrainPointToLatLng(new Geometries.TerrainPoint(0, 0));
            var northEast = area.TerrainPointToLatLng(new Geometries.TerrainPoint(config.SizeInMeters, config.SizeInMeters));
            var material = assets.Materials.GetMaterialByUsage(TerrainMaterialUsage.Default);

            var centerUTM = new CoordinateSharp.Coordinate(center.Y, center.X).UTM;

            return FormattableString.Invariant(@$"
latitude={center.Y:0.00000000};
longitude={center.X:0.0000000};

mapArea[] = {{
    {southWest.Y:0.00000000}, {southWest.X:0.0000000}, //Bottom Left => SW
	{northEast.Y:0.00000000}, {northEast.X:0.0000000} //Top Right => NE
}}; 
mapSize={config.SizeInMeters};
mapZone={centerUTM.LongZone};

centerPosition[]={{{config.SizeInMeters / 2},{config.SizeInMeters / 2}}};

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
    offsetY = {config.SizeInMeters};
    class Zoom1 {{
        zoomMax = {1536d / config.SizeInMeters:0.0000};
        format = ""XY"";
        formatX = ""000"";
        formatY = ""000"";
        stepX = 100;
        stepY = -100;
    }};
    class Zoom2 {{
        zoomMax = {15360d / config.SizeInMeters:0.0000};
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
");
        }

        private string GenerateConfigCpp(IArma3MapConfig config)
        {
            return $@"class CfgPatches
{{
	class arm_{config.WorldName}
	{{
		units[] = {{}};
		weapons[] = {{}};
		requiredVersion = 1;
		requiredAddons[] = {{ ""{assets.BaseDependency}"" }};
	}};
}};
class CfgWorldList
{{
	class {config.WorldName}{{}};
}};
class CfgWorlds
{{
	class {assets.BaseWorldName};
	class {config.WorldName}: {assets.BaseWorldName}
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
        }
    }
}