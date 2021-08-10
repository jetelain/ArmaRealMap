class CfgPatches
{
	class arm_gossi
	{
		units[] = {};
		weapons[] = {};
		requiredVersion = 1;
		requiredAddons[] =
		{
			"A3_Map_Stratis"
		};
	};
};
 
class CfgWorldList
{
	class gossi{};
};
 
class CfgWorlds
{
	class Stratis;
	
	class gossi: Stratis
	{
		cutscenes[] = {};
		description = "GOSSI, Mali";
		worldName = "z\arm\addons\gossi\gossi.wrp";
		author = "1er GTD";
		icon = "";
		previewVideo = "";
		pictureMap = "";
		pictureShot = "";
 
		newRoadsShape = "z\arm\addons\gossi\data\roads\roads.shp";
		
		centerPosition[] =
		{
			40960, 40960
		};
		ilsDirection[] =
		{
			0, 0.08, 1
		};
		ilsPosition[] =
		{
			0, 0
		};
		ilsTaxiIn[] = {};
		ilsTaxiOff[] = {};
		drawTaxiway = 0;
		class SecondaryAirports{};
		class ReplaceObjects{};
 
		class Sounds
		{
			sounds[] = {};
		};
 
		class Animation
		{
			vehicles[] = {};
		};
 
		minTreesInForestSquare = 2;
		minRocksInRockSquare = 2;
 
		class Subdivision{};
		class Names{
			#include "names.hpp"
		};
		class DefaultClutter;
		class clutter {
			#include "data\gdt\Clutter.hpp"
		};
	};
};

#include "data\gdt\CfgSurfaces.hpp"