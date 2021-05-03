class CfgPatches
{
	class arm_belfort
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
	class arm_belfort{};
};
 
class CfgWorlds
{
	class Stratis;
	
	class belfort: Stratis
	{
		cutscenes[] = {};
		description = "BELFORT, France";
		worldName = "z\arm\addons\belfort\belfort.wrp";
		author = "GrueArbre";
		icon = "";
		previewVideo = "";
		pictureMap = "";
		pictureShot = "";
 
		newRoadsShape = "z\arm\addons\belfort\data\roads\roads.shp";
		
		centerPosition[] =
		{
			10240, 10240
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
		class Names{};
	};
};