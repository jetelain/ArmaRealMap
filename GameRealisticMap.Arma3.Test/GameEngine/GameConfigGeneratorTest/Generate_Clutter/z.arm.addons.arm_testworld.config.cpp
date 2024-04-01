class CfgPatches
{
	class arm_arm_testworld
	{
		units[] = {};
		weapons[] = {};
		requiredVersion = 1;
		requiredAddons[] = { "arm_centraleurope" };
	};
};
class CfgWorldList
{
	class arm_testworld{};
};
class CfgWorlds
{
	class arm_world_centraleurope;
	class arm_testworld: arm_world_centraleurope
	{
		cutscenes[] = {};
		description = "arm_testworld, GameRealisticMap";
		worldName = "z\arm\addons\arm_testworld\arm_testworld.wrp";
		author = "";
		icon = "";
		previewVideo = "";
		pictureShot = "";
		newRoadsShape = "z\arm\addons\arm_testworld\data\roads\roads.shp";
		ilsDirection[] = { 0, 0.08, 1 };
		ilsPosition[] = {0, 0};
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
		minTreesInForestSquare = 5;
		minRocksInRockSquare = 10;
		class Subdivision{};
		class Names {
			#include "names.hpp"
		};
        class DefaultClutter;
		class clutter {
			#include "clutter.hpp"
		};
		#include"mapinfos.hpp"
		#include"ace-weather.hpp"
		loadingTexts[]={
			"Loading..."
		};
		pictureMap = "z\arm\addons\arm_testworld\data\picturemap_ca.paa";
	};
};
#include "surfaces.hpp"
