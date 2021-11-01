class CfgPatches
{
	class arm_gossi
	{
		units[] = {};
		weapons[] = {};
		requiredVersion = 1;
		requiredAddons[] =
		{
			"A3_Map_Stratis",
			"arm_common"
		};
	};
};
 
class CfgWorldList
{
	class gossi{};
};
 
class CfgWorlds
{
	class arm_world_sahel;
	
	class gossi: arm_world_sahel
	{
		cutscenes[] = {};
		description = "GOSSI, Mali";
		worldName = "z\arm\addons\gossi\gossi.wrp";
		author = "1er GTD";
		icon = "";
		previewVideo = "";
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
		/*
		class DefaultClutter;
		class clutter {
			#include "..\common\data\gdt\Clutter.hpp"
		};
		*/
		#include"mapinfos.hpp"
		
		loadingTexts[]={
			"Gossi est une ville et une commune du Mali, dans le cercle de Gourma-Rharous et la région de Tombouctou, située à 160 km au sud-ouest de Gao.",
			"Gossi est une zone d’élevage nomade. Chaque dimanche, il y a un important marché aux animaux, principalement des dromadaires.",
			"Depuis 1987, sœur Anne-Marie Salomon, une religieuse-médecin y a installé à Gossi un hôpital pour nomades unique au Mali : les nomades peuvent y installer leur tente pour la durée des soins.",
			"En avril 2019, les forces françaises de l'opération Barkhane décident d'installer à Gossi une base opérationnelle avancée tactique afin de lutter dans le Gourma contre les éléments terroristes et djihadistes de la région et de soutenir les troupes des FAMa et celles du G5 Sahel.",
			"La commune de Gossi comprend environ 31 villages et au recensement de 2009, elle comptait 24 521 habitants.",
			"La majorité de la population de Gossi est constituée de pasteurs nomades mais il existe des établissements permanents autour du lac Gossi, du lac Ebanguemalène et du lac Agoufou.",
			"Gossi a été le site de la capture de Mimi Ould Baba Ould El Mokhtar, soupçonné d'être responsable d'un attentat terroriste à Grand-Bassam, par les forces militaires françaises."
		};
		
		pictureMap = "z\arm\addons\gossi\data\pictureMap_ca.paa";
	};
};