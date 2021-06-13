#include "script_component.hpp"

class CfgPatches {
    class ADDON {
        name = QUOTE(COMPONENT);
        units[] = {};
        weapons[] = {};
        requiredVersion = REQUIRED_VERSION;
        requiredAddons[] = {"ams_main", "A3_3DEN"};
        author = "AUTHOR";
        VERSION_CONFIG;
    };
};

class ctrlMenuStrip;
class display3DEN
{
	class Controls
	{
		class MenuStrip: ctrlMenuStrip
		{
			class Items
			{
				class Tools
				{
					items[] += {"AMS_Export"};
				};
				class AMS_Export
				{
					text = "Export to Arma Map Studio";
					action = QUOTE([] spawn FUNC(export););
				};
			};
		};
	};
};


class Cfg3DEN
{
	class Object
	{
		class AttributeCategories
		{
			class StateSpecial
			{
				class Attributes
				{
					class AMS_Exclude
					{
						displayName="Exclude from Arma Map Studio Export";
						property="AMS_Exclude";
						control="Checkbox";
						expression="";
						defaultValue="false";
					};
				};
			};
		};
	};
};

class CfgVehicles {
	
	class All;
	
    class Static: All 
	{
		class AmsEden
		{
			surfacenormal = 1;
		};
	};
	
    class Building: Static 
	{
		class AmsEden
		{
			surfacenormal = 0;
		};
	};

	class Wall_F;
	class Land_New_WiredFence_5m_F: Wall_F 
	{
		class AmsEden
		{
			surfacenormal = 1;
			canexport = 1;
		};
    };
    class Land_New_WiredFence_10m_F: Wall_F 
	{
		class AmsEden
		{
			surfacenormal = 1;
			canexport = 1;
		};
    };
	
	class TargetBootcampHumanSimple_F;
	class TargetBootcampHuman_F: TargetBootcampHumanSimple_F 
	{
		class AmsEden
		{
			canexport = 1;
		};
	};
	
	class AllVehicles: All 
	{
		class AmsEden
		{
			canexport = -1;
		};
    };
};

#include "CfgEventHandlers.hpp"


